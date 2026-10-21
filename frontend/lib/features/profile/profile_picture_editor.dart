import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/current_user.dart';
import '../../core/browser/browser_files.dart';
import '../../core/widgets/user_avatar.dart';
import '../../l10n/generated/app_localizations.dart';
import 'profile_picture_controller.dart';
import 'profile_picture_failure.dart';

class ProfilePictureEditor extends ConsumerWidget {
  const ProfilePictureEditor({super.key, required this.user, required this.radius});

  final CurrentUser user;
  final double radius;

  Future<void> _replace(BuildContext context, WidgetRef ref) async {
    final messenger = ScaffoldMessenger.of(context);
    final l10n = AppLocalizations.of(context);
    final file = await ref.read(browserFilesProvider).pickImage();
    if (file == null) {
      return;
    }
    await _apply(
      messenger,
      l10n,
      () => ref.read(profilePictureControllerProvider.notifier).replace(user.id, file),
      l10n.profilePictureChanged,
    );
  }

  Future<void> _remove(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    return _apply(
      ScaffoldMessenger.of(context),
      l10n,
      () => ref.read(profilePictureControllerProvider.notifier).remove(user.id),
      l10n.profilePictureRemoved,
    );
  }

  Future<void> _apply(
    ScaffoldMessengerState messenger,
    AppLocalizations l10n,
    Future<void> Function() change,
    String success,
  ) async {
    try {
      await change();
      _show(messenger, success);
    } on ProfilePictureFailure catch (failure) {
      _show(messenger, failure.message(l10n));
    }
  }

  void _show(ScaffoldMessengerState messenger, String message) => messenger
    ..hideCurrentSnackBar()
    ..showSnackBar(SnackBar(content: Text(message)));

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final busy = ref.watch(profilePictureControllerProvider);

    return Stack(
      clipBehavior: Clip.none,
      children: [
        UserAvatar(userId: user.id, name: user.displayName, hasPicture: user.hasPicture, radius: radius),
        if (busy) const Positioned.fill(child: CircularProgressIndicator()),
        PositionedDirectional(
          end: 0,
          bottom: 0,
          child: MenuAnchor(
            menuChildren: [
              MenuItemButton(
                leadingIcon: const Icon(LucideIcons.upload, size: 18),
                onPressed: () => unawaited(_replace(context, ref)),
                child: Text(l10n.profilePictureUpload),
              ),
              MenuItemButton(
                leadingIcon: const Icon(LucideIcons.trash2, size: 18),
                onPressed: () => unawaited(_remove(context, ref)),
                child: Text(l10n.profilePictureRemove),
              ),
            ],
            builder: (context, menu, _) => _EditBadge(
              tooltip: l10n.profilePictureEdit,
              onPressed: switch ((busy, user.hasPicture)) {
                (true, _) => null,
                (false, true) => () => menu.isOpen ? menu.close() : menu.open(),
                (false, false) => () => unawaited(_replace(context, ref)),
              },
            ),
          ),
        ),
      ],
    );
  }
}

class _EditBadge extends StatelessWidget {
  const _EditBadge({required this.tooltip, required this.onPressed});

  static const _size = 28.0;
  static const _iconSize = 14.0;
  static const _borderWidth = 2.0;

  final String tooltip;
  final VoidCallback? onPressed;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    return Tooltip(
      message: tooltip,
      child: DecoratedBox(
        decoration: BoxDecoration(color: colorScheme.surfaceContainerHighest, shape: BoxShape.circle),
        child: Padding(
          padding: const EdgeInsets.all(_borderWidth),
          child: Material(
            color: colorScheme.primary,
            shape: const CircleBorder(),
            clipBehavior: Clip.antiAlias,
            child: InkWell(
              onTap: onPressed,
              child: SizedBox.square(
                dimension: _size - _borderWidth * 2,
                child: Icon(LucideIcons.pencil, size: _iconSize, color: colorScheme.onPrimary),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
