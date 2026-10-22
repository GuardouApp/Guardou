import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../api/generated/models/item_entry.dart';
import '../../app/router/routes.dart';
import '../../app/theme/app_theme.dart';
import '../../l10n/generated/app_localizations.dart';
import '../files/files_service.dart';
import '../files/folder_controller.dart';
import '../files/folder_states.dart';
import 'media_siblings.dart';
import 'viewer_frame.dart';

class ViewerPage extends ConsumerWidget {
  const ViewerPage({super.key, required this.fileId});

  final String fileId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);

    return Theme(
      data: AppTheme.dark(),
      child: switch (ref.watch(fileDetailsProvider(fileId))) {
        AsyncValue(hasValue: true, :final value?) => _Viewer(entry: value.entry),
        AsyncValue(:final error?) => Scaffold(
          appBar: AppBar(leading: const _CloseButton(location: Routes.files)),
          body: FolderErrorView(
            isRoot: false,
            error: error,
            onRetry: () => ref.invalidate(fileDetailsProvider(fileId)),
          ),
        ),
        _ => Scaffold(
          body: Center(child: CircularProgressIndicator(semanticsLabel: l10n.loading)),
        ),
      },
    );
  }
}

class _Viewer extends ConsumerWidget {
  const _Viewer({required this.entry});

  final ItemEntry entry;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final siblings = ref.watch(mediaSiblingsProvider(entry.parentId ?? Routes.rootFolderId));
    final index = siblings.indexWhere((sibling) => sibling.id == entry.id);
    final previous = index > 0 ? siblings[index - 1] : null;
    final next = index >= 0 && index < siblings.length - 1 ? siblings[index + 1] : null;

    void show(ItemEntry? target) {
      if (target != null) {
        context.go(Routes.viewer(target.id));
      }
    }

    return CallbackShortcuts(
      bindings: {
        const SingleActivator(LogicalKeyboardKey.arrowLeft): () => show(previous),
        const SingleActivator(LogicalKeyboardKey.arrowRight): () => show(next),
        const SingleActivator(LogicalKeyboardKey.escape): () => context.go(Routes.fileItem(entry.id)),
      },
      child: Focus(
        autofocus: true,
        child: ViewerFrame(
          entry: entry,
          source: ref.watch(filesServiceProvider),
          leading: _CloseButton(location: Routes.fileItem(entry.id)),
          overlays: [
            if (previous != null)
              Align(
                alignment: Alignment.centerLeft,
                child: _StepButton(
                  icon: LucideIcons.chevronLeft,
                  tooltip: l10n.viewerPrevious,
                  onPressed: () => show(previous),
                ),
              ),
            if (next != null)
              Align(
                alignment: Alignment.centerRight,
                child: _StepButton(
                  icon: LucideIcons.chevronRight,
                  tooltip: l10n.viewerNext,
                  onPressed: () => show(next),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

class _CloseButton extends StatelessWidget {
  const _CloseButton({required this.location});

  final String location;

  @override
  Widget build(BuildContext context) => IconButton(
    tooltip: AppLocalizations.of(context).close,
    onPressed: () => context.go(location),
    icon: const Icon(LucideIcons.x),
  );
}

class _StepButton extends StatelessWidget {
  const _StepButton({required this.icon, required this.tooltip, required this.onPressed});

  static const _margin = 8.0;

  final IconData icon;
  final String tooltip;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.all(_margin),
    child: IconButton.filledTonal(tooltip: tooltip, onPressed: onPressed, icon: Icon(icon)),
  );
}
