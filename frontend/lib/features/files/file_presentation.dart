import 'dart:math';

import 'package:intl/intl.dart';

import '../../api/generated/models/breadcrumb.dart';
import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../../l10n/generated/app_localizations.dart';
import 'file_arrangement.dart';

const _sizeUnits = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];

String formatSize(int bytes, String locale) {
  final exponent = bytes <= 0 ? 0 : min(log(bytes) ~/ log(1024), _sizeUnits.length - 1);
  final value = bytes / pow(1024, exponent);
  return '${NumberFormat(exponent == 0 ? '#,##0' : '#,##0.#', locale).format(value)} ${_sizeUnits[exponent]}';
}

String formatDate(DateTime date, String locale) => DateFormat.yMMMd(locale).format(date.toLocal());

String formatCardDate(DateTime date, DateTime now, String locale) {
  final local = date.toLocal();
  return local.year == now.year ? DateFormat.MMMd(locale).format(local) : DateFormat.y(locale).format(local);
}

String kindLabel(ItemEntry entry, AppLocalizations l10n) {
  if (entry.kind == ItemKind.folder) {
    return l10n.kindFolder;
  }
  final extension = extensionOf(entry);
  return extension.isEmpty ? l10n.kindFileUnknown : l10n.kindFile(extension.toUpperCase());
}

String folderLabel(ItemEntry folder, AppLocalizations l10n) => folder.name.isEmpty ? l10n.myFiles : folder.name;

String breadcrumbLabel(Breadcrumb crumb, AppLocalizations l10n) => crumb.name.isEmpty ? l10n.myFiles : crumb.name;
