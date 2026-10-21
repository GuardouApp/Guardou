import '../../api/generated/models/password_policy.dart';
import '../../l10n/generated/app_localizations.dart';
import 'credential_rules.dart';

String? validateUsername(AppLocalizations l10n, String? value) {
  if (value == null || value.isEmpty) {
    return l10n.loginValueRequired;
  }
  return isValidUsername(value) ? null : l10n.registerUsernameInvalid;
}

String? validateDisplayName(AppLocalizations l10n, String? value) {
  final displayName = value?.trim() ?? '';
  if (displayName.isEmpty) {
    return l10n.loginValueRequired;
  }
  return displayName.length <= maximumDisplayNameLength ? null : l10n.displayNameInvalid(maximumDisplayNameLength);
}

String? validateEmail(AppLocalizations l10n, String? value) {
  final email = value?.trim() ?? '';
  if (email.isEmpty) {
    return l10n.loginValueRequired;
  }
  return isValidEmail(email) ? null : l10n.registerEmailInvalid;
}

String? validatePassword(AppLocalizations l10n, PasswordPolicy? policy, String? value) {
  if (policy == null) {
    return null;
  }
  final messages = [
    for (final rule in brokenPasswordRules(policy, value ?? ''))
      switch (rule) {
        PasswordRule.minimumLength => l10n.registerPasswordMinimumLength(policy.minimumLength),
        PasswordRule.digit => l10n.registerPasswordDigit,
        PasswordRule.lowercase => l10n.registerPasswordLowercase,
        PasswordRule.uppercase => l10n.registerPasswordUppercase,
        PasswordRule.nonAlphanumeric => l10n.registerPasswordNonAlphanumeric,
      },
  ];
  return messages.isEmpty ? null : messages.join('\n');
}
