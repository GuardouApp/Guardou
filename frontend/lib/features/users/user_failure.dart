import 'package:dio/dio.dart';

import '../../core/api/api_problem.dart';
import '../../l10n/generated/app_localizations.dart';

enum UserFailure {
  usernameTaken,
  usernameInvalid,
  emailTaken,
  emailInvalid,
  passwordRejected,
  lastAdministrator,
  selfDeletion,
  notFound,
  unknown;

  static UserFailure of(DioException error) => switch (error.error) {
    ApiProblem(status: 404) => notFound,
    ApiProblem(:final codes) => codes.map(_fromCode).firstWhere((failure) => failure != unknown, orElse: () => unknown),
    _ => unknown,
  };

  static UserFailure _fromCode(String code) => switch (code) {
    'DuplicateUserName' => usernameTaken,
    'InvalidUserName' => usernameInvalid,
    'DuplicateEmail' => emailTaken,
    'InvalidEmail' => emailInvalid,
    'LastAdministrator' => lastAdministrator,
    'SelfDeletion' => selfDeletion,
    _ when code.startsWith('Password') => passwordRejected,
    _ => unknown,
  };

  String message(AppLocalizations l10n) => switch (this) {
    usernameTaken => l10n.userUsernameTaken,
    usernameInvalid => l10n.registerUsernameInvalid,
    emailTaken => l10n.userEmailTaken,
    emailInvalid => l10n.registerEmailInvalid,
    passwordRejected => l10n.userPasswordRejected,
    lastAdministrator => l10n.userLastAdministrator,
    selfDeletion => l10n.userSelfDeletion,
    notFound => l10n.userNotFoundMessage,
    unknown => l10n.genericError,
  };
}
