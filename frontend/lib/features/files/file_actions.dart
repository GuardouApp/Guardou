import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../api/generated/models/item_entry.dart';
import '../../api/generated/models/item_kind.dart';
import '../../app/router/routes.dart';
import '../../core/browser/browser_files.dart';
import '../../core/layout/window_size_class.dart';
import '../../core/status/operation_status.dart';
import '../../l10n/generated/app_localizations.dart';
import '../shares/share_dialog.dart';
import '../trash/trash_controller.dart';
import '../trash/trash_service.dart';
import 'conflict_session.dart';
import 'details_panel.dart';
import 'details_panel_controller.dart';
import 'dialogs/confirm_dialog.dart';
import 'dialogs/conflict_dialog.dart';
import 'dialogs/destination_picker.dart';
import 'dialogs/name_dialog.dart';
import 'files_service.dart';
import 'folder_controller.dart';
import 'item_action.dart';
import 'storage_usage_controller.dart';

class FileActions {
  FileActions(this._context)
    : _container = ProviderScope.containerOf(_context, listen: false),
      _messenger = ScaffoldMessenger.of(_context),
      _l10n = AppLocalizations.of(_context);

  final BuildContext _context;
  final ProviderContainer _container;
  final ScaffoldMessengerState _messenger;
  final AppLocalizations _l10n;

  FilesService get _files => _container.read(filesServiceProvider);

  TrashService get _trash => _container.read(trashServiceProvider);

  BrowserFiles get _browser => _container.read(browserFilesProvider);

  OperationStatusController get _status => _container.read(operationStatusProvider.notifier);

  Future<void> perform(ItemAction action, List<ItemEntry> entries) => switch (action) {
    ItemAction.open => open(entries.single),
    ItemAction.download => download(entries),
    ItemAction.share => showShareDialog(_context, entries.single),
    ItemAction.rename => rename(entries.single),
    ItemAction.move => transfer(entries, copy: false),
    ItemAction.copy => transfer(entries, copy: true),
    ItemAction.details => details(entries.single),
    ItemAction.trash => trash(entries),
  };

  Future<void> open(ItemEntry entry) async {
    if (entry.kind == ItemKind.folder) {
      _context.go(Routes.folder(entry.name.isEmpty ? Routes.rootFolderId : entry.id));
    } else {
      _context.go(Routes.viewer(entry.id));
    }
  }

  Future<void> download(List<ItemEntry> entries) async => _browser.download(
    entries.length == 1
        ? _files.downloadUri(entries.single.id)
        : _files.archiveUri([for (final entry in entries) entry.id]),
  );

  Future<void> details(ItemEntry entry) async {
    if (WindowSizeClass.of(_context) == WindowSizeClass.compact) {
      await Navigator.of(
        _context,
      ).push(MaterialPageRoute<void>(fullscreenDialog: true, builder: (_) => DetailsPage(entry: entry)));
    } else {
      _container.read(detailsPanelProvider.notifier).show();
    }
  }

  Future<void> createFolder(String parentId) async {
    final name = await showNameDialog(
      _context,
      title: _l10n.newFolder,
      confirmLabel: _l10n.create,
      initialName: _l10n.untitledFolder,
    );
    if (name != null) {
      await _guard(() => _files.createFolder(parentId, name), done: _l10n.folderCreated(name));
    }
  }

  Future<void> uploadFiles(String folderId) async => upload(folderId, await _browser.pickFiles());

  Future<void> uploadFolder(String folderId) async => upload(folderId, await _browser.pickFolder());

  Future<void> upload(String folderId, List<LocalFile> files) async {
    if (files.isEmpty) {
      return;
    }
    final session = ConflictSession(_askConflict);
    final total = files.fold(0, (sum, file) => sum + file.size);
    var uploaded = 0;
    final succeeded = await _guard(
      () => _status.track(_l10n.uploadingFiles(files.length), (report) async {
        var sent = 0;
        for (final file in files.takeWhile((_) => !session.cancelled)) {
          final entry = await session.run(
            file.name,
            (policy) => _files.upload(
              folderId,
              file,
              conflict: policy,
              onProgress: (count, _) => report(total == 0 ? 1 : (sent + count) / total),
            ),
          );
          sent += file.size;
          uploaded += entry == null ? 0 : 1;
        }
      }),
    );
    if (succeeded) {
      _show(_l10n.filesUploaded(uploaded));
    }
  }

