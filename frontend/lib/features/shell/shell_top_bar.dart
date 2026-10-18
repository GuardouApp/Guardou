import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../app/router/routes.dart';
import '../../app/theme/app_tokens.dart';
import '../../core/layout/window_size_class.dart';
import '../../core/widgets/brand_mark.dart';
import '../../l10n/generated/app_localizations.dart';
import '../auth/session_controller.dart';
import '../configuration/configuration_controller.dart';
import 'account_menu.dart';

class ShellTopBar extends ConsumerWidget implements PreferredSizeWidget {
  const ShellTopBar({super.key, this.showMenuButton = false});

  static const _searchMaxWidth = 720.0;
  static const _searchHeight = 44.0;
  static const _brandSpacing = 32.0;

  final bool showMenuButton;

  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final tokens = AppTokens.of(context);
    final applicationName = ref.watch(configurationControllerProvider).value?.applicationName ?? '';
    final isAdmin = ref.watch(sessionControllerProvider).value?.isAdmin ?? false;
    final expanded = WindowSizeClass.of(context) == WindowSizeClass.expanded;
    final brand = _Brand(applicationName: applicationName);
    final searchColor = Theme.of(context).colorScheme.onPrimaryContainer;

    return AppBar(
      backgroundColor: Colors.transparent,
      scrolledUnderElevation: 0,
      automaticallyImplyLeading: false,
      leading: showMenuButton
          ? Builder(
              builder: (context) => IconButton(
                tooltip: l10n.navigationOpen,
                icon: const Icon(LucideIcons.menu),
                onPressed: () => Scaffold.of(context).openDrawer(),
              ),
            )
          : null,
      titleSpacing: showMenuButton || expanded ? 0 : null,
      title: Row(
        children: [
          if (expanded)
            SizedBox(
              width: tokens.navigationWidth,
              child: Padding(
                padding: EdgeInsets.only(left: tokens.spacing),
                child: brand,
              ),
            )
          else
            Flexible(child: brand),
          if (!showMenuButton) ...[
            if (!expanded) const SizedBox(width: _brandSpacing),
            Expanded(
              flex: 4,
              child: Align(
                alignment: AlignmentDirectional.centerStart,
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: _searchMaxWidth, maxHeight: _searchHeight),
                  child: SearchBar(
                    enabled: false,
                    elevation: const WidgetStatePropertyAll(0),
                    leading: Icon(LucideIcons.search, color: searchColor),
                    hintText: l10n.searchHint,
                    hintStyle: WidgetStatePropertyAll(TextStyle(color: searchColor)),
                  ),
                ),
              ),
            ),
          ],
        ],
      ),
      actions: [
        if (showMenuButton) IconButton(tooltip: l10n.search, onPressed: null, icon: const Icon(LucideIcons.search)),
        IconButton(tooltip: l10n.settings, onPressed: null, icon: const Icon(LucideIcons.settings)),
        if (isAdmin)
          IconButton(
            tooltip: l10n.administration,
            onPressed: () => context.go(Routes.adminUsers),
            icon: const Icon(LucideIcons.shieldCheck),
          ),
        const AccountMenu(),
        const SizedBox(width: 8),
      ],
    );
  }
}

class _Brand extends StatelessWidget {
  const _Brand({required this.applicationName});

  final String applicationName;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        const BrandMark(),
        const SizedBox(width: 12),
        Flexible(child: Text(applicationName, overflow: TextOverflow.ellipsis)),
      ],
    );
  }
}
