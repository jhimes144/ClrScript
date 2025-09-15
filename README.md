# ClrScript

ClrScript (pronounced ClearScript) is an embedded dynamically-typed scripting language for .NET that is similar to JavaScript. It targets .NET 9.0 or .NET Standard 2.1

## Why ClrScript?

### Built For Real-Time

ClrScript was created out of the need of having an embedded scripting language for the .Net ecosystem suitable for games and real-time applications, where performance is critical. ClrScript is extremely fast, nearly as fast as C#. It accomplishes this by employing whole code compile-time analysis techniques for type inference and other optimizations. ClrScript is directly compiled to CLR (Common Language Runtime) bytecode, which is then JIT compiled at runtime. ClrScript offers the flexibility of dynamic typing during development with the performance of static typing at runtime.

Below is ClrScript benchmarks against other popular scripting languages for .Net

### Thread-Safe
ClrScript scripts can be called by multiple threads. Compilation of scripts is also thread-safe.

### Lightweight

ClrScript has zero dependencies. It does not rely on Microsoft's DLR (Dynamic Language Runtime), ANTLR, ect.

### 0 Cost Interop

ClrScript does not use reflection (unless optimizations cannot be performed) or marshalling to pass data between C# and ClrScript.

### Sandboxing

ClrScript has powerful sandboxing and security features.

- Configurable execution time constraint (stops a script from running too long)
- Ability to dictate whether static variables are allowed (made with `forever` modifier).
- Ability to dictate whether custom objects can be allocated.
- Ability to dictate whether arrays can be allocated.
- Standard library was created with security in mind, containing no built-in I/O or system functionality.
- And more.

## Why Not ClrScript?

ClrScript is a versatile language - but does not have all the features one would find in a more mature offering such as [Jint](https://github.com/sebastienros/jint). ClrScript is also its own language (despite being similar to JavaScript) - where other .Net solutions use existing well-known languages. If performance is not important, consider evaluating other embedded scripting offerings for .Net.