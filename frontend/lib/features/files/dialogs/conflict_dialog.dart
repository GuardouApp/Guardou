import 'package:flutter/material.dart';

import '../../../api/generated/models/conflict_policy.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../conflict_session.dart';

Future<ConflictDecision?> showConflictDialog(BuildContext context, String name) => showDialog<ConflictDecision>(
  context: context,
  builder: (_) => _ConflictDialog(name: name),
);

class _ConflictDialog extends StatefulWidget {
  const _ConflictDialog({required this.name});

  final String name;

  @override
  State<_ConflictDialog> createState() => _ConflictDialogState();
}

class _ConflictDialogState extends State<_ConflictDialog> {
  static const _choices = [ConflictPolicy.replace, ConflictPolicy.rename, ConflictPolicy.skip];

  ConflictPolicy _policy = ConflictPolicy.rename;
  bool _applyToAll = false;

  String _label(ConflictPolicy policy, AppLocalizations l10n) => switch (policy) {
    ConflictPolicy.replace => l10n.conflictReplace,
    ConflictPolicy.skip => l10n.conflictSkip,
    _ => l10n.conflictRename,
  };

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return AlertDialog(
      title: Text(l10n.conflictTitle),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(l10n.conflictMessage(widget.name)),
          const SizedBox(height: 8),
          RadioGroup<ConflictPolicy>(
            groupValue: _policy,
            onChanged: (policy) => setState(() => _policy = policy ?? _policy),
            child: Column(
              children: [
                for (final policy in _choices)
                  RadioListTile<ConflictPolicy>(
                    value: policy,
                    contentPadding: EdgeInsets.zero,
                    title: Text(_label(policy, l10n)),
                  ),
              ],
            ),
          ),
          CheckboxListTile(
            value: _applyToAll,
            contentPadding: EdgeInsets.zero,
            controlAffinity: ListTileControlAffinity.leading,
            onChanged: (value) => setState(() => _applyToAll = value ?? false),
            title: Text(l10n.conflictApplyToAll),
          ),
        ],
      ),
      actions: [
        TextButton(onPressed: () => Navigator.of(context).pop(), child: Text(l10n.cancel)),
        FilledButton(
          onPressed: () => Navigator.of(context).pop(ConflictDecision(_policy, applyToAll: _applyToAll)),
          child: Text(l10n.conflictContinue),
        ),
      ],
    );
  }
}
