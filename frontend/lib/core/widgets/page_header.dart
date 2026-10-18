import 'package:flutter/material.dart';

import '../../app/theme/app_tokens.dart';
import '../layout/window_size_class.dart';

class PageHeader extends StatelessWidget {
  const PageHeader({super.key, required this.child});

  static const _height = 64.0;

  final Widget child;

  static TextStyle? _titleStyle(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    return WindowSizeClass.of(context) == WindowSizeClass.compact ? textTheme.titleMedium : textTheme.titleLarge;
  }

  @override
  Widget build(BuildContext context) {
    final spacing = AppTokens.of(context).spacing;
    return SizedBox(
      height: _height,
      child: Padding(
        padding: EdgeInsets.symmetric(horizontal: spacing),
        child: Align(
          alignment: AlignmentDirectional.centerStart,
          child: DefaultTextStyle.merge(
            style: _titleStyle(context)?.copyWith(color: Theme.of(context).colorScheme.onSurface),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            child: child,
          ),
        ),
      ),
    );
  }
}
