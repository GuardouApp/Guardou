import 'package:flutter/material.dart';

import '../../api/generated/models/item_type.dart';

@immutable
class ItemTypeColors extends ThemeExtension<ItemTypeColors> {
  const ItemTypeColors(this._colors);

  factory ItemTypeColors.from(Brightness brightness, Color seed) => ItemTypeColors({
    for (final MapEntry(key: type, value: hue) in {..._hues, ItemType.folder: seed}.entries)
      type: ColorScheme.fromSeed(seedColor: hue, brightness: brightness).primary,
  });

  static const _hues = {
    ItemType.image: Color(0xFF9C27B0),
    ItemType.video: Color(0xFFE91E63),
    ItemType.audio: Color(0xFF009688),
    ItemType.pdf: Color(0xFFF44336),
    ItemType.document: Color(0xFF2196F3),
    ItemType.spreadsheet: Color(0xFF4CAF50),
    ItemType.presentation: Color(0xFFFF9800),
    ItemType.archive: Color(0xFF795548),
    ItemType.text: Color(0xFF607D8B),
    ItemType.other: Color(0xFF9E9E9E),
  };

  final Map<ItemType, Color> _colors;

  Color of(ItemType type) => _colors[type] ?? _colors[ItemType.other] ?? Colors.grey;

  static ItemTypeColors on(BuildContext context) {
    final theme = Theme.of(context);
    return theme.extension<ItemTypeColors>() ?? ItemTypeColors.from(theme.brightness, theme.colorScheme.primary);
  }

  @override
  ItemTypeColors copyWith({Map<ItemType, Color>? colors}) => ItemTypeColors(colors ?? _colors);

  @override
  ItemTypeColors lerp(ItemTypeColors? other, double t) {
    if (other == null) {
      return this;
    }
    return ItemTypeColors({
      for (final type in ItemType.$valuesDefined) type: Color.lerp(of(type), other.of(type), t) ?? of(type),
    });
  }
}
