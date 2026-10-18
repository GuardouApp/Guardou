import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import '../configuration/configuration_controller.dart';
import 'auth_card.dart';
import 'auth_form_controller.dart';

class LoginPage extends ConsumerStatefulWidget {
  const LoginPage({super.key});

  @override
  ConsumerState<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends ConsumerState<LoginPage> {
  final _formKey = GlobalKey<FormState>();
  final _username = TextEditingController();
  final _password = TextEditingController();
  bool _rememberMe = false;

  @override
  void dispose() {
    _username.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!(_formKey.currentState?.validate() ?? false)) {
      return;
    }
    await ref
        .read(authFormControllerProvider.notifier)
        .login(username: _username.text, password: _password.text, rememberMe: _rememberMe);
  }

  String? _required(String? value) =>
      value == null || value.isEmpty ? AppLocalizations.of(context).loginValueRequired : null;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final submission = ref.watch(authFormControllerProvider);
    final applicationName = ref.watch(configurationControllerProvider).value?.applicationName ?? '';

    return AuthCard(
      formKey: _formKey,
      title: l10n.loginTitle(applicationName),
      failure: switch (submission.error) {
        final AuthFailure failure => failure,
        _ => null,
      },
      children: [
        TextFormField(
          controller: _username,
          decoration: InputDecoration(labelText: l10n.loginUsername),
          autofillHints: const [AutofillHints.username],
          textInputAction: TextInputAction.next,
          validator: _required,
        ),
        SizedBox(height: AppTokens.of(context).spacing),
        TextFormField(
          controller: _password,
          decoration: InputDecoration(labelText: l10n.loginPassword),
          autofillHints: const [AutofillHints.password],
          obscureText: true,
          onFieldSubmitted: (_) => _submit(),
          validator: _required,
        ),
        CheckboxListTile(
          value: _rememberMe,
          onChanged: (value) => setState(() => _rememberMe = value ?? false),
          title: Text(l10n.loginRememberMe),
          controlAffinity: ListTileControlAffinity.leading,
          contentPadding: EdgeInsets.zero,
        ),
        FilledButton(onPressed: submission.isLoading ? null : _submit, child: Text(l10n.loginSubmit)),
      ],
    );
  }
}
