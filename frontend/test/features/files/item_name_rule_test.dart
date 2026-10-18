import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/files/item_name_rule.dart';

void main() {
  test('accepts names', () {
    for (final name in ['report.txt', '.env', 'my files', 'relatório 2026']) {
      expect(ItemNameRule.isValid(name), isTrue, reason: name);
    }
  });

  test('refuses names', () {
    for (final name in ['', '.', '..', 'a/b', r'a\b', ' padded', 'padded ', 'tab\there', 'x' * 256]) {
      expect(ItemNameRule.isValid(name), isFalse, reason: name);
    }
  });

  test('finds the base name', () {
    expect(ItemNameRule.baseNameLength('report.txt'), 6);
    expect(ItemNameRule.baseNameLength('.env'), 4);
    expect(ItemNameRule.baseNameLength('folder'), 6);
  });
}
