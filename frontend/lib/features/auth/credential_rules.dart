import '../../api/generated/models/password_policy.dart';

enum PasswordRule { minimumLength, digit, lowercase, uppercase, nonAlphanumeric }

final _username = RegExp(r'^[A-Za-z0-9]+$');
final _email = RegExp(r'^[^@\s]+@[^@\s]+$');
final _digit = RegExp('[0-9]');
final _lowercase = RegExp('[a-z]');
final _uppercase = RegExp('[A-Z]');
final _nonAlphanumeric = RegExp('[^A-Za-z0-9]');

const maximumDisplayNameLength = 100;

bool isValidUsername(String username) => _username.hasMatch(username);

bool isValidEmail(String email) => _email.hasMatch(email);

List<PasswordRule> passwordRules(PasswordPolicy policy) => [
  PasswordRule.minimumLength,
  if (policy.requireDigit) PasswordRule.digit,
  if (policy.requireLowercase) PasswordRule.lowercase,
  if (policy.requireUppercase) PasswordRule.uppercase,
  if (policy.requireNonAlphanumeric) PasswordRule.nonAlphanumeric,
];

bool isPasswordRuleMet(PasswordPolicy policy, PasswordRule rule, String password) => switch (rule) {
  PasswordRule.minimumLength => password.length >= policy.minimumLength,
  PasswordRule.digit => _digit.hasMatch(password),
  PasswordRule.lowercase => _lowercase.hasMatch(password),
  PasswordRule.uppercase => _uppercase.hasMatch(password),
  PasswordRule.nonAlphanumeric => _nonAlphanumeric.hasMatch(password),
};

List<PasswordRule> brokenPasswordRules(PasswordPolicy policy, String password) => [
  for (final rule in passwordRules(policy))
    if (!isPasswordRuleMet(policy, rule, password)) rule,
];
