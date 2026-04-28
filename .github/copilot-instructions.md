# Copilot Instructions

When editing files in `RappSack`, follow `CONTRIBUTING.md` as the primary source of truth.

## Core Rules
- Keep code compatible with C# 7.3.
- Prefer clarity over cleverness.
- Prefer consistency over personal style.
- Reduce duplication; extract shared helpers/extensions when logic repeats or obscures intent.
- Use existing .NET / Microsoft.CrmSdk / Microsoft.PowerPlatform APIs before introducing custom abstractions.
- Improve shared helper libraries when it benefits multiple callers.

## RappSack-Specific Guidance
- Keep shared base classes such as `RappSackPlugin` simple.
- Avoid overly complex framework-style extension points.
- Prefer small, explicit APIs over flexible but difficult-to-understand infrastructure.
- Avoid adding solution-specific behavior to shared RappSack components.

## Control Flow and Formatting
- Always use braces for `if`, `else`, `for`, `foreach`, `while`, `do`, `using`, `lock`, and `switch` case blocks.
- Always use braces for `try` / `catch`.
- Do not compress multi-statement logic onto one line.
- Prefer early returns to reduce nesting.
- Keep one public class per file unless small related types clearly belong together.

## Exception Handling
- Scope `try` blocks narrowly.
- Prefer specific exceptions over broad `Exception` where practical.
- Do not silently swallow exceptions unless the failure is truly non-critical and a comment explains why.
- Avoid logic-heavy one-line `try/catch`.

## Naming and Organization
- Use PascalCase verb names for methods.
- Use clear boolean names such as `IsLoaded` and `HasChanges`.
- Use `_camelCase` for private fields.
- Name extension classes `<Domain>Extensions` or another clear domain-specific helper name.
- Keep related private helpers grouped near the public API they support.

## Logging
- Use existing logging helpers such as `LogUse`, `LogError`, `LogInfo`, and `LogWarn` when available.
- Log unexpected failures with `LogError`.
- Log recoverable fallbacks with `LogWarn`.
- Avoid noisy per-item logging in large loops unless diagnosing an issue.

## Preferred Patterns
- Validate inputs at public boundaries.
- Prefer helper/extension methods when the same logic appears 3+ times.
- Prefer simple ternaries only when both branches are easy to read and free of side effects.
- Use `InvokeRequired` guards for UI marshaling.

## Avoid
- One-line `try/catch` with non-trivial logic.
- Silent exception swallowing without comment/logging.
- Brittle `.Parent.Parent` chains; prefer ancestor-search helpers.
- Deeply nested ternaries.
- Mixing unrelated concerns in a single method.