import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../l10n/generated/app_localizations.dart';
import '../auth/session_controller.dart';
import '../notifications/notifications_section.dart';
import '../settings/settings_layout.dart';
import 'general_section.dart';
import 'password_section.dart';
import 'upload_section.dart';

class PreferencesPage extends ConsumerWidget {
  const PreferencesPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final hasPassword = ref.watch(sessionControllerProvider).value?.hasPassword ?? false;
    return SettingsLayout(
      title: AppLocalizations.of(context).accountSettings,
      sections: [
        const GeneralSection(),
        const UploadSection(),
        const NotificationsSection(),
        if (hasPassword) const PasswordSection(),
      ],
    );
  }
}
