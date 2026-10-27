import 'package:flutter/foundation.dart';

enum SearchType { folder, image, video, audio, pdf, document, spreadsheet, presentation, archive }

enum SearchModified { today, last7Days, last30Days, thisYear, lastYear }

enum SearchSize { smallerThan1Mb, largerThan1Mb, largerThan100Mb, largerThan1Gb }

@immutable
class SearchFilters {
  const SearchFilters({this.types = const {}, this.modified, this.size});

  final Set<SearchType> types;
  final SearchModified? modified;
  final SearchSize? size;

  bool get isEmpty => types.isEmpty && modified == null && size == null;

  SearchFilters toggleType(SearchType type) => SearchFilters(
    types: types.contains(type) ? ({...types}..remove(type)) : {...types, type},
    modified: modified,
    size: size,
  );

  SearchFilters withModified(SearchModified? value) => SearchFilters(types: types, modified: value, size: size);

  SearchFilters withSize(SearchSize? value) => SearchFilters(types: types, modified: modified, size: value);

  List<String> tokens(DateTime now) => [
    for (final type in SearchType.values)
      if (types.contains(type)) 'type:${type.name}',
    if (modified case final value?) 'modified:${_modifiedValue(value, now)}',
    if (size case final value?) 'size:${_sizeValue(value)}',
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
      other is SearchFilters && setEquals(other.types, types) && other.modified == modified && other.size == size;

  @override
  int get hashCode => Object.hash(Object.hashAllUnordered(types), modified, size);
}

String searchQuery(String text, SearchFilters filters, DateTime now) =>
    [text.trim(), ...filters.tokens(now)].where((part) => part.isNotEmpty).join(' ');
