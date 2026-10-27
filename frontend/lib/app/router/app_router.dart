import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/admin/admin_shell.dart';
import '../../features/auth/login_page.dart';
import '../../features/auth/register_page.dart';
import '../../features/auth/session_controller.dart';
import '../../features/configuration/configuration_controller.dart';
import '../../features/errors/error_page.dart';
import '../../features/errors/loading_page.dart';
import '../../features/files/file_route_page.dart';
import '../../features/files/folder_page.dart';
import '../../features/home/home_page.dart';
import '../../features/settings/settings_page.dart';
import '../../features/shares/shared_folder_page.dart';
import '../../features/shares/shared_link_gate.dart';
import '../../features/shares/shared_link_page.dart';
import '../../features/shares/shared_viewer_page.dart';
import '../../features/shares/shared_with_me_page.dart';
import '../../features/shell/adaptive_shell.dart';
import '../../features/shell/section_page.dart';
import '../../features/shell/shell_destination.dart';
import '../../features/tasks/tasks_page.dart';
import '../../features/trash/trash_page.dart';
import '../../features/users/users_page.dart';
import '../../features/viewer/viewer_page.dart';
import 'redirect_guard.dart';
import 'routes.dart';

final appRouterProvider = Provider<GoRouter>((ref) {
  final refresh = ValueNotifier(0);
  void notify(Object? _, Object? _) => refresh.value++;
  ref.listen(configurationControllerProvider, notify);
  ref.listen(sessionControllerProvider, notify);

  String? adminOnly(BuildContext _, GoRouterState _) =>
      ref.read(sessionControllerProvider).value?.isAdmin ?? false ? null : Routes.forbidden;

  final router = GoRouter(
    initialLocation: Routes.home,
    refreshListenable: refresh,
    redirect: (context, state) => resolveRedirect(
      configuration: ref.read(configurationControllerProvider),
      session: ref.read(sessionControllerProvider),
      registrationOpen: ref.read(configurationControllerProvider).value?.registrationOpen ?? false,
      location: state.uri,
    ),
    errorBuilder: (context, state) => const ErrorPage(kind: ErrorKind.notFound),
    routes: [
      GoRoute(path: Routes.root, redirect: (_, _) => Routes.home),
      GoRoute(path: Routes.loading, builder: (_, _) => const LoadingPage()),
      GoRoute(
        path: Routes.login,
        builder: (_, state) => LoginPage(
          redirect: state.uri.queryParameters[Routes.redirectParameter],
          oidcError: state.uri.queryParameters[Routes.oidcErrorParameter],
        ),
      ),
      GoRoute(path: Routes.register, builder: (_, _) => const RegisterPage()),
      GoRoute(
        path: Routes.forbidden,
        builder: (_, _) => const ErrorPage(kind: ErrorKind.forbidden),
      ),
      GoRoute(
        path: Routes.serverError,
        builder: (_, _) => const ErrorPage(kind: ErrorKind.serverError),
      ),
      GoRoute(
        path: '${Routes.view}/:${Routes.idParameter}',
        builder: (_, state) {
          final id = state.pathParameters[Routes.idParameter] ?? '';
          return ViewerPage(key: ValueKey(id), fileId: id);
        },
      ),
      GoRoute(
        path: '${Routes.sharedLink}/:${Routes.tokenParameter}',
        builder: (_, state) {
          final token = state.pathParameters[Routes.tokenParameter] ?? '';
          return SharedLinkGate(
            key: ValueKey(token),
            token: token,
            locationOf: sharedItemLocation,
            child: SharedLinkPage(token: token),
          );
        },
        routes: [
          GoRoute(
            path: '${Routes.folders.substring(1)}/:${Routes.idParameter}',
            builder: (_, state) {
              final token = state.pathParameters[Routes.tokenParameter] ?? '';
              final id = state.pathParameters[Routes.idParameter] ?? '';
              return SharedLinkGate(
                key: ValueKey('$token/$id'),
                token: token,
                locationOf: (_) => Routes.folder(id),
                child: SharedFolderPage(token: token, folderId: id),
              );
            },
          ),
          GoRoute(
            path: '${Routes.view.substring(1)}/:${Routes.idParameter}',
            builder: (_, state) {
              final token = state.pathParameters[Routes.tokenParameter] ?? '';
              final id = state.pathParameters[Routes.idParameter] ?? '';
              return SharedLinkGate(
                key: ValueKey('$token/$id'),
                token: token,
                locationOf: (_) => Routes.viewer(id),
                child: SharedViewerPage(token: token, fileId: id),
              );
            },
          ),
        ],
      ),
      ShellRoute(
        builder: (context, state, child) => AdaptiveShell(location: state.uri.path, child: child),
        routes: [
          for (final destination in ShellDestination.values.where(
            (destination) => !_dedicatedPages.contains(destination),
          ))
            GoRoute(
              path: destination.path,
              pageBuilder: (_, _) => NoTransitionPage(child: SectionPage(destination: destination)),
            ),
          GoRoute(path: Routes.files, pageBuilder: (_, _) => _folderPage(Routes.rootFolderId)),
          GoRoute(
            path: '${Routes.folders}/:${Routes.idParameter}',
            pageBuilder: (_, state) => _folderPage(state.pathParameters[Routes.idParameter] ?? Routes.rootFolderId),
          ),
          GoRoute(
            path: '${Routes.file}/:${Routes.idParameter}',
            pageBuilder: (_, state) {
              final id = state.pathParameters[Routes.idParameter] ?? '';
              return NoTransitionPage(
                child: FileRoutePage(key: ValueKey(id), fileId: id),
              );
            },
          ),
          GoRoute(
            path: Routes.home,
            pageBuilder: (_, _) => const NoTransitionPage(child: HomePage()),
          ),
          GoRoute(
            path: Routes.shared,
            pageBuilder: (_, _) => const NoTransitionPage(child: SharedWithMePage()),
          ),
          GoRoute(
            path: Routes.trash,
            pageBuilder: (_, _) => const NoTransitionPage(child: TrashPage()),
          ),
        ],
      ),
      GoRoute(path: Routes.admin, redirect: (_, _) => Routes.adminUsers),
      ShellRoute(
        builder: (context, state, child) => AdminShell(location: state.uri.path, child: child),
        routes: [
          GoRoute(
            path: Routes.adminUsers,
            redirect: adminOnly,
            pageBuilder: (_, _) => const NoTransitionPage(child: UsersPage()),
          ),
          GoRoute(
            path: Routes.adminTasks,
            redirect: adminOnly,
            pageBuilder: (_, _) => const NoTransitionPage(child: TasksPage()),
          ),
          GoRoute(
            path: Routes.adminSettings,
            redirect: adminOnly,
            pageBuilder: (_, _) => const NoTransitionPage(child: SettingsPage()),
          ),
        ],
      ),
    ],
  );

  ref.onDispose(() {
    router.dispose();
    refresh.dispose();
  });
  return router;
});

const _dedicatedPages = {ShellDestination.home, ShellDestination.files, ShellDestination.shared, ShellDestination.trash};

Page<void> _folderPage(String folderId) => NoTransitionPage(
  child: FolderPage(key: ValueKey(folderId), folderId: folderId),
);
