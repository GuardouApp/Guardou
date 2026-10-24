import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/server_settings.dart';
import '../../core/widgets/labeled_text_field.dart';
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

  final ServerSettings settings;

  @override
  ConsumerState<_ServerSettingsForm> createState() => _ServerSettingsFormState();
}

class _ServerSettingsFormState extends ConsumerState<_ServerSettingsForm>
    with SettingsForm<_ServerSettingsForm, ServerSettings> {
  late final _externalUrl = TextEditingController(text: widget.settings.externalUrl);

  @override
  ServerSettings get saved => widget.settings;

  @override
  void fill(ServerSettings value) => _externalUrl.text = value.externalUrl ?? '';

  @override
  Future<ServerSettings> defaults() => ref.read(serverSettingsProvider.notifier).defaults();

  @override
  Future<void> persist() =>
      ref.read(serverSettingsProvider.notifier).save(ServerSettings(externalUrl: trimmedOrNull(_externalUrl.text)));

  @override
  void dispose() {
    _externalUrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Form(
      key: formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          LabeledTextField(
            label: l10n.externalUrl,
            description: l10n.externalUrlDescription,
            controller: _externalUrl,
            keyboardType: TextInputType.url,
            validator: (value) => validateHttpUrl(l10n, value),
          ),
          settingsActions(),
        ],
      ),
    );
  }
}
