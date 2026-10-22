import 'package:flutter/widgets.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../l10n/generated/app_localizations.dart';

enum ShareAccess {
  restricted(LucideIcons.lock),
  anyoneWithLink(LucideIcons.globe);

  const ShareAccess(this.icon);

  final IconData icon;

  static ShareAccess of(String? link) => link == null ? restricted : anyoneWithLink;

  String label(AppLocalizations l10n) => switch (this) {
    restricted => l10n.shareAccessRestricted,
    anyoneWithLink => l10n.shareAccessAnyoneWithLink,
  };

  String description(AppLocalizations l10n) => switch (this) {
    restricted => l10n.shareAccessRestrictedDescription,
    anyoneWithLink => l10n.shareAccessAnyoneWithLinkDescription,
  };
}
