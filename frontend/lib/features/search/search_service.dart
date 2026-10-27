import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/item_entry.dart';
import '../../core/api/api_providers.dart';

final searchServiceProvider = Provider<SearchService>((ref) => SearchService(ref.watch(apiClientProvider)));

class SearchService {
  const SearchService(this._api);

  final ApiClient _api;

  Future<List<ItemEntry>> search(String query, {String? folderId}) =>
      _api.search.searchItems(query: query, folderId: folderId);
}
