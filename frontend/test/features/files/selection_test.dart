import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/files/selection.dart';

void main() {
  const order = ['a', 'b', 'c', 'd'];

  test('selects only one item', () {
    final selection = Selection.empty.only('a').only('b');

    expect(selection.ids, {'b'});
    expect(selection.anchor, 'b');
  });

  test('toggles items', () {
    final selection = Selection.empty.toggle('a').toggle('b').toggle('a');

    expect(selection.ids, {'b'});
  });

  test('extends the selection from the anchor in both directions', () {
    expect(Selection.empty.only('b').extendTo('d', order).ids, {'b', 'c', 'd'});
    expect(Selection.empty.only('c').extendTo('a', order).ids, {'a', 'b', 'c'});
  });

  test('selects all items', () {
    expect(Selection.empty.all(order).ids, order.toSet());
  });

  test('leaves the checkbox mode when the last item is cleared', () {
    final selection = Selection.empty.toggle('a').withCheckboxMode(true);

    expect(selection.checkboxMode, isTrue);
    expect(selection.toggle('a').checkboxMode, isFalse);
  });

  test('keeps only the items that still exist', () {
    final selection = Selection.empty.all(order).retainOnly(['b', 'x']);

    expect(selection.ids, {'b'});
    expect(selection.anchor, isNull);
  });
}
