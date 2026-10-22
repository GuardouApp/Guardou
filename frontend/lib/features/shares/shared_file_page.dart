import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../app/theme/app_theme.dart';
import '../../app/theme/app_tokens.dart';
import '../../core/providers/retry.dart';
import '../../core/widgets/brand_mark.dart';
import '../../l10n/generated/app_localizations.dart';
import '../viewer/viewer_frame.dart';
import 'shares_service.dart';

final sharedItemProvider = FutureProvider.autoDispose.family<ItemEntry, String>(
  (ref, token) => ref.read(sharesServiceProvider).sharedItem(token),
  retry: noRetry,
);

class SharedFilePage extends ConsumerWidget {
  const SharedFilePage({super.key, required this.token});

  final String token;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);

    return Theme(
      data: AppTheme.dark(),
      child: switch (ref.watch(sharedItemProvider(token))) {
        AsyncValue(hasValue: true, :final value?) => ViewerFrame(
          entry: value,
          source: ref.watch(sharesServiceProvider).files(token),
          leading: const _Brand(),
        ),
        AsyncValue(:final error?) => _SharedFileError(
          unavailable: error is DioException && error.response?.statusCode == 404,
          onRetry: () => ref.invalidate(sharedItemProvider(token)),
        ),
        _ => Scaffold(
          body: Center(child: CircularProgressIndicator(semanticsLabel: l10n.loading)),
        ),
      },
    );
  }
}

class _Brand extends StatelessWidget {
  const _Brand();

  @override
  Widget build(BuildContext context) => const Center(child: BrandMark());
}

class _SharedFileError extends StatelessWidget {
  const _SharedFileError({required this.unavailable, required this.onRetry});

  static const _iconSize = 64.0;

  final bool unavailable;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final spacing = AppTokens.of(context).spacing;

    return Scaffold(
      appBar: AppBar(leading: const _Brand()),
      body: Center(
        child: SingleChildScrollView(
          padding: EdgeInsets.all(spacing),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(unavailable ? LucideIcons.link2Off : LucideIcons.circleAlert, size: _iconSize),
              SizedBox(height: spacing),
              Text(
                unavailable ? l10n.sharedLinkUnavailable : l10n.genericError,
                style: Theme.of(context).textTheme.titleMedium,
                textAlign: TextAlign.center,
              ),
              if (!unavailable) ...[
                SizedBox(height: spacing),
                FilledButton(onPressed: onRetry, child: Text(l10n.errorRetry)),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
