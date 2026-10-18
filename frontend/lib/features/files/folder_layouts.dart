import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/browser/local_store.dart';

enum FileLayout { list, grid }

@immutable
class FolderLayouts {
  const FolderLayouts({this.fallback = FileLayout.list, this.overrides = const {}});

  factory FolderLayouts.decode(String? source) {
    if (_tryDecode(source) case {_fallbackKey: final fallback, _overridesKey: final Map<String, Object?> overrides}) {
      return FolderLayouts(
        fallback: _layoutOf(fallback) ?? FileLayout.list,
        overrides: {
          for (final MapEntry(:key, :value) in overrides.entries)
            key: ?_layoutOf(value),
        },
      );
    }
    return const FolderLayouts();
  }

  static const maximumOverrides = 500;
  static const _fallbackKey = 'fallback';
  static const _overridesKey = 'overrides';

  final FileLayout fallback;
  final Map<String, FileLayout> overrides;

  FileLayout of(String folderId) => overrides[folderId] ?? fallback;

  FolderLayouts toggled(String folderId) {
    final next = of(folderId) == FileLayout.list ? FileLayout.grid : FileLayout.list;
    final entries = [
      ...overrides.entries.where((entry) => entry.key != folderId),
      MapEntry(folderId, next),
    ];
    final kept = entries.skip(entries.length > maximumOverrides ? entries.length - maximumOverrides : 0);
    return FolderLayouts(fallback: next, overrides: Map.fromEntries(kept));
  }

  String encode() => jsonEncode({
    _fallbackKey: fallback.name,
    _overridesKey: {for (final MapEntry(:key, :value) in overrides.entries) key: value.name},
  });

  static Object? _tryDecode(String? source) {
    if (source == null) {
      return null;
    }
    try {
      return jsonDecode(source);
    } on FormatException {
      return null;
    }
  }

  static FileLayout? _layoutOf(Object? name) => name is String ? FileLayout.values.asNameMap()[name] : null;
}

final folderLayoutsProvider = NotifierProvider<FolderLayoutsController, FolderLayouts>(FolderLayoutsController.new);

class FolderLayoutsController extends Notifier<FolderLayouts> {
  static const _storageKey = 'guardou.folderLayouts';

  @override
  FolderLayouts build() => FolderLayouts.decode(ref.read(localStoreProvider).read(_storageKey));

  void toggle(String folderId) {
    state = state.toggled(folderId);
    ref.read(localStoreProvider).write(_storageKey, state.encode());
  }
}
