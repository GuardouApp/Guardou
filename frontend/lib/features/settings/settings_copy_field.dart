import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:lucide_icons_flutter/lucide_icons.dart';

import '../../app/theme/app_tokens.dart';
import '../../core/widgets/labeled_text_field.dart';
import '../../l10n/generated/app_localizations.dart';

class SettingsCopyField extends StatefulWidget {
  const SettingsCopyField({super.key, required this.label, required this.value});

  final String label;
  final String value;

  @override
  State<SettingsCopyField> createState() => _SettingsCopyFieldState();
}

class _SettingsCopyFieldState extends State<SettingsCopyField> {
  static const _copiedDuration = Duration(seconds: 2);

  Timer? _timer;
  bool _copied = false;

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  Future<void> _copy() async {
    await Clipboard.setData(ClipboardData(text: widget.value));
    if (!mounted) {
      return;
    }
    setState(() => _copied = true);
    _timer?.cancel();
    _timer = Timer(_copiedDuration, () {
      if (mounted) {
        setState(() => _copied = false);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final tokens = AppTokens.of(context);
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return LabeledField(
      label: widget.label,
      child: Container(
        padding: const EdgeInsets.all(4),
        decoration: BoxDecoration(color: colors.surfaceContainerHighest, borderRadius: BorderRadius.circular(8)),
        child: Row(
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 12),
              child: Icon(LucideIcons.link, size: 18, color: colors.onSurfaceVariant),
            ),
            Container(width: 1, height: 24, color: colors.outlineVariant),
            Expanded(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 12),
                child: Text(
                  widget.value,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: theme.textTheme.bodyLarge?.copyWith(color: colors.onSurfaceVariant),
                ),
              ),
            ),
            FilledButton.icon(
              onPressed: () => unawaited(_copy()),
              style: FilledButton.styleFrom(
                backgroundColor: _copied ? tokens.success : null,
                foregroundColor: _copied ? tokens.onSuccess : null,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(6)),
              ),
              icon: Icon(_copied ? LucideIcons.check : LucideIcons.copy, size: 16),
              label: Text(_copied ? l10n.copied : l10n.copy),
            ),
          ],
        ),
      ),
    );
  }
}
