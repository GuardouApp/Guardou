import 'package:flutter/material.dart';

import '../../app/theme/app_tokens.dart';
import '../../core/widgets/page_header.dart';

class SettingsLayout extends StatelessWidget {
  const SettingsLayout({super.key, required this.title, required this.sections});

  static const _maxWidth = 800.0;

  final String title;
  final List<Widget> sections;

  @override
  Widget build(BuildContext context) {
    final spacing = AppTokens.of(context).spacing;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(child: Text(title)),
        const Divider(height: 1),
        Expanded(
          child: Material(
            type: MaterialType.transparency,
            child: ListView(
              padding: EdgeInsets.all(spacing * 1.5),
              children: [
                for (final section in sections)
                  Center(
                    child: ConstrainedBox(
                      constraints: const BoxConstraints(maxWidth: _maxWidth),
                      child: Padding(
                        padding: EdgeInsets.only(bottom: spacing),
                        child: section,
                      ),
                    ),
                  ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}
