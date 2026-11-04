import 'package:flutter/material.dart';

import '../../l10n/generated/app_localizations.dart';
import 'oidc_settings_section.dart';
import 'server_settings_section.dart';
import 'settings_layout.dart';
import 'task_settings_section.dart';

class SettingsPage extends StatelessWidget {
  const SettingsPage({super.key});

  @override
  Widget build(BuildContext context) => SettingsLayout(
    title: AppLocalizations.of(context).systemSettings,
    sections: const [ServerSettingsSection(), OidcSettingsSection(), TaskSettingsSection()],
  );
}
