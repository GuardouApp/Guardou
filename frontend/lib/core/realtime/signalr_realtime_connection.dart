import 'dart:async';

import 'package:signalr_netcore/iretry_policy.dart';
import 'package:signalr_netcore/signalr_client.dart';

import 'realtime_connection.dart';
import 'realtime_event.dart';

class SignalRRealtimeConnection implements RealtimeConnection {
  SignalRRealtimeConnection(String url)
    : _hub = HubConnectionBuilder().withUrl(url).withAutomaticReconnect(reconnectPolicy: const _EndlessRetry()).build() {
    _on('ItemsChanged', ItemsChanged.fromJson);
    _on('NotificationReceived', NotificationReceived.fromJson);
    _hub.on('SharesChanged', (_) => _events.add(const SharesChanged()));
    _hub.onreconnecting(({error}) => _statuses.add(RealtimeStatus.reconnecting));
    _hub.onreconnected(({connectionId}) => _statuses.add(RealtimeStatus.connected));
    _hub.onclose(({error}) => _retryLater());
  }

  static const _startRetryDelay = Duration(seconds: 10);

  final HubConnection _hub;
  final _events = StreamController<RealtimeEvent>.broadcast();
  final _statuses = StreamController<RealtimeStatus>.broadcast();
  Timer? _retry;
  bool _stopped = false;

  @override
  Stream<RealtimeEvent> get events => _events.stream;

  @override
  Stream<RealtimeStatus> get statuses => _statuses.stream;

  @override
  Future<void> start() async {
    _stopped = false;
    _statuses.add(RealtimeStatus.connecting);
    await _connect();
  }

  @override
  Future<void> stop() async {
    _stopped = true;
    _retry?.cancel();
    await _hub.stop();
    await _events.close();
    await _statuses.close();
  }

  void _on(String method, RealtimeEvent Function(Map<String, Object?> json) parse) => _hub.on(method, (arguments) {
    if (arguments?.firstOrNull case final Map<String, Object?> json) {
      _events.add(parse(json));
    }
  });

  Future<void> _connect() async {
    if (_stopped || _hub.state != HubConnectionState.Disconnected) {
      return;
    }
    try {
      await _hub.start();
      _statuses.add(RealtimeStatus.connected);
    } on Exception {
      _retryLater();
    }
  }

  void _retryLater() {
    if (_stopped) {
      return;
    }
    _statuses.add(RealtimeStatus.reconnecting);
    _retry?.cancel();
    _retry = Timer(_startRetryDelay, () => unawaited(_connect()));
  }
}

class _EndlessRetry implements IRetryPolicy {
  const _EndlessRetry();

  static const _delays = [0, 2000, 5000, 10000];
  static const _maximumDelay = 30000;

  @override
  int? nextRetryDelayInMilliseconds(RetryContext retryContext) =>
      retryContext.previousRetryCount < _delays.length ? _delays[retryContext.previousRetryCount] : _maximumDelay;
}
