import 'package:flutter/material.dart';

import 'app_tokens.dart';
import 'item_type_colors.dart';

abstract final class AppTheme {
  static const _seedColor = Color(0xFF00639B);
  static const _fontFamily = 'Roboto';
  static const _navigationTileHeight = 40.0;
  static const _navigationIconSize = 20.0;

  static const _selectionDark = Color(0xFF004A77);
  static const _selectionLight = Color(0xFFC2E7FF);
  static const _actionContainerDark = Color(0xFF37393B);

  static ThemeData light() => _build(Brightness.light);

  static ThemeData dark() => _build(Brightness.dark);

  static ThemeData _build(Brightness brightness) {
    final colorScheme = _colorScheme(brightness);
    return ThemeData(
      colorScheme: colorScheme,
      fontFamily: _fontFamily,
      scaffoldBackgroundColor: colorScheme.surfaceContainerLow,
      navigationDrawerTheme: const NavigationDrawerThemeData(
        tileHeight: _navigationTileHeight,
        iconTheme: WidgetStatePropertyAll(IconThemeData(size: _navigationIconSize)),
      ),
      extensions: [AppTokens.from(colorScheme), ItemTypeColors.from(brightness, _seedColor)],
    );
  }

  static ColorScheme _colorScheme(Brightness brightness) {
    final accent = ColorScheme.fromSeed(seedColor: _seedColor, brightness: brightness);
    final neutral = ColorScheme.fromSeed(
      seedColor: _seedColor,
      brightness: brightness,
      dynamicSchemeVariant: DynamicSchemeVariant.monochrome,
    );
    final isDark = brightness == Brightness.dark;

    return accent.copyWith(
      surface: neutral.surface,
      surfaceDim: neutral.surfaceDim,
      surfaceBright: neutral.surfaceBright,
      surfaceContainerLowest: neutral.surfaceContainerLowest,
      surfaceContainerLow: neutral.surfaceContainerLow,
      surfaceContainer: neutral.surfaceContainer,
      surfaceContainerHigh: neutral.surfaceContainerHigh,
      surfaceContainerHighest: neutral.surfaceContainerHighest,
      onSurface: neutral.onSurface,
      onSurfaceVariant: neutral.onSurfaceVariant,
      outline: neutral.outline,
      outlineVariant: neutral.outlineVariant,
      inverseSurface: neutral.inverseSurface,
      onInverseSurface: neutral.onInverseSurface,
      secondaryContainer: isDark ? _selectionDark : _selectionLight,
      onSecondaryContainer: isDark ? _selectionLight : _selectionDark,
      primaryContainer: isDark ? _actionContainerDark : null,
      onPrimaryContainer: isDark ? neutral.onSurface : null,
    );
  }
}
