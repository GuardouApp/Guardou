import 'package:flutter/material.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../l10n/generated/app_localizations.dart';

const _markColor = Colors.white;

class ItemMarks extends StatelessWidget {
  const ItemMarks({super.key, required this.entry, required this.size, required this.child});

  static const _scale = 0.6;
  static const _minimumSize = 12.0;
  static const _overhang = 0.25;
  static const _spotScale = 1.8;

  final ItemEntry entry;
  final double size;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    if (!entry.starred && !entry.sharedByMe) {
      return child;
    }
    final extent = (size * _scale).clamp(_minimumSize, double.infinity);
    final spot = extent * _spotScale;
    final offset = -extent * _overhang - (spot - extent) / 2;
    return Stack(
      clipBehavior: Clip.none,
      children: [
        child,
        if (entry.sharedByMe)
          PositionedDirectional(
            start: offset,
            bottom: offset,
            child: _SpotMark(
              size: spot,
              child: SharedBadge(size: extent),
            ),
          ),
        if (entry.starred)
          PositionedDirectional(
            end: offset,
            bottom: offset,
            child: _SpotMark(
              size: spot,
              child: StarBadge(size: extent),
            ),
          ),
      ],
    );
  }
}

class PreviewMarks extends StatelessWidget {
  const PreviewMarks({super.key, required this.entry, required this.child});

  static const _size = 24.0;
  static const _inset = 8.0;
  static const _scrimExtent = 72.0;

  final ItemEntry entry;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Stack(
      fit: StackFit.expand,
      children: [
        child,
        if (entry.sharedByMe) ...[
          const PositionedDirectional(
            start: 0,
            bottom: 0,
            child: _CornerScrim(extent: _scrimExtent, corner: AlignmentDirectional.bottomStart),
          ),
          const PositionedDirectional(
            start: _inset,
            bottom: _inset,
            child: SharedBadge(size: _size),
          ),
        ],
        if (entry.starred) ...[
          const PositionedDirectional(
            end: 0,
            bottom: 0,
            child: _CornerScrim(extent: _scrimExtent, corner: AlignmentDirectional.bottomEnd),
          ),
          const PositionedDirectional(
            end: _inset,
            bottom: _inset,
            child: StarBadge(size: _size),
          ),
        ],
      ],
    );
  }
}

class StarBadge extends StatelessWidget {
  const StarBadge({super.key, required this.size});

  final double size;

  @override
  Widget build(BuildContext context) {
    return _MarkIcon(size: size, icon: LucideIcons.star, label: AppLocalizations.of(context).navigationStarred);
  }
}

class SharedBadge extends StatelessWidget {
  const SharedBadge({super.key, required this.size});

  final double size;

  @override
  Widget build(BuildContext context) {
    return _MarkIcon(size: size, icon: LucideIcons.users, label: AppLocalizations.of(context).itemSharedByMe);
  }
}

class _MarkIcon extends StatelessWidget {
  const _MarkIcon({required this.size, required this.icon, required this.label});

  static const _iconScale = 0.75;

  final double size;
  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return SizedBox.square(
      dimension: size,
      child: Center(
        child: Icon(icon, size: size * _iconScale, color: _markColor, semanticLabel: label),
      ),
    );
  }
}

class _CornerScrim extends StatelessWidget {
  const _CornerScrim({required this.extent, required this.corner});

  final double extent;
  final AlignmentDirectional corner;

  @override
  Widget build(BuildContext context) {
    return IgnorePointer(
      child: SizedBox.square(
        dimension: extent,
        child: DecoratedBox(
          decoration: BoxDecoration(
            gradient: RadialGradient(
              center: corner.resolve(Directionality.of(context)),
              radius: 1,
              colors: _scrimColors,
              stops: _scrimStops,
            ),
          ),
        ),
      ),
    );
  }
}

class _SpotMark extends StatelessWidget {
  const _SpotMark({required this.size, required this.child});

  final double size;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return SizedBox.square(
      dimension: size,
      child: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: RadialGradient(radius: 0.5, colors: _scrimColors, stops: _scrimStops),
        ),
        child: Center(child: child),
      ),
    );
  }
}

const _scrimColors = [Color(0x8C000000), Color(0x46000000), Color(0x00000000)];

const _scrimStops = [0.0, 0.5, 1.0];
