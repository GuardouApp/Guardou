import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/features/viewer/viewer_kind.dart';

import '../../helpers/test_app.dart';

void main() {
  test('selects the viewer from the MIME type and the extension', () {
    expect(ViewerKind.of(testEntry('photo.heic', mimeType: 'image/heic')), ViewerKind.image);
    expect(ViewerKind.of(testEntry('clip.mp4', mimeType: 'video/mp4')), ViewerKind.video);
    expect(ViewerKind.of(testEntry('song.mp3', mimeType: 'audio/mpeg')), ViewerKind.audio);
    expect(ViewerKind.of(testEntry('manual.PDF', mimeType: 'application/pdf')), ViewerKind.pdf);
    expect(ViewerKind.of(testEntry('data.bin', mimeType: 'application/octet-stream')), ViewerKind.unsupported);
  });

  test('treats only images, videos and audio as media', () {
    expect(
      [
        for (final kind in ViewerKind.values)
          if (kind.isMedia) kind,
      ],
      [ViewerKind.image, ViewerKind.video, ViewerKind.audio],
    );
  });
}
