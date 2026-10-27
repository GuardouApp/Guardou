import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_type.dart';
import 'package:guardou/features/search/search_filters.dart';

void main() {
  final now = DateTime(2026, 10, 27);

  test('gives only the text without filters', () {
    expect(searchQuery('  annual report ', const SearchFilters(), now), 'annual report');
  });

  test('changes the filters into the query grammar', () {
    final filters = const SearchFilters()
        .toggleType(ItemType.video)
        .toggleType(ItemType.image)
        .withModified(SearchModified.last7Days)
        .withSize(SearchSize.largerThan100Mb);

    expect(searchQuery('holiday', filters, now), 'holiday type:image type:video modified:7d size:>100MB');
  });

  test('gives the calendar year for the year filters', () {
    expect(const SearchFilters(modified: SearchModified.thisYear).tokens(now), ['modified:2026']);
    expect(const SearchFilters(modified: SearchModified.lastYear).tokens(now), ['modified:2025']);
  });

  test('removes a type when it is toggled again', () {
    final filters = const SearchFilters().toggleType(ItemType.pdf).toggleType(ItemType.pdf);

    expect(filters.isEmpty, isTrue);
    expect(searchQuery('', filters, now), isEmpty);
  });

  test('clears the modified and size filters', () {
    final filters = const SearchFilters(
      modified: SearchModified.today,
      size: SearchSize.smallerThan1Mb,
    ).withModified(null).withSize(null);

    expect(filters, const SearchFilters());
  });
}
