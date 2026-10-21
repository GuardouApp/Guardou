import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../api/api_providers.dart';

class UserAvatar extends ConsumerWidget {
  const UserAvatar({
    super.key,
    required this.userId,
    required this.name,
    required this.hasPicture,
    required this.radius,
  });

  final String userId;
  final String name;
  final bool hasPicture;
  final double radius;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final initial = CircleAvatar(
      radius: radius,
      child: Text(name.characters.firstOrNull?.toUpperCase() ?? '', style: TextStyle(fontSize: radius * 0.9)),
    );
    if (!hasPicture) {
      return initial;
    }
    final url = Uri.parse(ref.watch(apiBaseUrlProvider)).replace(pathSegments: ['api', 'users', userId, 'picture']);
    return ClipOval(
      child: Image.network(
        url.toString(),
        width: radius * 2,
        height: radius * 2,
        fit: BoxFit.cover,
        webHtmlElementStrategy: WebHtmlElementStrategy.fallback,
        errorBuilder: (_, _, _) => initial,
      ),
    );
  }
}
