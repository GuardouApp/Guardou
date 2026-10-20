import 'dart:ui_web' as ui_web;

import 'package:flutter/widgets.dart';
import 'package:web/web.dart' as web;

import 'browser_embeds.dart';

BrowserEmbeds createBrowserEmbeds() => _WebBrowserEmbeds();

class _WebBrowserEmbeds implements BrowserEmbeds {
  static const _viewTypePrefix = 'guardou-embed-';

  final _viewTypes = <String, String>{};

  @override
  Widget video(Uri source) => _view('video:$source', () {
    final video = web.HTMLVideoElement()
      ..src = source.toString()
      ..controls = true
      ..playsInline = true
      ..preload = 'metadata';
    video.style.background = 'black';
    return video;
  });

  @override
  Widget audio(Uri source) => _view(
    'audio:$source',
    () => web.HTMLAudioElement()
      ..src = source.toString()
      ..controls = true
      ..preload = 'metadata',
  );

  @override
  Widget document(Uri source) => _view('document:$source', () => web.HTMLIFrameElement()..src = source.toString());

  Widget _view(String key, web.HTMLElement Function() create) {
    final viewType = _viewTypes.putIfAbsent(key, () {
      final viewType = '$_viewTypePrefix${_viewTypes.length}';
      ui_web.platformViewRegistry.registerViewFactory(viewType, (int _) => _fill(create()));
      return viewType;
    });
    return HtmlElementView(viewType: viewType);
  }

  web.HTMLElement _fill(web.HTMLElement element) {
    element.style
      ..width = '100%'
      ..height = '100%'
      ..border = 'none';
    return element;
  }
}
