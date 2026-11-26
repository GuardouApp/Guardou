import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../app/router/routes.dart';
import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import '../configuration/configuration_controller.dart';
import '../settings/settings_section.dart';
import 'auth_card.dart';
import 'auth_form_controller.dart';
import 'credential_validators.dart';
import 'new_password_fields.dart';

enum RegistrationKind { firstAdministrator, signUp }

class RegisterPage extends ConsumerStatefulWidget {
  const RegisterPage({super.key, required this.kind});

  final RegistrationKind kind;

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
    final form = ref.read(authFormControllerProvider.notifier);
    final username = _username.text;
    final displayName = _displayName.text.trim();
    final email = _email.text.trim();
    final password = _password.text;
    await switch (widget.kind) {
      RegistrationKind.firstAdministrator => form.register(
        username: username,
        displayName: displayName,
        email: email,
        password: password,
      ),
      RegistrationKind.signUp => form.signUp(
        username: username,
        displayName: displayName,
        email: email,
        password: password,
      ),
    };
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final spacing = AppTokens.of(context).spacing;
    final submission = ref.watch(authFormControllerProvider);
    final applicationName = ref.watch(configurationControllerProvider).value?.applicationName ?? '';
    final signUp = widget.kind == RegistrationKind.signUp;

    return AuthCard(
      formKey: _formKey,
      title: signUp ? l10n.signUpTitle(applicationName) : l10n.registerTitle,
      subtitle: signUp ? null : l10n.registerSubtitle,
      failure: switch (submission.error) {
        final AuthFailure failure => failure,
        _ => null,
      },
      children: [
        SettingsTextField(
          label: l10n.loginUsername,
          required: true,
          controller: _username,
          autofillHints: const [AutofillHints.newUsername],
          textInputAction: TextInputAction.next,
          validator: (value) => validateUsername(l10n, value),
        ),
        SettingsTextField(
          label: l10n.displayName,
          required: true,
          controller: _displayName,
          autofillHints: const [AutofillHints.name],
          textInputAction: TextInputAction.next,
          validator: (value) => validateDisplayName(l10n, value),
        ),
        SettingsTextField(
          label: l10n.registerEmail,
          required: true,
          controller: _email,
          autofillHints: const [AutofillHints.email],
          keyboardType: TextInputType.emailAddress,
          textInputAction: TextInputAction.next,
          validator: (value) => validateEmail(l10n, value),
        ),
        NewPasswordFields(
          label: l10n.loginPassword,
          password: _password,
          confirmation: _confirmation,
          onSubmitted: _submit,
        ),
        SizedBox(height: spacing * 1.5),
        FilledButton(onPressed: submission.isLoading ? null : _submit, child: Text(l10n.registerSubmit)),
        if (signUp) ...[
          SizedBox(height: spacing / 2),
          TextButton(onPressed: () => context.go(Routes.login), child: Text(l10n.signUpSignInInstead)),
        ],
      ],
    );
  }
}
