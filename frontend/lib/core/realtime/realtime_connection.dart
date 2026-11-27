import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../api/api_providers.dart';
import 'realtime_event.dart';
import 'signalr_realtime_connection.dart';

enum RealtimeStatus { disconnected, connecting, connected, reconnecting }

abstract interface class RealtimeConnection {
  Stream<RealtimeEvent> get events;

  Stream<RealtimeStatus> get statuses;

  Future<void> start();

  Future<void> stop();
}

typedef RealtimeConnectionFactory = RealtimeConnection Function();

const realtimeHubPath = '/api/events';

final realtimeConnectionFactoryProvider = Provider<RealtimeConnectionFactory>((ref) {
  final url = Uri.parse(ref.watch(apiBaseUrlProvider)).resolve(realtimeHubPath).toString();
  return () => SignalRRealtimeConnection(url);
});
