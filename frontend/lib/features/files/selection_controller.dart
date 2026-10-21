import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'selection.dart';

final selectionControllerProvider = NotifierProvider.autoDispose.family<SelectionController, Selection, String>(
  SelectionController.new,
);

class SelectionController extends Notifier<Selection> {
  SelectionController(this.scope);

  final String scope;

  @override
  Selection build() => Selection.empty;

  void select(String id) => state = state.only(id);

  void toggle(String id) => state = state.toggle(id);

  void extendTo(String id, List<String> order) => state = state.extendTo(id, order);

  void selectAll(List<String> order) => state = state.all(order);

  void startCheckboxMode(String id) => state = (state.contains(id) ? state : state.toggle(id)).withCheckboxMode(true);

  void replace(Selection selection) => state = selection;

  void retainOnly(Iterable<String> available) => state = state.retainOnly(available);

  void clear() => state = Selection.empty;
}
