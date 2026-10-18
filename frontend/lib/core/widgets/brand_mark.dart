import 'package:flutter/widgets.dart';
import 'package:flutter_svg/flutter_svg.dart';

class BrandMark extends StatelessWidget {
  const BrandMark({super.key, this.size = 32});

  static const _asset = 'assets/logo.svg';

  final double size;

  @override
  Widget build(BuildContext context) {
    return SvgPicture.asset(_asset, width: size, height: size, excludeFromSemantics: true);
  }
}
