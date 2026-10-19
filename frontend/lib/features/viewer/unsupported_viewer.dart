import 'package:flutter/material.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../app/theme/app_tokens.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/file_actions.dart';
import '../files/file_presentation.dart';

class UnsupportedViewer extends StatelessWidget {
  const UnsupportedViewer({super.key, required this.entry});

  static const _iconSize = 64.0;

  final ItemEntry entry;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final spacing = AppTokens.of(context).spacing;
    final actions = FileActions(context);

    return Center(
      child: SingleChildScrollView(
        padding: EdgeInsets.all(spacing),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(iconFor(entry), size: _iconSize),
            SizedBox(height: spacing),
            Text(l10n.viewerUnsupported, style: Theme.of(context).textTheme.titleMedium, textAlign: TextAlign.center),
            SizedBox(height: spacing),
            Wrap(
              spacing: spacing / 2,
              runSpacing: spacing / 2,
              alignment: WrapAlignment.center,
              children: [
                FilledButton.icon(
                  onPressed: () => actions.download([entry]),
                  icon: const Icon(LucideIcons.download),
                  label: Text(l10n.actionDownload),
                ),
                OutlinedButton.icon(
                  onPressed: () => actions.openInNewTab(entry),
                  icon: const Icon(LucideIcons.externalLink),
                  label: Text(l10n.actionOpenInNewTab),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
