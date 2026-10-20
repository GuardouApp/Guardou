import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/user_account.dart';
import '../../api/generated/models/user_request.dart';
import '../../core/api/api_providers.dart';

final usersServiceProvider = Provider<UsersService>((ref) => UsersService(ref.watch(apiClientProvider)));

class UsersService {
  const UsersService(this._api);

  static const oidcProvider = 'oidc';

  final ApiClient _api;

  Future<List<UserAccount>> list() => _api.users.listUsers();

  Future<UserAccount> create(UserRequest request) => _api.users.createUser(body: request);

  Future<UserAccount> update(String id, UserRequest request) => _api.users.updateUser(id: id, body: request);

  Future<void> delete(String id) => _api.users.deleteUser(id: id);

  Future<UserAccount> removeOidcLogin(String id) => _api.users.removeUserLogin(id: id, provider: oidcProvider);
}
