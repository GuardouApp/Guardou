import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/storage_usage.dart';
import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import 'file_presentation.dart';
import 'storage_usage_controller.dart';

class StorageUsageIndicator extends ConsumerWidget {
  const StorageUsageIndicator({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final spacing = AppTokens.of(context).spacing;
    final usage = ref.watch(storageUsageControllerProvider).value;
    if (usage == null) {
      return const SizedBox.shrink();
    }

    return Padding(
      padding: EdgeInsets.symmetric(horizontal: spacing * 0.75),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Divider(),
          SizedBox(height: spacing / 2),
          _StorageUsageBar(usage: usage),
        ],
      ),
    );
  }
}

class _StorageUsageBar extends StatelessWidget {
  const _StorageUsageBar({required this.usage});

  final StorageUsage usage;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final theme = Theme.of(context);
    final spacing = AppTokens.of(context).spacing;
    final limit = usage.quotaBytes ?? usage.totalBytes;
    final ratio = limit <= 0 ? 1.0 : usage.usedBytes / limit;

    return Padding(
      padding: EdgeInsets.fromLTRB(spacing, 0, spacing, spacing),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          LinearProgressIndicator(
            value: ratio.clamp(0, 1),
            color: ratio >= 1 ? theme.colorScheme.error : null,
            borderRadius: BorderRadius.circular(4),
          ),
          const SizedBox(height: 4),
          Text(
            l10n.storageUsage(formatSize(usage.usedBytes, l10n.localeName), formatSize(limit, l10n.localeName)),
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}
