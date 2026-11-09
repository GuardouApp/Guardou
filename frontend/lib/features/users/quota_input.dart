import 'package:intl/intl.dart';

abstract final class QuotaInput {
  static const _bytesPerGigabyte = 1024 * 1024 * 1024;

  static String format(int? bytes) =>
      bytes == null ? '' : NumberFormat('0.###', 'en').format(bytes / _bytesPerGigabyte);

  static bool isValid(String text) => text.trim().isEmpty || (_gigabytes(text) ?? -1) >= 0;

  static int? bytesOf(String text) => switch (_gigabytes(text)) {
    final double gigabytes => (gigabytes * _bytesPerGigabyte).round(),
    null => null,
  };

  static double? _gigabytes(String text) => double.tryParse(text.trim().replaceAll(',', '.'));
}
