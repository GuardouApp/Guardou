import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/item_entry.dart';
import 'package:guardou/features/search/search_controller.dart';
import 'package:guardou/api/generated/models/item_type.dart';
import 'package:guardou/features/search/search_service.dart';
import 'package:mocktail/mocktail.dart';

class _MockSearchService extends Mock implements SearchService {}

void main() {
  late _MockSearchService service;
  late ProviderContainer container;

  ItemSearchController controller() => container.read(itemSearchControllerProvider.notifier);
  ItemSearchState state() => container.read(itemSearchControllerProvider);

  setUp(() {
    service = _MockSearchService();
    when(() => service.search(any(), folderId: any(named: 'folderId'))).thenAnswer((_) async => <ItemEntry>[]);
    container = ProviderContainer(overrides: [searchServiceProvider.overrideWithValue(service)]);
  });

  tearDown(() => container.dispose());

  test('waits for the minimum length', () async {
    controller().setText('a');
    await Future<void>.delayed(ItemSearchController.debounce * 2);

    expect(state().results, isNull);
    verifyNever(() => service.search(any(), folderId: any(named: 'folderId')));
  });

  test('searches once after the user stops typing', () async {
    controller()
      ..setText('re')
      ..setText('rep')
      ..setText('report');

    expect(state().results, isA<AsyncLoading<List<ItemEntry>>>());
    await Future<void>.delayed(ItemSearchController.debounce * 2);

    verify(() => service.search('report')).called(1);
    expect(state().results, isA<AsyncData<List<ItemEntry>>>());
  });

  test('searches with only a filter', () async {
    controller().toggleType(ItemType.image);
    await Future<void>.delayed(ItemSearchController.debounce * 2);

    verify(() => service.search('type:image')).called(1);
  });

  test('searches the current folder only when selected', () async {
    controller()
      ..setFolder('folder')
      ..setText('plan');
    await Future<void>.delayed(ItemSearchController.debounce * 2);
    controller().setFolderOnly(true);
    await Future<void>.delayed(ItemSearchController.debounce * 2);

    verify(() => service.search('plan')).called(1);
    verify(() => service.search('plan', folderId: 'folder')).called(1);
  });
}
