import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../app/theme/app_tokens.dart';

class LabeledField extends StatelessWidget {
  const LabeledField({super.key, required this.label, this.description, this.required = false, required this.child});

  final String label;
  final String? description;
  final bool required;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final text = description;

    return Padding(
      padding: EdgeInsets.only(top: AppTokens.of(context).spacing * 1.25),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text.rich(
            TextSpan(
              text: label,
              children: [
                if (required)
                  TextSpan(
                    text: ' *',
                    style: TextStyle(color: colors.error),
                  ),
              ],
            ),
            style: theme.textTheme.titleSmall?.copyWith(color: colors.primary),
          ),
          if (text != null) ...[const SizedBox(height: 4), Text(text, style: theme.textTheme.bodyMedium)],
          const SizedBox(height: 8),
          child,
        ],
      ),
    );
  }
}

class LabeledTextField extends StatelessWidget {
  const LabeledTextField({
    super.key,
    required this.label,
    this.description,
    this.required = false,
    required this.controller,
    this.focusNode,
    this.autofocus = false,
    this.validator,
    this.autovalidateMode,
    this.onChanged,
    this.onSubmitted,
    this.keyboardType,
    this.textInputAction,
    this.autofillHints,
    this.obscureText = false,
    this.inputFormatters,
    this.prefixIcon,
    this.helperText,
    this.errorText,
  });

  final String label;
  final String? description;
  final bool required;
  final TextEditingController controller;
  final FocusNode? focusNode;
  final bool autofocus;
  final FormFieldValidator<String>? validator;
  final AutovalidateMode? autovalidateMode;
  final ValueChanged<String>? onChanged;
  final ValueChanged<String>? onSubmitted;
  final TextInputType? keyboardType;
  final TextInputAction? textInputAction;
  final Iterable<String>? autofillHints;
  final bool obscureText;
  final List<TextInputFormatter>? inputFormatters;
  final Widget? prefixIcon;
  final String? helperText;
  final String? errorText;

  @override
  Widget build(BuildContext context) => LabeledField(
    label: label,
    description: description,
    required: required,
    child: TextFormField(
      controller: controller,
      focusNode: focusNode,
      autofocus: autofocus,
      keyboardType: keyboardType,
      textInputAction: textInputAction,
      obscureText: obscureText,
      inputFormatters: inputFormatters,
      autofillHints: autofillHints ?? (obscureText ? const [] : null),
      autovalidateMode: autovalidateMode,
      decoration: filledInputDecoration(
        context,
      ).copyWith(prefixIcon: prefixIcon, helperText: helperText, errorText: errorText),
      onChanged: onChanged,
      onFieldSubmitted: onSubmitted,
      validator: validator,
    ),
  );
}

InputDecoration filledInputDecoration(BuildContext context) {
  final colors = Theme.of(context).colorScheme;
  final shape = OutlineInputBorder(borderRadius: BorderRadius.circular(8), borderSide: BorderSide.none);
  return InputDecoration(
    filled: true,
    fillColor: colors.surfaceContainerHighest,
    border: shape,
    enabledBorder: shape,
    focusedBorder: shape.copyWith(borderSide: BorderSide(color: colors.primary, width: 2)),
    errorBorder: shape.copyWith(borderSide: BorderSide(color: colors.error)),
    focusedErrorBorder: shape.copyWith(borderSide: BorderSide(color: colors.error, width: 2)),
    errorMaxLines: 5,
  );
}
