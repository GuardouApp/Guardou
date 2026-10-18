import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/core/layout/window_size_class.dart';

void main() {
  test('maps widths to window size classes', () {
    expect(WindowSizeClass.fromWidth(360), WindowSizeClass.compact);
    expect(WindowSizeClass.fromWidth(599), WindowSizeClass.compact);
    expect(WindowSizeClass.fromWidth(600), WindowSizeClass.medium);
    expect(WindowSizeClass.fromWidth(839), WindowSizeClass.medium);
    expect(WindowSizeClass.fromWidth(840), WindowSizeClass.expanded);
  });
}
