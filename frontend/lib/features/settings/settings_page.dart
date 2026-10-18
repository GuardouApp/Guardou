import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/task_concurrency.dart';
import '../../app/theme/app_tokens.dart';
import '../../core/widgets/page_header.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/folder_states.dart';
import '../tasks/task_presentation.dart';
import 'task_settings_controller.dart';

class SettingsPage extends ConsumerWidget {
  const SettingsPage({super.key});

  static const _maxWidth = 800.0;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final spacing = AppTokens.of(context).spacing;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(child: Text(l10n.systemSettings)),
        const Divider(height: 1),
        Expanded(
          child: Material(
            type: MaterialType.transparency,
            child: switch (ref.watch(taskSettingsProvider)) {
              AsyncValue(:final value?) => ListView(
                padding: EdgeInsets.all(spacing * 1.5),
                children: [
                  Center(
                    child: ConstrainedBox(
                      constraints: const BoxConstraints(maxWidth: _maxWidth),
                      child: _TaskSettingsSection(settings: value),
                    ),
                  ),
                ],
              ),
              AsyncValue(error: _?) => FolderMessage(
                icon: LucideIcons.serverCrash,
                title: l10n.settingsLoadErrorTitle,
                message: l10n.genericError,
                action: FilledButton(
                  onPressed: () => ref.invalidate(taskSettingsProvider),
                  child: Text(l10n.errorRetry),
                ),
              ),
              _ => Center(child: CircularProgressIndicator(semanticsLabel: l10n.loading)),
            },
          ),
        ),
      ],
    );
  }
}

class _TaskSettingsSection extends ConsumerStatefulWidget {
  const _TaskSettingsSection({required this.settings});

  final List<TaskConcurrency> settings;

  @override
  ConsumerState<_TaskSettingsSection> createState() => _TaskSettingsSectionState();
}

class _TaskSettingsSectionState extends ConsumerState<_TaskSettingsSection> {
  static const _minimum = 1;
  static const _maximum = 16;

  final _formKey = GlobalKey<FormState>();
  late final Map<TaskConcurrency, TextEditingController> _fields = {
    for (final setting in widget.settings) setting: TextEditingController(text: '${setting.concurrency}'),
  };
  bool _saving = false;

  @override
  void dispose() {
    for (final field in _fields.values) {
      field.dispose();
    }
    super.dispose();
  }

  String? _validate(String? value, AppLocalizations l10n) {
    final number = int.tryParse(value ?? '');
    return number == null || number < _minimum || number > _maximum
        ? l10n.concurrencyInvalid(_minimum, _maximum)
        : null;
  }

  Future<void> _save() async {
    if (!(_formKey.currentState?.validate() ?? false)) {
      return;
    }
    final l10n = AppLocalizations.of(context);
    final messenger = ScaffoldMessenger.of(context);
    setState(() => _saving = true);
    try {
      await ref.read(taskSettingsProvider.notifier).save([
        for (final MapEntry(key: setting, value: field) in _fields.entries)
          setting.copyWith(concurrency: int.parse(field.text)),
      ]);
      messenger.showSnackBar(SnackBar(content: Text(l10n.settingsSaved)));
    } on DioException {
      messenger.showSnackBar(SnackBar(content: Text(l10n.genericError)));
    } finally {
      if (mounted) {
        setState(() => _saving = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final tokens = AppTokens.of(context);
    final theme = Theme.of(context);

    return Material(
      color: theme.colorScheme.surfaceContainer,
      clipBehavior: Clip.antiAlias,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(tokens.panelRadius)),
      child: ExpansionTile(
        initiallyExpanded: true,
        shape: const Border(),
        collapsedShape: const Border(),
        leading: Icon(LucideIcons.refreshCw, color: theme.colorScheme.primary),
        title: Text(l10n.taskSettings, style: theme.textTheme.titleMedium?.copyWith(color: theme.colorScheme.primary)),
        subtitle: Text(l10n.taskSettingsDescription),
        childrenPadding: EdgeInsets.fromLTRB(tokens.spacing * 1.5, 0, tokens.spacing * 1.5, tokens.spacing * 1.5),
        children: [
          Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                for (final MapEntry(key: setting, value: field) in _fields.entries)
                  Padding(
                    padding: EdgeInsets.only(top: tokens.spacing),
                    child: TextFormField(
                      controller: field,
                      keyboardType: TextInputType.number,
                      inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                      decoration: InputDecoration(
                        labelText: l10n.queueConcurrency(queueTitle(setting.kind, l10n)),
                        filled: true,
                      ),
                      validator: (value) => _validate(value, l10n),
                    ),
                  ),
                SizedBox(height: tokens.spacing),
                Align(
                  alignment: AlignmentDirectional.centerEnd,
                  child: FilledButton(onPressed: _saving ? null : () => unawaited(_save()), child: Text(l10n.save)),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
