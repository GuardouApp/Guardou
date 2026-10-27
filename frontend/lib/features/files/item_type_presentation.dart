import 'package:flutter/material.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_type.dart';
import '../../app/theme/item_type_colors.dart';
import '../../l10n/generated/app_localizations.dart';

IconData itemTypeIcon(ItemType type) => switch (type) {
  ItemType.folder => LucideIcons.folder,
  ItemType.image => LucideIcons.fileImage,
  ItemType.video => LucideIcons.fileVideo,
  ItemType.audio => LucideIcons.fileAudio,
  ItemType.pdf => LucideIcons.fileText,
  ItemType.document => LucideIcons.fileType,
  ItemType.spreadsheet => LucideIcons.fileSpreadsheet,
  ItemType.presentation => LucideIcons.presentation,
  ItemType.archive => LucideIcons.fileArchive,
  ItemType.text => LucideIcons.fileCode,
  ItemType.other || ItemType.$unknown => LucideIcons.file,
};

String itemTypeLabel(ItemType type, AppLocalizations l10n) => switch (type) {
  ItemType.folder => l10n.itemTypeFolder,
  ItemType.image => l10n.itemTypeImage,
  ItemType.video => l10n.itemTypeVideo,
  ItemType.audio => l10n.itemTypeAudio,
  ItemType.pdf => l10n.itemTypePdf,
  ItemType.document => l10n.itemTypeDocument,
  ItemType.spreadsheet => l10n.itemTypeSpreadsheet,
  ItemType.presentation => l10n.itemTypePresentation,
  ItemType.archive => l10n.itemTypeArchive,
  ItemType.text => l10n.itemTypeText,
  ItemType.other || ItemType.$unknown => l10n.itemTypeOther,
};

class ItemTypeIcon extends StatelessWidget {
  const ItemTypeIcon(this.type, {super.key, this.size});

  final ItemType type;
  final double? size;

  @override
  Widget build(BuildContext context) =>
      Icon(itemTypeIcon(type), size: size, color: ItemTypeColors.on(context).of(type));
}
