import 'package:flutter_riverpod/flutter_riverpod.dart';

final folderTreeExpandedProvider = NotifierProvider<FolderTreeController, bool>(FolderTreeController.new);

class FolderTreeController extends Notifier<bool> {
  @override
  bool build() => false;

  void toggle() => state = !state;
}
