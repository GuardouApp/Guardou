import 'dart:async';

import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/browser/browser_notifications.dart';
import '../../l10n/generated/app_localizations.dart';
import '../settings/settings_section.dart';
import 'browser_notifications_controller.dart';

class NotificationsSection extends ConsumerWidget {
  const NotificationsSection({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    return SettingsSection<bool>(
      icon: FluentIcons.alert_24_regular,
      title: l10n.notificationSettings,
      description: l10n.notificationSettingsDescription,
      value: AsyncData(ref.watch(browserNotificationsControllerProvider)),
      onRetry: () => ref.invalidate(browserNotificationsControllerProvider),
      builder: (enabled) => _BrowserNotificationsSwitch(enabled: enabled),
    );
  }
}

class _BrowserNotificationsSwitch extends ConsumerWidget {
  const _BrowserNotificationsSwitch({required this.enabled});

  final bool enabled;

  Future<void> _change(BuildContext context, WidgetRef ref, bool value) async {
    final controller = ref.read(browserNotificationsControllerProvider.notifier);
    if (!value) {
      controller.disable();
      return;
    }
    final messenger = ScaffoldMessenger.of(context);
    final denied = AppLocalizations.of(context).browserNotificationsDenied;
    if (await controller.enable() != BrowserNotificationPermission.granted) {
      messenger.showSnackBar(SnackBar(content: Text(denied)));
    }
  }

  String _description(AppLocalizations l10n, BrowserNotificationPermission permission) => switch (permission) {
    BrowserNotificationPermission.unsupported => l10n.browserNotificationsUnsupported,
    BrowserNotificationPermission.denied => l10n.browserNotificationsDenied,
    _ => l10n.browserNotificationsDescription,
  };

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final permission = ref.read(browserNotificationsControllerProvider.notifier).permission;
    final available = permission != BrowserNotificationPermission.unsupported;

    return SettingsField(
      label: l10n.browserNotifications,
      description: _description(l10n, permission),
      child: Align(
        alignment: AlignmentDirectional.centerStart,
        child: Switch(
          value: enabled,
          onChanged: available ? (value) => unawaited(_change(context, ref, value)) : null,
        ),
      ),
    );
  }
}
