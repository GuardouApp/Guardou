import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/files/folder_layouts.dart';

void main() {
  test('uses the fallback for folders without an override', () {
    const layouts = FolderLayouts(fallback: FileLayout.grid, overrides: {'docs': FileLayout.list});

    expect(layouts.of('docs'), FileLayout.list);
    expect(layouts.of('photos'), FileLayout.grid);
  });

  test('toggles a folder and makes the result the fallback', () {
    final layouts = const FolderLayouts().toggled('photos');

    expect(layouts.of('photos'), FileLayout.grid);
    expect(layouts.fallback, FileLayout.grid);
    expect(layouts.toggled('photos').of('photos'), FileLayout.list);
  });

  test('keeps only the most recent overrides', () {
    var layouts = const FolderLayouts();
    for (var index = 0; index <= FolderLayouts.maximumOverrides; index++) {
      layouts = layouts.toggled('folder-$index');
    }

    expect(layouts.overrides.length, FolderLayouts.maximumOverrides);
    expect(layouts.overrides.containsKey('folder-0'), isFalse);
    expect(layouts.overrides.containsKey('folder-${FolderLayouts.maximumOverrides}'), isTrue);
  });

  test('encodes and decodes the layouts', () {
    final layouts = const FolderLayouts().toggled('photos').toggled('docs');

    final decoded = FolderLayouts.decode(layouts.encode());

    expect(decoded.fallback, layouts.fallback);
    expect(decoded.overrides, layouts.overrides);
  });

  test('ignores invalid stored values', () {
    expect(FolderLayouts.decode(null).fallback, FileLayout.list);
    expect(FolderLayouts.decode('not json').overrides, isEmpty);
    expect(FolderLayouts.decode('{"fallback":"table","overrides":{"a":"grid","b":"table","c":1}}').overrides, {
      'a': FileLayout.grid,
    });
  });
}
