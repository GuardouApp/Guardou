import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_entry.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/features/files/file_arrangement.dart';
import 'package:guardou/features/files/file_view_settings.dart';

import '../../helpers/test_app.dart';

void main() {
  List<String> names(List<ItemEntry> entries) => [for (final entry in entries) entry.name];

  test('compares names in natural order', () {
    final sorted = ['file10.txt', 'File2.txt', 'file1.txt', 'b', 'a']..sort(compareNatural);

    expect(sorted, ['a', 'b', 'file1.txt', 'File2.txt', 'file10.txt']);
  });

  test('shows folders first and sorts by the selected column', () {
    final entries = [
      testEntry('big.txt', size: 300),
      testEntry('zeta', kind: ItemKind.folder),
      testEntry('small.txt', size: 10),
      testEntry('alpha', kind: ItemKind.folder),
    ];

    expect(names(arrangeEntries(entries, const FileViewSettings())), ['alpha', 'zeta', 'big.txt', 'small.txt']);
    expect(names(arrangeEntries(entries, const FileViewSettings(sortField: SortField.size, ascending: false))), [
      'zeta',
      'alpha',
      'big.txt',
      'small.txt',
    ]);
  });

  test('sorting the same column again changes the direction', () {
    final container = ProviderContainer();
    addTearDown(container.dispose);
    final controller = container.read(fileViewSettingsProvider.notifier);

    controller.sortBy(SortField.size);
    expect(container.read(fileViewSettingsProvider).ascending, isTrue);
    controller.sortBy(SortField.size);
    expect(container.read(fileViewSettingsProvider).ascending, isFalse);
    controller.sortBy(SortField.name);
    expect(container.read(fileViewSettingsProvider).sortField, SortField.name);
    expect(container.read(fileViewSettingsProvider).ascending, isTrue);
  });
}
