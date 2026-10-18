import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_selection.dart';
import '../../api/generated/models/trash_entry.dart';
import '../../core/api/api_providers.dart';

final trashServiceProvider = Provider<TrashService>((ref) => TrashService(ref.watch(apiClientProvider)));

class TrashService {
  const TrashService(this._api);

  final ApiClient _api;

  Future<List<TrashEntry>> list() => _api.trash.getTrash();

  Future<List<ItemEntry>> restore(List<String> ids) => _api.trash.restoreItems(body: ItemSelection(ids: ids));

  Future<void> delete(List<String> ids) => _api.trash.deleteItems(body: ItemSelection(ids: ids));

  Future<void> empty() => _api.trash.emptyTrash();
}