  Future<void> rename(ItemEntry entry) async {
    final name = await showNameDialog(
      _context,
      title: _l10n.actionRename,
      confirmLabel: _l10n.actionRename,
      initialName: entry.name,
    );
    if (name != null && name != entry.name) {
      await _guard(() => _files.rename(entry.id, name), done: _l10n.itemRenamed(name));
    }
  }

  Future<void> transfer(List<ItemEntry> entries, {required bool copy}) async {
    final destination = await showDestinationPicker(
      _context,
      title: copy ? _l10n.copyItems(entries.length) : _l10n.moveItems(entries.length),
      confirmLabel: copy ? _l10n.copyHere : _l10n.moveHere,
      startFolderId: entries.first.parentId ?? Routes.rootFolderId,
      excludedIds: {for (final entry in entries) entry.id},
    );
    if (destination == null) {
      return;
    }
    final session = ConflictSession(_askConflict);
    var transferred = 0;
    final succeeded = await _guard(
      () => _status.track(copy ? _l10n.copyingItems(entries.length) : _l10n.movingItems(entries.length), (_) async {
        for (final entry in entries.takeWhile((_) => !session.cancelled)) {
          final result = await session.run(
            entry.name,
            (policy) => copy ? _files.copy(entry.id, destination, policy) : _files.move(entry.id, destination, policy),
          );
          transferred += result == null ? 0 : 1;
        }
      }),
    );
    if (succeeded) {
      _show(copy ? _l10n.itemsCopied(transferred) : _l10n.itemsMoved(transferred));
    }
  }

  Future<void> trash(List<ItemEntry> entries) async {
    final ids = [for (final entry in entries) entry.id];
    final trashed = await _guard(() => _files.trash(ids));
    if (trashed) {
      _show(
        _l10n.itemsTrashed(ids.length),
        action: SnackBarAction(label: _l10n.undo, onPressed: () => restore(ids)),
      );
    }
  }

  Future<void> restore(List<String> ids) => _guard(() => _trash.restore(ids), done: _l10n.itemsRestored(ids.length));

  Future<void> deleteForever(List<String> ids) async {
    final confirmed = await showConfirmDialog(
      _context,
      title: _l10n.deleteForeverTitle(ids.length),
      message: _l10n.deleteForeverMessage,
      confirmLabel: _l10n.deleteForever,
    );
    if (confirmed) {
      await _guard(() => _trash.delete(ids), done: _l10n.itemsDeleted(ids.length));
    }
  }

  Future<void> emptyTrash() async {
    final confirmed = await showConfirmDialog(
      _context,
      title: _l10n.emptyTrashTitle,
      message: _l10n.deleteForeverMessage,
      confirmLabel: _l10n.emptyTrash,
    );
    if (confirmed) {
      await _guard(_trash.empty, done: _l10n.trashEmptied);
    }
  }

  Future<ConflictDecision?> _askConflict(String name) async =>
      _context.mounted ? showConflictDialog(_context, name) : null;

  Future<bool> _guard(Future<void> Function() operation, {String? done}) async {
    try {
      await operation();
      if (done != null) {
        _show(done);
      }
      return true;
    } on DioException catch (error) {
      _show(ConflictSession.isConflict(error) ? _l10n.nameTaken : _l10n.genericError);
      return false;
    } finally {
      _refresh();
    }
  }

  void _refresh() {
    _container
      ..invalidate(folderControllerProvider)
      ..invalidate(childrenControllerProvider)
      ..invalidate(storageUsageControllerProvider)
      ..invalidate(trashControllerProvider);
  }

  void _show(String message, {SnackBarAction? action}) => _messenger
    ..hideCurrentSnackBar()
    ..showSnackBar(SnackBar(content: Text(message), action: action));
}
