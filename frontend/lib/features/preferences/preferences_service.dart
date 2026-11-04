import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/api_client.dart';
import '../../api/generated/models/user_preferences.dart';
import '../../core/api/api_providers.dart';

final preferencesServiceProvider = Provider<PreferencesService>(
  (ref) => PreferencesService(ref.watch(apiClientProvider)),
);

class PreferencesService {
  const PreferencesService(this._api);

  final ApiClient _api;

  Future<UserPreferences> preferences() => _api.preferences.getPreferences();

  Future<UserPreferences> defaults() => _api.preferences.getPreferencesDefaults();

  Future<UserPreferences> change(UserPreferences preferences) => _api.preferences.changePreferences(body: preferences);
}
