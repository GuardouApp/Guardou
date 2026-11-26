import 'package:flutter/material.dart';

import '../../l10n/generated/app_localizations.dart';
import '../settings/settings_section.dart';
import 'new_password_fields.dart';

class PasswordChangeFields extends StatelessWidget {
  const PasswordChangeFields({
    super.key,
    required this.current,
    required this.password,
    required this.confirmation,
    required this.onSubmitted,
  });

  final TextEditingController current;
  final TextEditingController password;
  final TextEditingController confirmation;
  final VoidCallback onSubmitted;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        SettingsTextField(
          label: l10n.passwordCurrent,
          required: true,
          controller: current,
          autofillHints: const [AutofillHints.password],
          obscureText: true,
          textInputAction: TextInputAction.next,
          validator: (value) => value == null || value.isEmpty ? l10n.loginValueRequired : null,
        ),
        NewPasswordFields(
          label: l10n.passwordNew,
          password: password,
          confirmation: confirmation,
          onSubmitted: onSubmitted,
        ),
      ],
    );
  }
}

class PasswordChangeControllers {
  final current = TextEditingController();
  final password = TextEditingController();
  final confirmation = TextEditingController();

  void clear() {
    current.clear();
    password.clear();
    confirmation.clear();
  }

  void dispose() {
    current.dispose();
    password.dispose();
    confirmation.dispose();
  }
}
