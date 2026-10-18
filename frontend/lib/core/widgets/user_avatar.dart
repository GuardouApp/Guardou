import 'package:flutter/material.dart';

class UserAvatar extends StatelessWidget {
  const UserAvatar({super.key, required this.username, required this.radius});

  final String username;
  final double radius;

  @override
  Widget build(BuildContext context) {
    return CircleAvatar(
      radius: radius,
      child: Text(username.characters.firstOrNull?.toUpperCase() ?? '', style: TextStyle(fontSize: radius * 0.9)),
    );
  }
}
