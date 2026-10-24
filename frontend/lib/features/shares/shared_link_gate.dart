import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../../app/router/routes.dart';
import 'shared_items.dart';
import 'shared_link_states.dart';

String sharedItemLocation(ItemEntry shared) =>
    shared.kind == ItemKind.folder ? Routes.folder(shared.id) : Routes.viewer(shared.id);

class SharedLinkGate extends ConsumerWidget {
  const SharedLinkGate({super.key, required this.token, required this.locationOf, required this.child});

  final String token;
  final String Function(ItemEntry shared) locationOf;
  final Widget child;

  @override
  Widget build(BuildContext context, WidgetRef ref) => switch (ref.watch(joinedShareProvider(token))) {
    AsyncData(value: final shared?) => _Redirect(location: locationOf(shared)),
    AsyncData() || AsyncError() => child,
    _ => const SharedLoadingPage(),
  };
}

class _Redirect extends StatefulWidget {
  const _Redirect({required this.location});

  final String location;

  @override
  State<_Redirect> createState() => _RedirectState();
}

class _RedirectState extends State<_Redirect> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        context.go(widget.location);
      }
    });
  }

  @override
  Widget build(BuildContext context) => const SharedLoadingPage();
}
