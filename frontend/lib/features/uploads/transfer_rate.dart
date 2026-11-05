class TransferRate {
  TransferRate(this._bytes);

  static const _interval = Duration(milliseconds: 250);
  static const _smoothing = 0.3;
  static const _microsecondsPerSecond = 1000000;

  final _clock = Stopwatch()..start();
  int _bytes;
  Duration _sampled = Duration.zero;
  double _bytesPerSecond = 0;

  double? record(int bytes) {
    final now = _clock.elapsed;
    final elapsed = now - _sampled;
    if (elapsed < _interval) {
      return null;
    }
    final current = (bytes - _bytes) * _microsecondsPerSecond / elapsed.inMicroseconds;
    _bytesPerSecond = _bytesPerSecond == 0 ? current : _bytesPerSecond + (current - _bytesPerSecond) * _smoothing;
    _bytes = bytes;
    _sampled = now;
    return _bytesPerSecond;
  }
}
