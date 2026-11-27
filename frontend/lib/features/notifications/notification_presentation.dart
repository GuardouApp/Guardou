import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:flutter/widgets.dart';

import '../../api/generated/models/item_kind.dart';
import '../../api/generated/models/notification_entry.dart';
import '../../api/generated/models/notification_kind.dart';
import '../../app/router/routes.dart';
import '../../l10n/generated/app_localizations.dart';

sealed class NotificationTarget {
  const NotificationTarget();
}

final class AppTarget extends NotificationTarget {
  const AppTarget(this.location);

  final String location;
}

final class ExternalTarget extends NotificationTarget {
  const ExternalTarget(this.uri);

  final Uri uri;
}

extension NotificationPresentation on NotificationEntry {
  String message(AppLocalizations l10n) => switch (kind) {
    NotificationKind.shareReceived => l10n.notificationShareReceived(
      sender?.displayName ?? l10n.notificationUnknownUser,
      item?.name ?? l10n.notificationUnknownItem,
    ),
    NotificationKind.quotaWarning => l10n.notificationQuotaWarning,
    NotificationKind.updateAvailable => l10n.notificationUpdateAvailable(version ?? ''),
    NotificationKind.$unknown => l10n.notificationUnknown,
  };

  IconData get icon => switch (kind) {
    NotificationKind.shareReceived => FluentIcons.people_24_regular,
    NotificationKind.quotaWarning => FluentIcons.storage_24_regular,
    NotificationKind.updateAvailable => FluentIcons.arrow_circle_up_24_regular,
    NotificationKind.$unknown => FluentIcons.alert_24_regular,
  };

  NotificationTarget? get target {
    final shared = item;
    final release = url;
    return switch (kind) {
      NotificationKind.shareReceived when shared != null => AppTarget(
        shared.kind == ItemKind.folder ? Routes.folder(shared.id) : Routes.viewer(shared.id),
      ),
      NotificationKind.quotaWarning => const AppTarget(Routes.files),
      NotificationKind.updateAvailable when release != null => ExternalTarget(Uri.parse(release)),
      _ => null,
    };
  }
}
