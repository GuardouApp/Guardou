import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/files/file_presentation.dart';
import 'package:intl/date_symbol_data_local.dart';

void main() {
  final now = DateTime(2026, 10, 18);

  setUpAll(() => initializeDateFormatting('pt_BR'));

  test('shows the day and the month for dates in the current year', () {
    expect(formatCardDate(DateTime(2026, 3, 5), now, 'en'), 'Mar 5');
    expect(formatCardDate(DateTime(2026, 10, 17), now, 'pt_BR'), '17 de out.');
  });

  test('shows only the year for dates in other years', () {
    expect(formatCardDate(DateTime(2024, 3, 5), now, 'en'), '2024');
  });
}
