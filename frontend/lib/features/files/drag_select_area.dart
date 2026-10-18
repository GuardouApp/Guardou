import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class ItemKeyRegistry {
  final _keys = <String, GlobalKey>{};

  GlobalKey keyFor(String id) => _keys.putIfAbsent(id, GlobalKey.new);

  Iterable<MapEntry<String, GlobalKey>> get entries => _keys.entries;
}

class DragSelectArea extends StatefulWidget {
  const DragSelectArea({
    super.key,
    required this.registry,
    required this.onSelect,
    required this.onBackgroundTap,
    required this.child,
    this.onBackgroundSecondaryTap,
  });

  final ItemKeyRegistry registry;
  final void Function(Set<String> ids, {required bool additive}) onSelect;
  final VoidCallback onBackgroundTap;
  final ValueChanged<Offset>? onBackgroundSecondaryTap;
  final Widget child;

  @override
  State<DragSelectArea> createState() => _DragSelectAreaState();
}

class _DragSelectAreaState extends State<DragSelectArea> {
  Offset? _start;
  Rect? _rect;
  bool _additive = false;

  void _begin(DragStartDetails details) {
    _start = details.localPosition;
    _additive = HardwareKeyboard.instance.isControlPressed || HardwareKeyboard.instance.isMetaPressed;
  }

  void _update(DragUpdateDetails details) {
    final start = _start;
    if (start == null) {
      return;
    }
    final rect = Rect.fromPoints(start, details.localPosition);
    setState(() => _rect = rect);
    widget.onSelect(_idsWithin(rect), additive: _additive);
  }

  void _end() => setState(() {
    _start = null;
    _rect = null;
  });

  Set<String> _idsWithin(Rect rect) {
    final area = context.findRenderObject();
    if (area is! RenderBox) {
      return const {};
    }
    return {
      for (final MapEntry(key: id, value: key) in widget.registry.entries)
        if (_boundsOf(key, area)?.overlaps(rect) ?? false) id,
    };
  }

  Rect? _boundsOf(GlobalKey key, RenderBox area) {
    final box = key.currentContext?.findRenderObject();
    if (box is! RenderBox || !box.attached || !box.hasSize) {
      return null;
    }
    return box.localToGlobal(Offset.zero, ancestor: area) & box.size;
  }

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    return GestureDetector(
      behavior: HitTestBehavior.translucent,
      supportedDevices: const {PointerDeviceKind.mouse},
      onTap: widget.onBackgroundTap,
      onSecondaryTapUp: switch (widget.onBackgroundSecondaryTap) {
        final onSecondaryTap? => (details) => onSecondaryTap(details.globalPosition),
        null => null,
      },
      onPanStart: _begin,
      onPanUpdate: _update,
      onPanEnd: (_) => _end(),
      onPanCancel: _end,
      child: Stack(
        children: [
          Positioned.fill(child: widget.child),
          if (_rect case final rect?)
            Positioned.fromRect(
              rect: rect,
              child: IgnorePointer(
                child: DecoratedBox(
                  decoration: BoxDecoration(
                    color: colorScheme.primary.withValues(alpha: 0.12),
                    border: Border.all(color: colorScheme.primary),
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }
}
