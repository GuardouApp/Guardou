import '../../api/generated/models/item_entry.dart';
import '../files/file_arrangement.dart';

enum ViewerKind {
  image,
  video,
  audio,
  pdf,
  unsupported;

  static const _pdfExtension = 'pdf';

  bool get isMedia => this == image || this == video || this == audio;

  static ViewerKind of(ItemEntry entry) {
    final extension = extensionOf(entry);
    return switch ((entry.mimeType ?? '').split('/').first) {
      'image' => image,
      'video' => video,
      'audio' => audio,
      _ when extension == _pdfExtension => pdf,
      _ => unsupported,
    };
  }
}
