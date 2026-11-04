import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/models/user_preferences.dart';
import '../settings/settings_section.dart';
import 'preferences_controller.dart';

class PreferencesSection extends ConsumerWidget {
  const PreferencesSection({
    super.key,
    required this.icon,
    required this.title,
    required this.description,
    required this.builder,
  });

  final IconData icon;
  final String title;
  final String description;
  final Widget Function(UserPreferences preferences) builder;

  @override
  Widget build(BuildContext context, WidgetRef ref) => SettingsSection(
    icon: icon,
    title: title,
    description: description,
    value: ref.watch(preferencesControllerProvider),
    onRetry: () => ref.invalidate(preferencesControllerProvider),
    builder: (UserPreferences? preferences) => preferences == null ? const SizedBox.shrink() : builder(preferences),
  );
}

mixin PreferencesForm<W extends ConsumerStatefulWidget> on SettingsForm<W, UserPreferences> {
  UserPreferences changed(UserPreferences current);

  @override
  Future<UserPreferences> defaults() => ref.read(preferencesControllerProvider.notifier).defaults();

  @override
  Future<void> persist() => ref.read(preferencesControllerProvider.notifier).save(changed(saved));
}
