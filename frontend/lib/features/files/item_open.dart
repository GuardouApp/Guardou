import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'files_service.dart';

final itemOpenProvider = FutureProvider.autoDispose.family<void, String>(
  (ref, id) => ref.read(filesServiceProvider).recordOpen(id),
);
