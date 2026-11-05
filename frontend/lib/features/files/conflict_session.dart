import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

import '../../api/generated/models/conflict_policy.dart';
import '../../core/api/api_problem.dart';

@immutable
class ConflictDecision {
  const ConflictDecision(this.policy, {this.applyToAll = false});

  final ConflictPolicy policy;
  final bool applyToAll;
}

typedef ConflictPrompt = Future<ConflictDecision?> Function(String name);

class ConflictSession {
  ConflictSession(this._prompt, {ConflictPolicy? remembered}) : _remembered = remembered;

  static const uploadInProgressCode = 'uploadInProgress';
  static const _resolvingPolicies = {ConflictPolicy.replace, ConflictPolicy.rename};

  final ConflictPrompt _prompt;
  ConflictPolicy? _remembered;
  bool _cancelled = false;
  Future<ConflictDecision?> _prompting = Future.value();

  bool get cancelled => _cancelled;

  Future<T?> run<T>(String name, Future<T> Function(ConflictPolicy policy) attempt) async {
    if (_cancelled) {
      return null;
    }
    final remembered = _remembered;
    try {
      final initial = remembered != null && _resolvingPolicies.contains(remembered) ? remembered : ConflictPolicy.fail;
      return await attempt(initial);
    } on DioException catch (error) {
      if (!isConflict(error)) {
        rethrow;
      }
    }
    final decision = await _decide(name);
    if (decision == null) {
      _cancelled = true;
      return null;
    }
    if (decision.applyToAll) {
      _remembered = decision.policy;
    }
    return decision.policy == ConflictPolicy.skip ? null : attempt(decision.policy);
  }

  Future<ConflictDecision?> _decide(String name) => _prompting = _prompting.then<ConflictDecision?>((_) {
    final remembered = _remembered;
    if (_cancelled) {
      return null;
    }
    return remembered == null ? _prompt(name) : ConflictDecision(remembered);
  });

  static bool isConflict(Object error) => switch (error) {
    DioException(error: ApiProblem(status: 409, :final codes)) => !codes.contains(uploadInProgressCode),
    _ => false,
  };
}
