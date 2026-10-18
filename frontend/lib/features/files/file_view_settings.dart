import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

enum FileLayout { list, grid }

enum SortField { name, modified, created, size, kind }

@immutable
class FileViewSettings {
  const FileViewSettings({this.layout = FileLayout.list, this.sortField = SortField.name, this.ascending = true});

  final FileLayout layout;
  final SortField sortField;
  final bool ascending;

  FileViewSettings copyWith({FileLayout? layout, SortField? sortField, bool? ascending}) => FileViewSettings(
    layout: layout ?? this.layout,
    sortField: sortField ?? this.sortField,
    ascending: ascending ?? this.ascending,
  );
}

final fileViewSettingsProvider = NotifierProvider<FileViewSettingsController, FileViewSettings>(
  FileViewSettingsController.new,
);

class FileViewSettingsController extends Notifier<FileViewSettings> {
  @override
  FileViewSettings build() => const FileViewSettings();

  void toggleLayout() =>
      state = state.copyWith(layout: state.layout == FileLayout.list ? FileLayout.grid : FileLayout.list);

  void sortBy(SortField field) => state = field == state.sortField
      ? state.copyWith(ascending: !state.ascending)
      : state.copyWith(sortField: field, ascending: true);
}
