import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/password_policy.dart';
import '../../l10n/generated/app_localizations.dart';
import '../configuration/configuration_controller.dart';
import '../settings/settings_section.dart';
import 'credential_rules.dart';
import 'credential_validators.dart';

class NewPasswordFields extends ConsumerWidget {
  const NewPasswordFields({
    super.key,
    required this.label,
    required this.password,
    required this.confirmation,
    required this.onSubmitted,
  });

  final String label;
  final TextEditingController password;
  final TextEditingController confirmation;
  final VoidCallback onSubmitted;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final policy = ref.watch(configurationControllerProvider).value?.passwordPolicy;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        SettingsTextField(
          label: label,
          required: true,
          controller: password,
          autofillHints: const [AutofillHints.newPassword],
          obscureText: true,
          textInputAction: TextInputAction.next,
          validator: (value) => validatePassword(l10n, policy, value),
        ),
        if (policy != null) PasswordHints(policy: policy, password: password),
        SettingsTextField(
          label: l10n.registerConfirmPassword,
          required: true,
          controller: confirmation,
          autofillHints: const [AutofillHints.newPassword],
          obscureText: true,
          onFieldSubmitted: (_) => onSubmitted(),
          validator: (value) => value == password.text ? null : l10n.registerPasswordsDiffer,
        ),
      ],
    );
  }
}

class PasswordHints extends StatelessWidget {
  const PasswordHints({super.key, required this.policy, required this.password});

  final PasswordPolicy policy;
  final TextEditingController password;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return ValueListenableBuilder(
      valueListenable: password,
      builder: (context, value, _) => Padding(
        padding: const EdgeInsets.only(top: 8),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            for (final rule in passwordRules(policy))
              _Hint(
                met: isPasswordRuleMet(policy, rule, value.text),
                text: passwordRuleMessage(l10n, policy, rule),
                metColor: colors.primary,
                unmetColor: colors.onSurfaceVariant,
                style: theme.textTheme.bodySmall,
              ),
          ],
        ),
      ),
    );
  }
}

class _Hint extends StatelessWidget {
  const _Hint({
    required this.met,
    required this.text,
    required this.metColor,
    required this.unmetColor,
    required this.style,
  });

  final bool met;
  final String text;
  final Color metColor;
  final Color unmetColor;
  final TextStyle? style;

  @override
  Widget build(BuildContext context) {
    final color = met ? metColor : unmetColor;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        children: [
          Icon(
            met ? FluentIcons.checkmark_circle_24_filled : FluentIcons.circle_24_regular,
            size: 16,
            color: color,
            semanticLabel: met ? AppLocalizations.of(context).passwordRuleMet : null,
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Text(text, style: style?.copyWith(color: color)),
          ),
        ],
      ),
    );
  }
}
