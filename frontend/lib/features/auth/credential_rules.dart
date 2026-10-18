import '../../api/generated/models/password_policy.dart';

enum PasswordRule { minimumLength, digit, lowercase, uppercase, nonAlphanumeric }

final _username = RegExp(r'^[A-Za-z0-9]+$');
final _email = RegExp(r'^[^@\s]+@[^@\s]+$');
final _digit = RegExp('[0-9]');
final _lowercase = RegExp('[a-z]');
final _uppercase = RegExp('[A-Z]');
final _nonAlphanumeric = RegExp('[^A-Za-z0-9]');

bool isValidUsername(String username) => _username.hasMatch(username);

bool isValidEmail(String email) => _email.hasMatch(email);

List<PasswordRule> brokenPasswordRules(PasswordPolicy policy, String password) => [
  if (password.length < policy.minimumLength) PasswordRule.minimumLength,
  if (policy.requireDigit && !_digit.hasMatch(password)) PasswordRule.digit,
  if (policy.requireLowercase && !_lowercase.hasMatch(password)) PasswordRule.lowercase,
  if (policy.requireUppercase && !_uppercase.hasMatch(password)) PasswordRule.uppercase,
  if (policy.requireNonAlphanumeric && !_nonAlphanumeric.hasMatch(password)) PasswordRule.nonAlphanumeric,
];
