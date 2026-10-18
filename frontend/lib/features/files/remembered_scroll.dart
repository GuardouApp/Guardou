import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class ScrollOffsets {
  final _offsets = <String, double>{};

  double of(String key) => _offsets[key] ?? 0;

  void save(String key, double offset) => _offsets[key] = offset;
}

final scrollOffsetsProvider = Provider<ScrollOffsets>((ref) => ScrollOffsets());

class RememberedScroll extends ConsumerStatefulWidget {
  const RememberedScroll({super.key, required this.storageKey, required this.builder});

  final String storageKey;
  final Widget Function(BuildContext context, ScrollController controller) builder;

  @override
  ConsumerState<RememberedScroll> createState() => _RememberedScrollState();
}

class _RememberedScrollState extends ConsumerState<RememberedScroll> {
  late final ScrollOffsets _offsets = ref.read(scrollOffsetsProvider);
  late ScrollController _controller = _createController();

  ScrollController _createController() =>
      ScrollController(initialScrollOffset: _offsets.of(widget.storageKey))..addListener(_save);

  void _save() {
    if (_controller.hasClients) {
      _offsets.save(widget.storageKey, _controller.offset);
    }
  }

  @override
  void didUpdateWidget(RememberedScroll oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.storageKey != widget.storageKey) {
      _controller.dispose();
      _controller = _createController();
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => widget.builder(context, _controller);
}
