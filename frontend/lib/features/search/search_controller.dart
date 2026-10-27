import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_entry.dart';
import 'search_filters.dart';
import 'search_service.dart';

final itemSearchControllerProvider = NotifierProvider<ItemSearchController, ItemSearchState>(ItemSearchController.new);

@immutable
class ItemSearchState {
  const ItemSearchState({
    this.text = '',
    this.filters = const SearchFilters(),
    this.folderId,
    this.folderOnly = false,
    this.results,
  });

  final String text;
  final SearchFilters filters;
  final String? folderId;
  final bool folderOnly;
  final AsyncValue<List<ItemEntry>>? results;

  bool get isReady => text.trim().length >= ItemSearchController.minimumLength || !filters.isEmpty;

  String? get scopeFolderId => folderOnly ? folderId : null;

  ItemSearchState copyWith({
    String? text,
    SearchFilters? filters,
    String? Function()? folderId,
    bool? folderOnly,
    AsyncValue<List<ItemEntry>>? Function()? results,
  }) => ItemSearchState(
    text: text ?? this.text,
    filters: filters ?? this.filters,
    folderId: folderId == null ? this.folderId : folderId(),
    folderOnly: folderOnly ?? this.folderOnly,
    results: results == null ? this.results : results(),
  );
}

class ItemSearchController extends Notifier<ItemSearchState> {
  static const minimumLength = 2;
  static const debounce = Duration(milliseconds: 300);

  Timer? _timer;
  int _generation = 0;

  @override
  ItemSearchState build() {
    ref.onDispose(() => _timer?.cancel());
    return const ItemSearchState();
  }

  void setText(String text) {
    if (text != state.text) {
      _update(state.copyWith(text: text));
    }
  }

  void setFolder(String? folderId) {
    if (folderId != state.folderId) {
      _update(state.copyWith(folderId: () => folderId, folderOnly: folderId != null && state.folderOnly));
    }
  }

  void setFolderOnly(bool folderOnly) => _update(state.copyWith(folderOnly: folderOnly));

  void toggleType(SearchType type) => _update(state.copyWith(filters: state.filters.toggleType(type)));

  void setModified(SearchModified? modified) => _update(state.copyWith(filters: state.filters.withModified(modified)));

  void setSize(SearchSize? size) => _update(state.copyWith(filters: state.filters.withSize(size)));

  void _update(ItemSearchState next) {
    _timer?.cancel();
    final generation = ++_generation;
    if (!next.isReady) {
      state = next.copyWith(results: () => null);
      return;
    }
    state = next.copyWith(results: () => const AsyncLoading());
    _timer = Timer(debounce, () => _search(next, generation));
  }

  Future<void> _search(ItemSearchState search, int generation) async {
    final query = searchQuery(search.text, search.filters, DateTime.now());
    final results = await AsyncValue.guard(
      () => ref.read(searchServiceProvider).search(query, folderId: search.scopeFolderId),
    );
    if (generation == _generation) {
      state = state.copyWith(results: () => results);
    }
  }
}
