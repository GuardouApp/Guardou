import 'package:flutter_riverpod/flutter_riverpod.dart';

final detailsPanelProvider = NotifierProvider<DetailsPanelController, bool>(DetailsPanelController.new);

class DetailsPanelController extends Notifier<bool> {
  @override
  bool build() => false;

  void toggle() => state = !state;

  void show() => state = true;

  void hide() => state = false;
}
