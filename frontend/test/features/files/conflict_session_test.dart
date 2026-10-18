import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:guardou/api/generated/models/conflict_policy.dart';
import 'package:guardou/core/api/api_problem.dart';
import 'package:guardou/features/files/conflict_session.dart';

void main() {
  DioException conflict() => DioException(
    requestOptions: RequestOptions(),
    error: const ApiProblem(status: 409),
  );

  Future<String> Function(ConflictPolicy) attemptWith(List<ConflictPolicy> attempts, {Set<ConflictPolicy> conflicts = const {ConflictPolicy.fail}}) =>
      (policy) async {
        attempts.add(policy);
        if (conflicts.contains(policy)) {
          throw conflict();
        }
        return policy.name;
      };

  test('does not ask without a conflict', () async {
    final attempts = <ConflictPolicy>[];
    final session = ConflictSession((_) async => fail('The prompt must not open.'));

    expect(await session.run('a', attemptWith(attempts, conflicts: const {})), 'fail');
    expect(attempts, [ConflictPolicy.fail]);
  });

  test('retries with the chosen policy', () async {
    final attempts = <ConflictPolicy>[];
    final session = ConflictSession((_) async => const ConflictDecision(ConflictPolicy.rename));

    expect(await session.run('a', attemptWith(attempts)), 'rename');
    expect(attempts, [ConflictPolicy.fail, ConflictPolicy.rename]);
  });

  test('skips without a second request', () async {
    final attempts = <ConflictPolicy>[];
    final session = ConflictSession((_) async => const ConflictDecision(ConflictPolicy.skip));

    expect(await session.run('a', attemptWith(attempts)), isNull);
    expect(attempts, [ConflictPolicy.fail]);
  });

  test('applies a decision to all later conflicts', () async {
    var prompts = 0;
    final attempts = <ConflictPolicy>[];
    final session = ConflictSession((_) async {
      prompts++;
      return const ConflictDecision(ConflictPolicy.replace, applyToAll: true);
    });

    await session.run('a', attemptWith(attempts));
    await session.run('b', attemptWith(attempts));

    expect(prompts, 1);
    expect(attempts, [ConflictPolicy.fail, ConflictPolicy.replace, ConflictPolicy.replace]);
  });

  test('remembers skip for all conflicts but still tries each item', () async {
    final attempts = <ConflictPolicy>[];
    final session = ConflictSession((_) async => const ConflictDecision(ConflictPolicy.skip, applyToAll: true));

    await session.run('a', attemptWith(attempts));
    expect(await session.run('b', attemptWith(attempts, conflicts: const {})), 'fail');
    expect(attempts, [ConflictPolicy.fail, ConflictPolicy.fail]);
  });

  test('cancels the remaining items', () async {
    final attempts = <ConflictPolicy>[];
    final session = ConflictSession((_) async => null);

    await session.run('a', attemptWith(attempts));
    await session.run('b', attemptWith(attempts));

    expect(session.cancelled, isTrue);
    expect(attempts, [ConflictPolicy.fail]);
  });

  test('rethrows other errors', () async {
    final session = ConflictSession((_) async => null);

    await expectLater(
      session.run('a', (_) async => throw DioException(requestOptions: RequestOptions(), error: const ApiProblem(status: 500))),
      throwsA(isA<DioException>()),
    );
  });
}
