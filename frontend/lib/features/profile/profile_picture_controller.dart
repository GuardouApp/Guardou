import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/browser/browser_files.dart';
import '../../core/widgets/picture_revision.dart';
import '../auth/session_controller.dart';
import 'profile_picture_failure.dart';
import 'profile_picture_service.dart';

final profilePictureControllerProvider = NotifierProvider<ProfilePictureController, bool>(ProfilePictureController.new);

class ProfilePictureController extends Notifier<bool> {
  @override
  bool build() => false;

  Future<void> replace(String userId, LocalFile file) async {
    if (file.size > maximumPictureBytes) {
      throw ProfilePictureFailure.tooLarge;
    }
    await _change(() => ref.read(profilePictureServiceProvider).replace(userId, file));
  }

  Future<void> remove(String userId) => _change(() => ref.read(profilePictureServiceProvider).delete(userId));

  Future<void> _change(Future<void> Function() change) async {
    state = true;
    try {
      await change();
      await ref.read(sessionControllerProvider.notifier).refresh();
      ref.read(pictureRevisionProvider.notifier).advance();
    } on DioException catch (error) {
      throw ProfilePictureFailure.of(error);
    } finally {
      state = false;
    }
  }
}
