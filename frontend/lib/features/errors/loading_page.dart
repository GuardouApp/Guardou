import 'package:flutter/material.dart';

import '../../l10n/generated/app_localizations.dart';

class LoadingPage extends StatelessWidget {
  const LoadingPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(child: CircularProgressIndicator(semanticsLabel: AppLocalizations.of(context).loading)),
    );
  }
}
