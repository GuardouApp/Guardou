import 'dart:ui';

import 'package:flutter/material.dart';

@immutable
class AppTokens extends ThemeExtension<AppTokens> {
  const AppTokens({
    required this.navigationWidth,
    required this.panelRadius,
    required this.panelColor,
    required this.spacing,
    required this.success,
    required this.onSuccess,
  });

  factory AppTokens.from(ColorScheme colorScheme) {
    final isDark = colorScheme.brightness == Brightness.dark;
    return AppTokens(
      navigationWidth: 256,
      panelRadius: 16,
      panelColor: colorScheme.surface,
      spacing: 16,
      success: isDark ? _successDark : _successLight,
      onSuccess: isDark ? _onSuccessDark : _onSuccessLight,
    );
  }

  static const _successLight = Color(0xFF146C2E);
  static const _onSuccessLight = Color(0xFFFFFFFF);
  static const _successDark = Color(0xFF6DD58C);
  static const _onSuccessDark = Color(0xFF00391A);

  final double navigationWidth;
  final double panelRadius;
  final Color panelColor;
  final double spacing;
  final Color success;
  final Color onSuccess;

  static AppTokens of(BuildContext context) {
    final theme = Theme.of(context);
    return theme.extension<AppTokens>() ?? AppTokens.from(theme.colorScheme);
  }

  @override
  AppTokens copyWith({
    double? navigationWidth,
    double? panelRadius,
    Color? panelColor,
    double? spacing,
    Color? success,
    Color? onSuccess,
  }) => AppTokens(
    navigationWidth: navigationWidth ?? this.navigationWidth,
    panelRadius: panelRadius ?? this.panelRadius,
    panelColor: panelColor ?? this.panelColor,
    spacing: spacing ?? this.spacing,
    success: success ?? this.success,
    onSuccess: onSuccess ?? this.onSuccess,
  );

  @override
  AppTokens lerp(AppTokens? other, double t) {
    if (other == null) {
      return this;
    }
    return AppTokens(
      navigationWidth: lerpDouble(navigationWidth, other.navigationWidth, t) ?? navigationWidth,
      panelRadius: lerpDouble(panelRadius, other.panelRadius, t) ?? panelRadius,
      panelColor: Color.lerp(panelColor, other.panelColor, t) ?? panelColor,
      spacing: lerpDouble(spacing, other.spacing, t) ?? spacing,
      success: Color.lerp(success, other.success, t) ?? success,
      onSuccess: Color.lerp(onSuccess, other.onSuccess, t) ?? onSuccess,
    );
  }
}
