import 'package:flutter/foundation.dart';

@immutable
class Selection {
  const Selection({this.ids = const {}, this.anchor, this.checkboxMode = false});

  static const empty = Selection();

  final Set<String> ids;
  final String? anchor;
  final bool checkboxMode;

  bool get isEmpty => ids.isEmpty;

  bool contains(String id) => ids.contains(id);

  Selection only(String id) => Selection(ids: {id}, anchor: id, checkboxMode: checkboxMode);

  Selection toggle(String id) {
    final next = {...ids};
    if (!next.remove(id)) {
      next.add(id);
    }
    return Selection(ids: next, anchor: id, checkboxMode: checkboxMode && next.isNotEmpty);
  }

  Selection extendTo(String id, List<String> order) {
    final from = order.indexOf(anchor ?? id);
    final to = order.indexOf(id);
    if (from < 0 || to < 0) {
      return only(id);
    }
    final range = order.sublist(from < to ? from : to, (from < to ? to : from) + 1);
    return Selection(ids: range.toSet(), anchor: anchor, checkboxMode: checkboxMode);
  }

  Selection all(List<String> order) =>
      Selection(ids: order.toSet(), anchor: order.isEmpty ? null : order.first, checkboxMode: checkboxMode);

  Selection withCheckboxMode(bool enabled) => Selection(ids: ids, anchor: anchor, checkboxMode: enabled);

  Selection retainOnly(Iterable<String> available) {
    final present = available.toSet();
    final kept = ids.where(present.contains).toSet();
    return Selection(
      ids: kept,
      anchor: present.contains(anchor) ? anchor : null,
      checkboxMode: checkboxMode && kept.isNotEmpty,
    );
  }

  @override
  bool operator ==(Object other) =>
      other is Selection && setEquals(other.ids, ids) && other.anchor == anchor && other.checkboxMode == checkboxMode;

  @override
  int get hashCode => Object.hash(Object.hashAllUnordered(ids), anchor, checkboxMode);
}
