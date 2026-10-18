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
  ConflictSession(this._prompt);

  static const _resolvingPolicies = {ConflictPolicy.replace, ConflictPolicy.rename};

  final ConflictPrompt _prompt;
  ConflictPolicy? _remembered;
  bool _cancelled = false;

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
    final decision = remembered == null ? await _prompt(name) : ConflictDecision(remembered);
    if (decision == null) {
      _cancelled = true;
      return null;
    }
    if (decision.applyToAll) {
      _remembered = decision.policy;
    }
    return decision.policy == ConflictPolicy.skip ? null : attempt(decision.policy);
  }

  static bool isConflict(Object error) => switch (error) {
    DioException(error: ApiProblem(status: 409)) => true,
    _ => false,
  };
}
