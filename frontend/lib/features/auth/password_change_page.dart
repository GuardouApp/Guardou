import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import 'auth_card.dart';
import 'auth_form_controller.dart';
import 'password_change_fields.dart';
import 'session_controller.dart';

class PasswordChangePage extends ConsumerStatefulWidget {
  const PasswordChangePage({super.key});

  @override
  ConsumerState<PasswordChangePage> createState() => _PasswordChangePageState();
}

class _PasswordChangePageState extends ConsumerState<PasswordChangePage> {
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
    await ref
        .read(authFormControllerProvider.notifier)
        .changePassword(currentPassword: _fields.current.text, newPassword: _fields.password.text);
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final spacing = AppTokens.of(context).spacing;
    final submission = ref.watch(authFormControllerProvider);

    return AuthCard(
      formKey: _formKey,
      title: l10n.passwordChangeTitle,
      subtitle: l10n.passwordChangeRequired,
      failure: switch (submission.error) {
        final AuthFailure failure => failure,
        _ => null,
      },
      children: [
        PasswordChangeFields(
          current: _fields.current,
          password: _fields.password,
          confirmation: _fields.confirmation,
          onSubmitted: _submit,
        ),
        SizedBox(height: spacing * 1.5),
        FilledButton(onPressed: submission.isLoading ? null : _submit, child: Text(l10n.passwordChangeSubmit)),
        SizedBox(height: spacing / 2),
        TextButton(onPressed: () => ref.read(sessionControllerProvider.notifier).logout(), child: Text(l10n.signOut)),
      ],
    );
  }
}
