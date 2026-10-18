import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/password_policy.dart';
import 'package:guardou/features/auth/credential_rules.dart';

void main() {
  test('accepts only alphanumeric usernames', () {
    expect(isValidUsername('Alice42'), isTrue);
    expect(isValidUsername(''), isFalse);
    expect(isValidUsername('john.doe'), isFalse);
    expect(isValidUsername('jöhn'), isFalse);
  });

  test('accepts emails with one at sign', () {
    expect(isValidEmail('alice@example.com'), isTrue);
    expect(isValidEmail('alice'), isFalse);
    expect(isValidEmail('alice@'), isFalse);
    expect(isValidEmail('a@b@c'), isFalse);
  });

  test('reports the broken password rules', () {
    const policy = PasswordPolicy(
      minimumLength: 8,
      requireDigit: true,
      requireLowercase: true,
      requireUppercase: true,
      requireNonAlphanumeric: true,
    );

    expect(brokenPasswordRules(policy, 'abc'), [
      PasswordRule.minimumLength,
      PasswordRule.digit,
      PasswordRule.uppercase,
      PasswordRule.nonAlphanumeric,
    ]);
    expect(brokenPasswordRules(policy, 'Abcdef1!'), isEmpty);
  });
}
