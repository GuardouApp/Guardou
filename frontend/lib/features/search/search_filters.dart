import 'package:flutter/foundation.dart';
import '../../api/generated/models/item_type.dart';

enum SearchModified { today, last7Days, last30Days, thisYear, lastYear }

enum SearchSize { smallerThan1Mb, largerThan1Mb, largerThan100Mb, largerThan1Gb }

@immutable
class SearchFilters {
  const SearchFilters({this.types = const {}, this.modified, this.size, this.starred = false});

  final Set<ItemType> types;
  final SearchModified? modified;
  final SearchSize? size;
  final bool starred;

  bool get isEmpty => types.isEmpty && modified == null && size == null && !starred;

  SearchFilters toggleType(ItemType type) =>
      _copyWith(types: types.contains(type) ? ({...types}..remove(type)) : {...types, type});

  SearchFilters withModified(SearchModified? value) => _copyWith(modified: () => value);

  SearchFilters withSize(SearchSize? value) => _copyWith(size: () => value);

  SearchFilters withStarred(bool value) => _copyWith(starred: value);

  SearchFilters _copyWith({
    Set<ItemType>? types,
    SearchModified? Function()? modified,
    SearchSize? Function()? size,
    bool? starred,
  }) => SearchFilters(
    types: types ?? this.types,
    modified: modified == null ? this.modified : modified(),
    size: size == null ? this.size : size(),
    starred: starred ?? this.starred,
  );

  List<String> tokens(DateTime now) => [
    for (final type in ItemType.values)
      if (types.contains(type)) 'type:${type.json}',
    if (modified case final value?) 'modified:${_modifiedValue(value, now)}',
    if (size case final value?) 'size:${_sizeValue(value)}',
    if (starred) 'is:starred',
  ];

  static String _modifiedValue(SearchModified modified, DateTime now) => switch (modified) {
    SearchModified.today => 'today',
    SearchModified.last7Days => '7d',
    SearchModified.last30Days => '30d',
    SearchModified.thisYear => '${now.year}',
    SearchModified.lastYear => '${now.year - 1}',
  };

  static String _sizeValue(SearchSize size) => switch (size) {
    SearchSize.smallerThan1Mb => '<1MB',
    SearchSize.largerThan1Mb => '>1MB',
    SearchSize.largerThan100Mb => '>100MB',
    SearchSize.largerThan1Gb => '>1GB',
  };

  @override
  bool operator ==(Object other) =>
      other is SearchFilters &&
      setEquals(other.types, types) &&
      other.modified == modified &&
      other.size == size &&
      other.starred == starred;

  @override
  int get hashCode => Object.hash(Object.hashAllUnordered(types), modified, size, starred);
}

String searchQuery(String text, SearchFilters filters, DateTime now) =>
    [text.trim(), ...filters.tokens(now)].where((part) => part.isNotEmpty).join(' ');
