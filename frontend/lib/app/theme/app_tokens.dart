import 'dart:ui';

import 'package:flutter/material.dart';

@immutable
class AppTokens extends ThemeExtension<AppTokens> {
  const AppTokens({
    required this.navigationWidth,
    required this.panelRadius,
    required this.panelColor,
    required this.spacing,
  });

  factory AppTokens.from(ColorScheme colorScheme) =>
      AppTokens(navigationWidth: 256, panelRadius: 16, panelColor: colorScheme.surface, spacing: 16);

  final double navigationWidth;
  final double panelRadius;
  final Color panelColor;
  final double spacing;

  static AppTokens of(BuildContext context) {
    final theme = Theme.of(context);
    return theme.extension<AppTokens>() ?? AppTokens.from(theme.colorScheme);
  }

  @override
  AppTokens copyWith({double? navigationWidth, double? panelRadius, Color? panelColor, double? spacing}) => AppTokens(
    navigationWidth: navigationWidth ?? this.navigationWidth,
    panelRadius: panelRadius ?? this.panelRadius,
    panelColor: panelColor ?? this.panelColor,
    spacing: spacing ?? this.spacing,
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
    );
  }
}
