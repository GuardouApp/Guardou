import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'local_store_stub.dart' if (dart.library.js_interop) 'local_store_web.dart';

abstract interface class LocalStore {
  String? read(String key);

  void write(String key, String value);
}

final localStoreProvider = Provider<LocalStore>((ref) => createLocalStore());
