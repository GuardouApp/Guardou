import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/item_kind.dart';
import 'shared_folder_page.dart';
import 'shared_items.dart';
import 'shared_link_states.dart';
import 'shared_viewer_page.dart';

class SharedLinkPage extends ConsumerWidget {
  const SharedLinkPage({super.key, required this.token});

  final String token;

  @override
  Widget build(BuildContext context, WidgetRef ref) => switch (ref.watch(sharedRootProvider(token))) {
    AsyncValue(hasValue: true, :final value?) when value.kind == ItemKind.folder => SharedFolderPage(
      token: token,
      folderId: value.id,
    ),
    AsyncValue(hasValue: true, :final value?) => SharedViewerPage(token: token, fileId: value.id),
    AsyncValue(:final error?) => SharedLinkError(
      error: error,
      onRetry: () => ref.invalidate(sharedRootProvider(token)),
    ),
    _ => const SharedLoadingPage(),
  };
}
