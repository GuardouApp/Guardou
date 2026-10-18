import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

enum SortField { name, modified, created, size, kind }

@immutable
class FileViewSettings {
  const FileViewSettings({this.sortField = SortField.name, this.ascending = true});

  final SortField sortField;
  final bool ascending;

  FileViewSettings copyWith({SortField? sortField, bool? ascending}) =>
      FileViewSettings(sortField: sortField ?? this.sortField, ascending: ascending ?? this.ascending);
}

final fileViewSettingsProvider = NotifierProvider<FileViewSettingsController, FileViewSettings>(
  FileViewSettingsController.new,
);

class FileViewSettingsController extends Notifier<FileViewSettings> {
  @override
  FileViewSettings build() => const FileViewSettings();

  void sortBy(SortField field) => state = field == state.sortField
      ? state.copyWith(ascending: !state.ascending)
      : state.copyWith(sortField: field, ascending: true);
}
