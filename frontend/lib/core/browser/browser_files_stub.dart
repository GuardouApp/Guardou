import 'browser_files.dart';

BrowserFiles createBrowserFiles() => const _UnsupportedBrowserFiles();

class _UnsupportedBrowserFiles implements BrowserFiles {
  const _UnsupportedBrowserFiles();

  @override
  Future<List<LocalFile>> pickFiles() async => const [];

  @override
  Future<List<LocalFile>> pickFolder() async => const [];

  @override
  Future<LocalFile?> pickImage() async => null;

  @override
  void download(Uri uri) {}

  @override
  Stream<FileDragEvent> get dragEvents => const Stream.empty();
}
