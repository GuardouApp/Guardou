import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/theme/app_tokens.dart';
import '../../core/widgets/labeled_text_field.dart';
import '../../l10n/generated/app_localizations.dart';
import '../configuration/configuration_controller.dart';
import 'auth_card.dart';
import 'auth_form_controller.dart';
import 'credential_validators.dart';

class RegisterPage extends ConsumerStatefulWidget {
  const RegisterPage({super.key});

  @override
  ConsumerState<RegisterPage> createState() => _RegisterPageState();
}

class _RegisterPageState extends ConsumerState<RegisterPage> {
  final _formKey = GlobalKey<FormState>();
  final _username = TextEditingController();
  final _displayName = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController();
  final _confirmation = TextEditingController();

  @override
  void dispose() {
    _username.dispose();
    _displayName.dispose();
    _email.dispose();
    _password.dispose();
    _confirmation.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!(_formKey.currentState?.validate() ?? false)) {
      return;
    }
    await ref
        .read(authFormControllerProvider.notifier)
        .register(
          username: _username.text,
          displayName: _displayName.text.trim(),
          email: _email.text.trim(),
          password: _password.text,
        );
  }

  String? _validateConfirmation(String? value) =>
      value == _password.text ? null : AppLocalizations.of(context).registerPasswordsDiffer;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final spacing = AppTokens.of(context).spacing;
    final submission = ref.watch(authFormControllerProvider);
    final policy = ref.watch(configurationControllerProvider).value?.passwordPolicy;

    return AuthCard(
      formKey: _formKey,
      title: l10n.registerTitle,
      subtitle: l10n.registerSubtitle,
      failure: switch (submission.error) {
        final AuthFailure failure => failure,
        _ => null,
      },
      children: [
        LabeledTextField(
          label: l10n.loginUsername,
          controller: _username,
          autofillHints: const [AutofillHints.newUsername],
          textInputAction: TextInputAction.next,
          validator: (value) => validateUsername(l10n, value),
        ),
        LabeledTextField(
          label: l10n.displayName,
          controller: _displayName,
          autofillHints: const [AutofillHints.name],
          textInputAction: TextInputAction.next,
          validator: (value) => validateDisplayName(l10n, value),
        ),
        LabeledTextField(
          label: l10n.registerEmail,
          controller: _email,
          autofillHints: const [AutofillHints.email],
          keyboardType: TextInputType.emailAddress,
          textInputAction: TextInputAction.next,
          validator: (value) => validateEmail(l10n, value),
        ),
        LabeledTextField(
          label: l10n.loginPassword,
          controller: _password,
          autofillHints: const [AutofillHints.newPassword],
          obscureText: true,
          textInputAction: TextInputAction.next,
          autovalidateMode: AutovalidateMode.onUnfocus,
          validator: (value) => validatePassword(l10n, policy, value),
        ),
        LabeledTextField(
          label: l10n.registerConfirmPassword,
          controller: _confirmation,
          autofillHints: const [AutofillHints.newPassword],
          obscureText: true,
          onSubmitted: (_) => _submit(),
          validator: _validateConfirmation,
        ),
        SizedBox(height: spacing * 1.5),
        FilledButton(onPressed: submission.isLoading ? null : _submit, child: Text(l10n.registerSubmit)),
      ],
    );
  }
}
