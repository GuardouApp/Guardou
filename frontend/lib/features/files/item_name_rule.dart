abstract final class ItemNameRule {
  static const maximumLength = 255;

  static const _reserved = {'.', '..'};
  static const _separators = {'/', r'\'};
  static const _firstPrintable = 0x20;
  static const _delete = 0x7F;

  static bool isValid(String name) =>
      name.isNotEmpty &&
      name.length <= maximumLength &&
      name.trim() == name &&
      !_reserved.contains(name) &&
      !_separators.any(name.contains) &&
      !name.runes.any(_isControl);

  static int baseNameLength(String name) {
    final dot = name.lastIndexOf('.');
    return dot > 0 ? dot : name.length;
  }

  static bool _isControl(int rune) => rune < _firstPrintable || rune == _delete;
}
