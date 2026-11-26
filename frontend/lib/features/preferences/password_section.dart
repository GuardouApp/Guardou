import 'package:fluentui_system_icons/fluentui_system_icons.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import '../auth/auth_card.dart';
import '../auth/auth_form_controller.dart';
import '../auth/password_change_fields.dart';
import '../settings/settings_section.dart';

class PasswordSection extends StatelessWidget {
  const PasswordSection({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return SettingsSection<bool>(
      icon: FluentIcons.password_24_regular,
      title: l10n.passwordSettings,
      description: l10n.passwordSettingsDescription,
      value: const AsyncData(true),
      onRetry: () {},
      builder: (_) => const _PasswordForm(),
    );
  }
}

class _PasswordForm extends ConsumerStatefulWidget {
  const _PasswordForm();

  @override
  ConsumerState<_PasswordForm> createState() => _PasswordFormState();
}

class _PasswordFormState extends ConsumerState<_PasswordForm> {
  final _formKey = GlobalKey<FormState>();
  final _fields = PasswordChangeControllers();

  @override
  void dispose() {
    _fields.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!(_formKey.currentState?.validate() ?? false)) {
      return;
    }
    final messenger = ScaffoldMessenger.of(context);
    final changed = AppLocalizations.of(context).passwordChanged;
    final succeeded = await ref
        .read(authFormControllerProvider.notifier)
        .changePassword(currentPassword: _fields.current.text, newPassword: _fields.password.text);
    if (succeeded && mounted) {
      _fields.clear();
      messenger.showSnackBar(SnackBar(content: Text(changed)));
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final theme = Theme.of(context);
    final spacing = AppTokens.of(context).spacing;
    final submission = ref.watch(authFormControllerProvider);

    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          PasswordChangeFields(
            current: _fields.current,
            password: _fields.password,
            confirmation: _fields.confirmation,
            onSubmitted: _submit,
          ),
          if (submission.error case final AuthFailure failure)
            Padding(
              padding: EdgeInsets.only(top: spacing),
              child: Text(
                AuthCard.failureMessage(l10n, failure),
                style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.error),
              ),
            ),
          Padding(
            padding: EdgeInsets.only(top: spacing * 1.5),
            child: Align(
              alignment: AlignmentDirectional.centerEnd,
              child: FilledButton(
                onPressed: submission.isLoading ? null : _submit,
                child: Text(l10n.passwordChangeSubmit),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
