import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/server_settings.dart';
import '../../api/generated/models/server_settings_view.dart';
import '../../l10n/generated/app_localizations.dart';
import 'server_settings_controller.dart';
import 'settings_section.dart';

class ServerSettingsSection extends ConsumerWidget {
  const ServerSettingsSection({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    return SettingsSection(
      icon: LucideIcons.globe,
      title: l10n.serverSettings,
      description: l10n.serverSettingsDescription,
      value: ref.watch(serverSettingsProvider),
      onRetry: () => ref.invalidate(serverSettingsProvider),
      builder: (settings) => _ServerSettingsForm(settings: settings),
    );
  }
}

class _ServerSettingsForm extends ConsumerStatefulWidget {
  const _ServerSettingsForm({required this.settings});

  final ServerSettingsView settings;

  @override
  ConsumerState<_ServerSettingsForm> createState() => _ServerSettingsFormState();
}

class _ServerSettingsFormState extends ConsumerState<_ServerSettingsForm>
    with SettingsForm<_ServerSettingsForm, ServerSettingsView> {
  static const _minimumRetention = 1;
  static const _maximumRetention = 3650;

  final _externalUrl = TextEditingController();
  final _trashRetention = TextEditingController();

  @override
  ServerSettingsView get saved => widget.settings;

  @override
  void initState() {
    super.initState();
    fill(widget.settings);
  }

  @override
  void fill(ServerSettingsView value) {
    _externalUrl.text = value.externalUrl ?? '';
    _trashRetention.text = '${value.trashRetentionDays}';
  }

  @override
  Future<ServerSettingsView> defaults() => ref.read(serverSettingsProvider.notifier).defaults();

  @override
  Future<void> persist() => ref
      .read(serverSettingsProvider.notifier)
      .save(
        ServerSettings(
          externalUrl: trimmedOrNull(_externalUrl.text),
          trashRetentionDays: int.parse(_trashRetention.text),
        ),
      );

  @override
  void dispose() {
    _externalUrl.dispose();
    _trashRetention.dispose();
    super.dispose();
  }

  String? _validateRetention(AppLocalizations l10n, String? value) {
    final days = int.tryParse(value ?? '');
    final valid = days != null && days >= _minimumRetention && days <= _maximumRetention;
    return valid ? null : l10n.concurrencyInvalid(_minimumRetention, _maximumRetention);
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Form(
      key: formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          SettingsTextField(
            label: l10n.externalUrl,
            description: l10n.externalUrlDescription,
            controller: _externalUrl,
            keyboardType: TextInputType.url,
            lockedBy: lockedVariable(widget.settings.locked, 'externalUrl'),
            validator: (value) => validateHttpUrl(l10n, value),
          ),
          SettingsTextField(
            label: l10n.trashRetentionDays,
            description: l10n.trashRetentionDescription,
            required: true,
            controller: _trashRetention,
            keyboardType: TextInputType.number,
            inputFormatters: [FilteringTextInputFormatter.digitsOnly],
            lockedBy: lockedVariable(widget.settings.locked, 'trashRetentionDays'),
            validator: (value) => _validateRetention(l10n, value),
          ),
          settingsActions(),
        ],
      ),
    );
  }
}
