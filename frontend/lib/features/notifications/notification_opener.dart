import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/router/app_router.dart';
import '../../core/browser/browser_location.dart';
import 'notification_presentation.dart';

final notificationOpenerProvider = Provider<NotificationOpener>((ref) => NotificationOpener(ref));

class NotificationOpener {
  const NotificationOpener(this._ref);

  final Ref _ref;

  void open(NotificationTarget target) => switch (target) {
    AppTarget(:final location) => _ref.read(appRouterProvider).go(location),
    ExternalTarget(:final uri) => _ref.read(browserLocationProvider).openInNewTab(uri),
  };
}
