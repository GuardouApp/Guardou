import 'dart:math';

import 'package:flutter/widgets.dart';
import 'package:intl/intl.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/breadcrumb.dart';
import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../../l10n/generated/app_localizations.dart';
import 'file_arrangement.dart';

const _sizeUnits = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];
const _codeExtensions = {
  'cs',
  'dart',
  'js',
  'ts',
  'py',
  'go',
  'java',
  'c',
  'cpp',
  'h',
  'html',
  'css',
  'json',
  'xml',
  'yaml',
  'yml',
  'sh',
};
const _archiveExtensions = {'zip', 'tar', 'gz', 'tgz', '7z', 'rar', 'bz2', 'xz'};
const _spreadsheetExtensions = {'csv', 'xls', 'xlsx', 'ods'};

IconData iconFor(ItemEntry entry) {
  if (entry.kind == ItemKind.folder) {
    return LucideIcons.folder;
  }
  final mimeType = entry.mimeType ?? '';
  final extension = extensionOf(entry);
  return switch (mimeType.split('/').first) {
    'image' => LucideIcons.fileImage,
    'video' => LucideIcons.fileVideo,
    'audio' => LucideIcons.fileAudio,
    _ when _codeExtensions.contains(extension) => LucideIcons.fileCode,
    _ when _archiveExtensions.contains(extension) => LucideIcons.fileArchive,
    _ when _spreadsheetExtensions.contains(extension) => LucideIcons.fileSpreadsheet,
    'text' => LucideIcons.fileText,
    _ when extension == 'pdf' => LucideIcons.fileText,
    _ => LucideIcons.file,
  };
}

String formatSize(int bytes, String locale) {
  final exponent = bytes <= 0 ? 0 : min(log(bytes) ~/ log(1024), _sizeUnits.length - 1);
  final value = bytes / pow(1024, exponent);
  return '${NumberFormat(exponent == 0 ? '#,##0' : '#,##0.#', locale).format(value)} ${_sizeUnits[exponent]}';
}

String formatDate(DateTime date, String locale) => DateFormat.yMMMd(locale).format(date.toLocal());

String kindLabel(ItemEntry entry, AppLocalizations l10n) {
  if (entry.kind == ItemKind.folder) {
    return l10n.kindFolder;
  }
  final extension = extensionOf(entry);
  return extension.isEmpty ? l10n.kindFileUnknown : l10n.kindFile(extension.toUpperCase());
}

String sizeLabel(ItemEntry entry, AppLocalizations l10n) =>
    entry.kind == ItemKind.folder ? l10n.valueMissing : formatSize(entry.size, l10n.localeName);

String folderLabel(ItemEntry folder, AppLocalizations l10n) => folder.parentId == null ? l10n.myFiles : folder.name;

String breadcrumbLabel(Breadcrumb crumb, AppLocalizations l10n) => crumb.name.isEmpty ? l10n.myFiles : crumb.name;
