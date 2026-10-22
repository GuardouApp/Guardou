import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/item_details.dart';
import '../../api/generated/models/item_entry.dart';
import '../../core/api/api_providers.dart';
import 'shared_file_source.dart';

final sharesServiceProvider = Provider<SharesService>(
  (ref) => SharesService(ref.watch(apiClientProvider), ref.watch(dioProvider), Uri.parse(ref.watch(apiBaseUrlProvider))),
);

class SharesService {
  const SharesService(this._api, this._dio, this._baseUri);

  static const _notFound = 404;

  final ApiClient _api;
  final Dio _dio;
  final Uri _baseUri;

  Future<String?> link(String id) async {
    try {
      return (await _api.shares.getItemShare(id: id)).url;
    } on DioException catch (error) {
      if (error.response?.statusCode == _notFound) {
        return null;
      }
      rethrow;
    }
  }

  Future<String> share(String id) async => (await _api.shares.shareItem(id: id)).url;

  Future<void> stopSharing(String id) => _api.shares.stopSharingItem(id: id);

  Future<ItemEntry> sharedItem(String token) => _api.shares.getSharedItem(token: token);

  Future<ItemDetails> sharedDetails(String token, String id) => _api.shares.getSharedItemDetails(token: token, id: id);

  SharedFileSource files(String token) => SharedFileSource(_dio, _baseUri, token);
}
