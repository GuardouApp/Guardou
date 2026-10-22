import '../files/file_source.dart';

class SharedFileSource extends FileSource {
  const SharedFileSource(super.dio, super.baseUri, this._token);

  final String _token;

  @override
  List<String> itemPath(String id) => ['api', 'shares', _token, 'items', id];
}
