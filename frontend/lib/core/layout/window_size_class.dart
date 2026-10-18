import 'package:flutter/widgets.dart';

enum WindowSizeClass {
  compact,
  medium,
  expanded;

  static const _mediumMinWidth = 600.0;
  static const _expandedMinWidth = 840.0;

  static WindowSizeClass fromWidth(double width) {
    if (width >= _expandedMinWidth) {
      return expanded;
    }
    if (width >= _mediumMinWidth) {
      return medium;
    }
    return compact;
  }

  static WindowSizeClass of(BuildContext context) => fromWidth(MediaQuery.sizeOf(context).width);
}
