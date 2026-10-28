import 'package:flutter/material.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../l10n/generated/app_localizations.dart';

class StarMark extends StatelessWidget {
  const StarMark({super.key, required this.entry, required this.size, required this.child});

  static const _scale = 0.6;
  static const _minimumSize = 12.0;
  static const _overhang = 0.25;

  final ItemEntry entry;
  final double size;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    if (!entry.starred) {
      return child;
    }
    final extent = (size * _scale).clamp(_minimumSize, double.infinity);
    final offset = -extent * _overhang;
    return Stack(
      clipBehavior: Clip.none,
      children: [
        child,
        PositionedDirectional(
          end: offset,
          bottom: offset,
          child: StarBadge(size: extent),
        ),
      ],
    );
  }
}

class StarBadge extends StatelessWidget {
  const StarBadge({super.key, required this.size});

  static const _iconScale = 0.75;

  final double size;

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(color: colors.surface, shape: BoxShape.circle),
      alignment: Alignment.center,
      child: Icon(
        LucideIcons.star,
        size: size * _iconScale,
        color: colors.onSurfaceVariant,
        semanticLabel: AppLocalizations.of(context).navigationStarred,
      ),
    );
  }
}
