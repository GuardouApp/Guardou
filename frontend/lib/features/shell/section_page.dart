import 'package:flutter/material.dart';

import '../../app/theme/app_tokens.dart';
import '../../core/widgets/page_header.dart';
import '../../l10n/generated/app_localizations.dart';
import 'shell_destination.dart';

class SectionPage extends StatelessWidget {
  const SectionPage({super.key, required this.destination});

  final ShellDestination destination;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final tokens = AppTokens.of(context);
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(child: Text(destination.label(l10n))),
        Expanded(
          child: Center(
            child: Padding(
              padding: EdgeInsets.all(tokens.spacing),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(destination.icon, size: 64, color: theme.colorScheme.outline),
                  SizedBox(height: tokens.spacing),
                  Text(l10n.emptySectionTitle, style: theme.textTheme.titleMedium),
                  const SizedBox(height: 4),
                  Text(l10n.emptySectionMessage, textAlign: TextAlign.center),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }
}
