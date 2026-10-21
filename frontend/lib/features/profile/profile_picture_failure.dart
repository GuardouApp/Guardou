import 'package:dio/dio.dart';

import '../../core/api/api_problem.dart';
import '../../l10n/generated/app_localizations.dart';

const maximumPictureMegabytes = 5;
const maximumPictureBytes = maximumPictureMegabytes * 1024 * 1024;

enum ProfilePictureFailure implements Exception {
  tooLarge,
  invalid,
  unknown;

  static ProfilePictureFailure of(DioException error) => switch (error.error) {
    ApiProblem(status: 413) => tooLarge,
    ApiProblem(status: 400) => invalid,
    _ => unknown,
  };

  String message(AppLocalizations l10n) => switch (this) {
    tooLarge => l10n.profilePictureTooLarge(maximumPictureMegabytes),
    invalid => l10n.profilePictureInvalid,
    unknown => l10n.genericError,
  };
}
