import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../../core/widgets/labeled_text_field.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../item_name_rule.dart';

Future<String?> showNameDialog(
  BuildContext context, {
  required String title,
  required String confirmLabel,
  required String initialName,
}) => showDialog<String>(
  context: context,
  builder: (_) => _NameDialog(title: title, confirmLabel: confirmLabel, initialName: initialName),
);

class _NameDialog extends StatefulWidget {
  const _NameDialog({required this.title, required this.confirmLabel, required this.initialName});

  final String title;
  final String confirmLabel;
  final String initialName;

  @override
  State<_NameDialog> createState() => _NameDialogState();
}

class _NameDialogState extends State<_NameDialog> {
  late final _controller = TextEditingController(text: widget.initialName)
    ..selection = TextSelection(baseOffset: 0, extentOffset: ItemNameRule.baseNameLength(widget.initialName));
  bool _invalid = false;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _submit() {
    final name = _controller.text;
    if (!ItemNameRule.isValid(name)) {
      setState(() => _invalid = true);
      return;
    }
    Navigator.of(context).pop(name);
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return AlertDialog(
      title: Text(widget.title),
      content: LabeledTextField(
        label: l10n.itemName,
        controller: _controller,
        autofocus: true,
        inputFormatters: [LengthLimitingTextInputFormatter(ItemNameRule.maximumLength)],
        errorText: _invalid ? l10n.itemNameInvalid : null,
        onChanged: (_) => setState(() => _invalid = false),
        onSubmitted: (_) => _submit(),
      ),
      actions: [
        TextButton(onPressed: () => Navigator.of(context).pop(), child: Text(l10n.cancel)),
        FilledButton(onPressed: _submit, child: Text(widget.confirmLabel)),
      ],
    );
  }
}
