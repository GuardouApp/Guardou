import 'local_store.dart';

LocalStore createLocalStore() => MemoryLocalStore();

class MemoryLocalStore implements LocalStore {
  final _values = <String, String>{};

  @override
  String? read(String key) => _values[key];

  @override
  void write(String key, String value) => _values[key] = value;
}
