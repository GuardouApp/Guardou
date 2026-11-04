import 'package:flutter/material.dart';

import '../../l10n/generated/app_localizations.dart';
import '../settings/settings_layout.dart';
import 'appearance_section.dart';
import 'upload_section.dart';

class PreferencesPage extends StatelessWidget {
  const PreferencesPage({super.key});

  @override
  Widget build(BuildContext context) => SettingsLayout(
    title: AppLocalizations.of(context).accountSettings,
    sections: const [AppearanceSection(), UploadSection()],
  );
}
