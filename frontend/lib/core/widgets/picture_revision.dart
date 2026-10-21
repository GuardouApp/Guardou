import 'package:flutter_riverpod/flutter_riverpod.dart';

final pictureRevisionProvider = NotifierProvider<PictureRevision, int>(PictureRevision.new);

class PictureRevision extends Notifier<int> {
  static const parameter = 'v';

  @override
  int build() => 0;

  void advance() => state++;
}
