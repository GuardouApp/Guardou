import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import 'file_view_settings.dart';

final _chunk = RegExp(r'\d+|\D+');
final _digits = RegExp(r'^\d+$');

int compareNatural(String a, String b) {
  final left = _chunk.allMatches(a.toLowerCase()).map((match) => match[0] ?? '').toList();
  final right = _chunk.allMatches(b.toLowerCase()).map((match) => match[0] ?? '').toList();
  for (var index = 0; index < left.length && index < right.length; index++) {
    final result = _compareChunks(left[index], right[index]);
    if (result != 0) {
      return result;
    }
  }
  final lengthResult = left.length.compareTo(right.length);
  return lengthResult != 0 ? lengthResult : a.compareTo(b);
}

int _compareChunks(String a, String b) {
  if (_digits.hasMatch(a) && _digits.hasMatch(b)) {
    final result = BigInt.parse(a).compareTo(BigInt.parse(b));
    return result != 0 ? result : a.length.compareTo(b.length);
  }
  return a.compareTo(b);
}

String extensionOf(ItemEntry entry) {
  final dot = entry.name.lastIndexOf('.');
  return entry.kind == ItemKind.folder || dot <= 0 ? '' : entry.name.substring(dot + 1).toLowerCase();
}

List<ItemEntry> arrangeEntries(List<ItemEntry> entries, FileViewSettings settings) {
  final direction = settings.ascending ? 1 : -1;
  return [...entries]..sort((a, b) {
    final folderFirst = _folderRank(a).compareTo(_folderRank(b));
    if (folderFirst != 0) {
      return folderFirst;
    }
    final result = _compareBy(settings.sortField, a, b);
    return direction * (result != 0 ? result : compareNatural(a.name, b.name));
  });
}

int _folderRank(ItemEntry entry) => entry.kind == ItemKind.folder ? 0 : 1;

int _compareBy(SortField field, ItemEntry a, ItemEntry b) => switch (field) {
  SortField.name => compareNatural(a.name, b.name),
  SortField.modified => a.modified.compareTo(b.modified),
  SortField.created => a.created.compareTo(b.created),
  SortField.size => a.size.compareTo(b.size),
  SortField.kind => extensionOf(a).compareTo(extensionOf(b)),
};
