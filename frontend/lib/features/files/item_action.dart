import 'package:flutter/widgets.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../l10n/generated/app_localizations.dart';

enum ItemAction {
  open(LucideIcons.externalLink),
  download(LucideIcons.download),
  rename(LucideIcons.pencil),
  move(LucideIcons.folderInput),
  copy(LucideIcons.copy),
  details(LucideIcons.info),
  trash(LucideIcons.trash2);

  const ItemAction(this.icon);

  final IconData icon;

  String label(AppLocalizations l10n) => switch (this) {
    open => l10n.actionOpen,
    download => l10n.actionDownload,
    rename => l10n.actionRename,
    move => l10n.actionMove,
    copy => l10n.actionCopy,
    details => l10n.actionDetails,
    trash => l10n.actionTrash,
  };

  bool isEnabledFor(List<ItemEntry> entries) => switch (this) {
    _ when entries.isEmpty => false,
    open || details => entries.length == 1,
    download => true,
    rename => entries.length == 1 && entries.single.permissions.canModify,
    move || copy || trash => entries.every((entry) => entry.permissions.canModify),
  };
}
