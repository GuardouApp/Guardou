import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/users/user_failure.dart';

import '../../helpers/test_app.dart';

void main() {
  UserFailure failureOf({int status = 400, List<String> codes = const []}) =>
      UserFailure.of(testProblem('/api/users', status: status, codes: codes));

  test('reads the failure from the problem codes', () {
    expect(failureOf(codes: ['DuplicateUserName']), UserFailure.usernameTaken);
    expect(failureOf(codes: ['DuplicateEmail']), UserFailure.emailTaken);
    expect(failureOf(codes: ['InvalidUserName']), UserFailure.usernameInvalid);
    expect(failureOf(codes: ['InvalidEmail']), UserFailure.emailInvalid);
    expect(failureOf(codes: ['PasswordTooShort', 'PasswordRequiresDigit']), UserFailure.passwordRejected);
    expect(failureOf(codes: ['LastAdministrator']), UserFailure.lastAdministrator);
    expect(failureOf(codes: ['SelfDeletion']), UserFailure.selfDeletion);
  });

  test('skips unknown codes', () {
    expect(failureOf(codes: ['Other', 'DuplicateEmail']), UserFailure.emailTaken);
    expect(failureOf(codes: ['Other']), UserFailure.unknown);
    expect(failureOf(), UserFailure.unknown);
  });

  test('reads a missing user from the status', () {
    expect(failureOf(status: 404), UserFailure.notFound);
  });
}
