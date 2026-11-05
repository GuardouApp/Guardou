import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/conflict_policy.dart';
import '../../api/generated/models/item_details.dart';
import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_selection.dart';
import '../../api/generated/models/name_request.dart';
import '../../api/generated/models/storage_usage.dart';
import '../../api/generated/models/transfer_request.dart';
import '../../core/api/api_providers.dart';
import 'file_source.dart';

final filesServiceProvider = Provider<FilesService>(
  (ref) => FilesService(ref.watch(apiClientProvider), ref.watch(dioProvider), Uri.parse(ref.watch(apiBaseUrlProvider))),
);

class FilesService extends FileSource {
  const FilesService(this._api, super.dio, super.baseUri);

  final ApiClient _api;

  Future<ItemDetails> item(String id) => _api.items.getItem(id: id);

  Future<List<ItemEntry>> children(String id) => _api.items.getItemChildren(id: id);

  Future<List<ItemEntry>> recent() => _api.recentItems.getRecentItems();

  Future<List<ItemEntry>> starred() => _api.itemStars.getStarredItems();

  Future<void> star(List<String> ids) => _api.itemStars.starItems(body: ItemSelection(ids: ids));

  Future<void> unstar(List<String> ids) => _api.itemStars.unstarItems(body: ItemSelection(ids: ids));

  Future<StorageUsage> usage() => _api.items.getStorageUsage();

  Future<ItemEntry> createFolder(String parentId, String name) => _api.items.createFolder(
    id: parentId,
    body: NameRequest(name: name),
  );

  Future<ItemEntry> rename(String id, String name) => _api.items.renameItem(
    id: id,
    body: NameRequest(name: name),
  );

  Future<ItemEntry> move(String id, String destinationId, ConflictPolicy conflict) => _api.items.moveItem(
    id: id,
    body: TransferRequest(destinationId: destinationId, conflict: conflict),
  );

  Future<ItemEntry> copy(String id, String destinationId, ConflictPolicy conflict) => _api.items.copyItem(
    id: id,
    body: TransferRequest(destinationId: destinationId, conflict: conflict),
  );

  Future<void> recordOpen(String id) => _api.itemOpens.recordItemOpen(id: id);

  Future<void> trash(List<String> ids) => _api.items.moveItemsToTrash(body: ItemSelection(ids: ids));

  Uri archiveUri(List<String> ids) => baseUri.replace(path: '/api/items/archive', queryParameters: {'ids': ids});

  @override
  List<String> itemPath(String id) => ['api', 'items', id];
}
