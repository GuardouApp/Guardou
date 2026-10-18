import 'package:web/web.dart' as web;

import 'local_store.dart';

LocalStore createLocalStore() => const _WebLocalStore();

class _WebLocalStore implements LocalStore {
  const _WebLocalStore();

  @override
  String? read(String key) => web.window.localStorage.getItem(key);

  @override
  void write(String key, String value) => web.window.localStorage.setItem(key, value);
}
