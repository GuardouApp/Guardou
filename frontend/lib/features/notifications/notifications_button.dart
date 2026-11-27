import 'dart:async';

import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../l10n/generated/app_localizations.dart';
import 'notifications_controller.dart';
import 'notifications_panel.dart';

class NotificationsButton extends ConsumerWidget {
  const NotificationsButton({super.key});

  static const _maxWidth = 400.0;
  static const _margin = 8.0;

  Future<void> _open(BuildContext context, WidgetRef ref) async {
    final controller = ref.read(notificationsControllerProvider.notifier);
    await showGeneralDialog<void>(
      context: context,
      barrierDismissible: true,
      barrierLabel: AppLocalizations.of(context).notifications,
      barrierColor: Colors.transparent,
      pageBuilder: (context, _, _) => SafeArea(
        child: Align(
          alignment: AlignmentDirectional.topEnd,
          child: Padding(
            padding: const EdgeInsets.fromLTRB(_margin, kToolbarHeight, _margin, _margin),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: _maxWidth),
              child: const NotificationsPanel(),
            ),
          ),
        ),
      ),
    );
    await controller.markAllRead();
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final unread = ref.watch(unreadNotificationsProvider);
    final l10n = AppLocalizations.of(context);

    return IconButton(
      tooltip: unread == 0 ? l10n.notifications : l10n.notificationsUnread(unread),
      onPressed: () => unawaited(_open(context, ref)),
      icon: Badge(
        isLabelVisible: unread > 0,
        label: Text('$unread'),
        child: const Icon(FluentIcons.alert_24_regular),
      ),
    );
  }
}
