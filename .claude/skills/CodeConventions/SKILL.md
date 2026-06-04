---
name: CodeConventions
description: C# code conventions and coding style guidelines for the project. Use whenever you need to write or review code to ensure consistency and maintainability across the codebase.
allowed-tools: Read, Grep, Glob
---

Project is developed in C#. Code conventions and code style is provided by .editorconfig.

Indentation: Tabs only. Never use spaces.
Line endings: LF only. Never use CRLF.
File encoding: UTF-8 with BOM. Javascript files must be encoded in UTF-8 without BOM.

Naming conventions:
- Namespaces, Classes, interfaces, enums, and structs: PascalCase (e.g., `MyNamespace`, `MyClass`, `IMyInterface`, `MyEnum`, `MyStruct`).
- Methods, properties, events: PascalCase (e.g., `MyMethod`, `MyProperty`, `MyEvent`).
- Constants: PascalCase (e.g., `MyConstant`).
- Local variables, method parameters : camelCase (e.g., `myVariable`, `myParameter`).
- Primary constructor parameters: _ prefix followed by camelCase (e.g., `_myParameter`).
- Instance fields: _ prefix followed by camelCase (e.g., `_myField`).
- Static fields: s_ prefix followed by camelCase (e.g., `s_myStaticField`).
- Fields must be always private.
- Always use access modifiers explicitly. Do not rely on default access modifiers.
- Asynchronous methods (returning `Task`/`Task<T>`/`ValueTask`) end with the `Async` suffix (e.g., `LoadDataAsync`, `CommitAsync`) and should accept `CancellationToken` as a last parameter. Blazor project where the source does not have access to `CancellationToken` is an exception to this rule.
- Generic type parameters are prefixed with `T` (e.g., `TItem`, `TValue`); a single type parameter may simply be `T`.

Code style guidelines:
- Prefer explicit types over `var`. Do not use `var`, even when the type is apparent.
- Use file-scoped namespace declarations (`namespace Foo;`), not block-scoped.
- Place `using` directives outside the namespace. Sort `System.*` directives first. Do not separate import groups with blank lines.
- Always use braces, even for single-line `if`/`for`/`while`/`foreach` bodies.
- Add parentheses for clarity in relational and binary operators (e.g., `(a != null) && (b != null)`).
- Prefer `readonly` for fields that are not reassigned after construction.
- Prefer `switch` expressions over `switch` statements where applicable.
- Forward `CancellationToken` to methods that accept one (CA2016).
- Do not leave primary-constructor parameters unread (CS9113).
