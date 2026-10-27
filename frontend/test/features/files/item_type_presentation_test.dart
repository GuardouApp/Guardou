import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_type.dart';
import 'package:guardou/app/theme/app_theme.dart';
import 'package:guardou/app/theme/item_type_colors.dart';
import 'package:guardou/features/files/item_type_presentation.dart';

void main() {
  for (final brightness in Brightness.values) {
    testWidgets('gives each type its own icon and $brightness color', (tester) async {
      final theme = brightness == Brightness.dark ? AppTheme.dark() : AppTheme.light();
      await tester.pumpWidget(
        MaterialApp(
          theme: theme,
          home: Row(children: [for (final type in ItemType.$valuesDefined) ItemTypeIcon(type)]),
        ),
      );

      final icons = tester.widgetList<Icon>(find.byType(Icon)).toList();
      final colors = theme.extension<ItemTypeColors>();
      expect(colors, isNotNull);
      expect(icons.map((icon) => icon.icon).toSet(), hasLength(ItemType.$valuesDefined.length));
      expect(icons.map((icon) => icon.color), [for (final type in ItemType.$valuesDefined) colors?.of(type)]);
    });
  }
}
