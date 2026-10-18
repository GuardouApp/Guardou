import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/app/theme/app_theme.dart';

void main() {
  test('dark theme uses neutral surfaces and blue selection colors', () {
    final colors = AppTheme.dark().colorScheme;

    expect(colors.surface, const Color(0xFF131313));
    expect(colors.surfaceContainerLow, const Color(0xFF1B1B1B));
    expect(colors.secondaryContainer, const Color(0xFF004A77));
    expect(colors.onSecondaryContainer, const Color(0xFFC2E7FF));
    expect(colors.primaryContainer, const Color(0xFF37393B));
  });

  test('surfaces have no hue', () {
    for (final theme in [AppTheme.light(), AppTheme.dark()]) {
      final colors = theme.colorScheme;
      for (final color in [colors.surface, colors.surfaceContainerLow, colors.surfaceContainerHigh]) {
        expect(color.r, color.g);
        expect(color.g, color.b);
      }
    }
  });

  test('navigation tiles are compact', () {
    final drawer = AppTheme.light().navigationDrawerTheme;

    expect(drawer.tileHeight, 40);
    expect(drawer.iconTheme?.resolve({})?.size, 20);
  });
}
