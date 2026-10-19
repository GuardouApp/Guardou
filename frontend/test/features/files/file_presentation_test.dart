import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_kind.dart';
import 'package:guardou/features/files/file_presentation.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../helpers/test_app.dart';

void main() {
  final now = DateTime(2026, 10, 18);

  setUpAll(() => initializeDateFormatting('pt_BR'));

  test('shows the day and the month for dates in the current year', () {
    expect(formatCardDate(DateTime(2026, 3, 5), now, 'en'), 'Mar 5');
    expect(formatCardDate(DateTime(2026, 10, 17), now, 'pt_BR'), '17 de out.');
  });

  test('shows a magnet icon for torrent files', () {
    expect(iconFor(testEntry('linux.torrent', mimeType: 'application/x-bittorrent')), LucideIcons.magnet);
    expect(iconFor(testEntry('torrent', kind: ItemKind.folder)), LucideIcons.folder);
  });

  test('shows only the year for dates in other years', () {
    expect(formatCardDate(DateTime(2024, 3, 5), now, 'en'), '2024');
  });
}
