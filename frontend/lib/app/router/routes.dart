abstract final class Routes {
  static const root = '/';
  static const loading = '/loading';
  static const login = '/login';
  static const register = '/register';
  static const home = '/home';
  static const files = '/files';
  static const folders = '/folders';
  static const file = '/file';
  static const view = '/view';
  static const starred = '/starred';
  static const shared = '/shared';
  static const trash = '/trash';
  static const admin = '/admin';
  static const adminUsers = '/admin/users';
  static const adminTasks = '/admin/tasks';
  static const adminSettings = '/admin/settings';
  static const forbidden = '/403';
  static const serverError = '/500';

  static const redirectParameter = 'redirect';
  static const oidcErrorParameter = 'oidcError';
  static const idParameter = 'id';
  static const rootFolderId = 'root';

  static const entryPages = {loading, login, register};
  static const errorPages = {forbidden, serverError};

  static String folder(String id) => id == rootFolderId ? files : '$folders/${Uri.encodeComponent(id)}';

  static String fileItem(String id) => '$file/${Uri.encodeComponent(id)}';

  static String viewer(String id) => '$view/${Uri.encodeComponent(id)}';

  static String? folderIdOf(String location) {
    final segments = Uri.parse(location).pathSegments.where((segment) => segment.isNotEmpty).toList();
    return switch (segments) {
      [final first] when '/$first' == files => rootFolderId,
      [final first, final id] when '/$first' == folders => id,
      _ => null,
    };
  }
}
