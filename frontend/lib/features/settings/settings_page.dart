import 'package:flutter/material.dart';

import '../../app/theme/app_tokens.dart';
import '../../core/widgets/page_header.dart';
import '../../l10n/generated/app_localizations.dart';
import 'oidc_settings_section.dart';
import 'server_settings_section.dart';
import 'task_settings_section.dart';

class SettingsPage extends StatelessWidget {
  const SettingsPage({super.key});

  static const _maxWidth = 800.0;

  @override
  Widget build(BuildContext context) {
    final spacing = AppTokens.of(context).spacing;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(child: Text(AppLocalizations.of(context).systemSettings)),
        const Divider(height: 1),
        Expanded(
          child: Material(
            type: MaterialType.transparency,
            child: ListView(
              padding: EdgeInsets.all(spacing * 1.5),
              children: [
                for (final section in const [ServerSettingsSection(), OidcSettingsSection(), TaskSettingsSection()])
                  Center(
                    child: ConstrainedBox(
                      constraints: const BoxConstraints(maxWidth: _maxWidth),
                      child: Padding(padding: EdgeInsets.only(bottom: spacing), child: section),
                    ),
                  ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}
