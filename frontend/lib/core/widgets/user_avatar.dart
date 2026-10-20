import 'package:flutter/material.dart';

class UserAvatar extends StatelessWidget {
  const UserAvatar({super.key, required this.username, required this.radius, this.pictureUrl});

  final String username;
  final double radius;
  final String? pictureUrl;

  @override
  Widget build(BuildContext context) {
    final initial = CircleAvatar(
      radius: radius,
      child: Text(username.characters.firstOrNull?.toUpperCase() ?? '', style: TextStyle(fontSize: radius * 0.9)),
    );
    final url = pictureUrl;
    if (url == null) {
      return initial;
    }
    return ClipOval(
      child: Image.network(
        url,
        width: radius * 2,
        height: radius * 2,
        fit: BoxFit.cover,
        webHtmlElementStrategy: WebHtmlElementStrategy.fallback,
        errorBuilder: (_, _, _) => initial,
      ),
    );
  }
}
