import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../app/theme/app_tokens.dart';
import 'operation_status.dart';

class StatusBar extends ConsumerWidget {
  const StatusBar({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final operations = ref.watch(operationStatusProvider);
    if (operations.isEmpty) {
      return const SizedBox.shrink();
    }
    final spacing = AppTokens.of(context).spacing;

    return Material(
      color: Theme.of(context).colorScheme.surfaceContainerHigh,
      child: SafeArea(
        top: false,
        child: Padding(
          padding: EdgeInsets.symmetric(horizontal: spacing, vertical: spacing / 2),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              for (final operation in operations)
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 4),
                  child: Row(
                    children: [
                      Expanded(child: Text(operation.label, overflow: TextOverflow.ellipsis)),
                      SizedBox(width: spacing),
                      SizedBox(width: 120, child: LinearProgressIndicator(value: operation.progress)),
                    ],
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }
}
