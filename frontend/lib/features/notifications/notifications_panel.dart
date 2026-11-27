import 'dart:async';

import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../api/generated/models/notification_entry.dart';
import '../../app/theme/app_tokens.dart';
import '../../core/widgets/user_avatar.dart';
import '../../l10n/generated/app_localizations.dart';
import 'notification_opener.dart';
import 'notification_presentation.dart';
import 'notifications_controller.dart';

class NotificationsPanel extends ConsumerWidget {
  const NotificationsPanel({super.key});

  static const _radius = 28.0;
  static const _maxListHeight = 480.0;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final spacing = AppTokens.of(context).spacing;
    final colorScheme = Theme.of(context).colorScheme;
    final notifications = ref.watch(notificationsControllerProvider);
    final entries = notifications.value ?? const <NotificationEntry>[];

    return Material(
      color: colorScheme.surfaceContainer,
      elevation: 3,
      borderRadius: BorderRadius.circular(_radius),
      clipBehavior: Clip.antiAlias,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Padding(
            padding: EdgeInsets.fromLTRB(spacing * 1.5, spacing, spacing / 2, spacing / 2),
            child: Row(
              children: [
                Expanded(child: Text(l10n.notifications, style: Theme.of(context).textTheme.titleMedium)),
                if (entries.isNotEmpty)
                  TextButton(
                    onPressed: () => unawaited(ref.read(notificationsControllerProvider.notifier).clear()),
                    child: Text(l10n.notificationsClear),
                  ),
              ],
            ),
          ),
          Flexible(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxHeight: _maxListHeight),
              child: switch (notifications) {
                AsyncValue(hasValue: true) when entries.isEmpty => _EmptyNotifications(spacing: spacing),
                AsyncValue(hasValue: true) => ListView.builder(
                  shrinkWrap: true,
                  padding: EdgeInsets.only(bottom: spacing / 2),
                  itemCount: entries.length,
                  itemBuilder: (context, index) => NotificationTile(notification: entries[index]),
                ),
                AsyncValue(hasError: true) => Padding(
                  padding: EdgeInsets.all(spacing),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(l10n.notificationsLoadError),
                      TextButton(
                        onPressed: () => ref.invalidate(notificationsControllerProvider),
                        child: Text(l10n.errorRetry),
                      ),
                    ],
                  ),
                ),
                _ => Padding(
                  padding: EdgeInsets.all(spacing),
                  child: Center(child: CircularProgressIndicator(semanticsLabel: l10n.loading)),
                ),
              },
            ),
          ),
        ],
      ),
    );
  }
}

class _EmptyNotifications extends StatelessWidget {
  const _EmptyNotifications({required this.spacing});

  final double spacing;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    return Padding(
      padding: EdgeInsets.all(spacing * 2),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(FluentIcons.alert_24_regular, size: 40, color: colorScheme.onSurfaceVariant),
          SizedBox(height: spacing),
          Text(AppLocalizations.of(context).notificationsEmpty, textAlign: TextAlign.center),
        ],
      ),
    );
  }
}

class NotificationTile extends ConsumerWidget {
  const NotificationTile({super.key, required this.notification});

  final NotificationEntry notification;

  void _open(BuildContext context, WidgetRef ref, NotificationTarget target) {
    Navigator.of(context).pop();
    ref.read(notificationOpenerProvider).open(target);
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final colorScheme = Theme.of(context).colorScheme;
    final sender = notification.sender;
    final target = notification.target;

    return ListTile(
      leading: sender == null
          ? CircleAvatar(
              backgroundColor: colorScheme.secondaryContainer,
              foregroundColor: colorScheme.onSecondaryContainer,
              child: Icon(notification.icon),
            )
          : UserAvatar(userId: sender.id, name: sender.displayName, hasPicture: sender.hasPicture, radius: 20),
      title: Text(
        notification.message(l10n),
        style: notification.read ? null : const TextStyle(fontWeight: FontWeight.w600),
      ),
      subtitle: Text(DateFormat.yMMMd(l10n.localeName).add_jm().format(notification.created.toLocal())),
      trailing: notification.read
          ? null
          : Icon(FluentIcons.circle_24_filled, size: 10, color: colorScheme.primary, semanticLabel: l10n.notificationUnread),
      onTap: target == null ? null : () => _open(context, ref, target),
    );
  }
}
