import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/language.dart';
import '../../api/generated/models/theme_preference.dart';
import '../../api/generated/models/user_preferences.dart';
import '../../l10n/generated/app_localizations.dart';
import '../settings/settings_section.dart';
import 'preference_choices.dart';
import 'preferences_section.dart';

class AppearanceSection extends StatelessWidget {
  const AppearanceSection({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return PreferencesSection(
      icon: LucideIcons.palette,
      title: l10n.appearanceSettings,
      description: l10n.appearanceSettingsDescription,
      builder: (preferences) => _AppearanceForm(preferences: preferences),
    );
  }
}

class _AppearanceForm extends ConsumerStatefulWidget {
  const _AppearanceForm({required this.preferences});

  final UserPreferences preferences;

  @override
  ConsumerState<_AppearanceForm> createState() => _AppearanceFormState();
}

class _AppearanceFormState extends ConsumerState<_AppearanceForm>
    with SettingsForm<_AppearanceForm, UserPreferences>, PreferencesForm<_AppearanceForm> {
  late ThemePreference _theme;
  late Language _language;

  @override
  UserPreferences get saved => widget.preferences;

  @override
  void initState() {
    super.initState();
    fill(widget.preferences);
  }

  @override
  void fill(UserPreferences value) {
    _theme = value.theme;
    _language = value.language;
  }

  @override
  UserPreferences changed(UserPreferences current) => current.copyWith(theme: _theme, language: _language);

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Form(
      key: formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          SettingsChoiceField(
            label: l10n.theme,
            value: _theme,
            choices: ThemePreference.$valuesDefined,
            labelOf: (choice) => choice.label(l10n),
            onChanged: (choice) => setState(() => _theme = choice),
          ),
          SettingsChoiceField(
            label: l10n.language,
            value: _language,
            choices: Language.$valuesDefined,
            labelOf: (choice) => choice.label(l10n),
            onChanged: (choice) => setState(() => _language = choice),
          ),
          settingsActions(),
        ],
      ),
    );
  }
}
