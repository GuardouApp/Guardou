import 'package:flutter/material.dart';

import '../../api/generated/models/item_entry.dart';
import '../../core/layout/window_size_class.dart';
import '../../l10n/generated/app_localizations.dart';
import 'item_action.dart';

Future<ItemAction?> showItemActions(
  BuildContext context, {
  required List<ItemEntry> entries,
  required Offset position,
  List<ItemAction> actions = ItemAction.values,
}) {
  final l10n = AppLocalizations.of(context);
  if (WindowSizeClass.of(context) == WindowSizeClass.compact) {
    return showModalBottomSheet<ItemAction>(
      context: context,
      showDragHandle: true,
      builder: (context) => SafeArea(
        child: ListView(
          shrinkWrap: true,
          children: [
            for (final action in actions)
              ListTile(
                leading: Icon(action.icon),
                title: Text(action.label(l10n)),
                enabled: action.isEnabledFor(entries),
                onTap: () => Navigator.of(context).pop(action),
              ),
          ],
        ),
      ),
    );
  }
  return showMenu<ItemAction>(
    context: context,
    position: menuPositionAt(context, position),
    items: [
      for (final action in actions)
        PopupMenuItem(
          value: action,
          enabled: action.isEnabledFor(entries),
          child: Row(
            children: [
              Icon(action.icon, size: 18),
              const SizedBox(width: 12),
              Text(action.label(l10n)),
            ],
          ),
        ),
    ],
  );
}

RelativeRect menuPositionAt(BuildContext context, Offset globalPosition) {
  final overlay = Navigator.of(context).overlay?.context.findRenderObject();
  if (overlay is! RenderBox) {
    return RelativeRect.fill;
  }
  return RelativeRect.fromRect(overlay.globalToLocal(globalPosition) & Size.zero, Offset.zero & overlay.size);
}

Offset anchorPosition(BuildContext anchor) {
  final box = anchor.findRenderObject();
  return box is RenderBox ? box.localToGlobal(box.size.bottomLeft(Offset.zero)) : Offset.zero;
}
