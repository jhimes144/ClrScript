NOTE: This is currently still a WIP - I have not yet advertised this project :smiley:

# ClrScript

ClrScript (pronounced ClearScript) is a dynamically-typed scripting language for .NET that is written similar to JavaScript. Its primary purpose is to be a scripting language that you can embed in your application, allowing users to write scripts for your app. It targets .NET 9.0 or .NET Standard 2.1.

## Why ClrScript?

### Built For Real-Time

ClrScript was created primarily out of the need of having an embedded scripting language for the .Net ecosystem suitable for games and real-time applications, where performance is critical. ClrScript is extremely fast, nearly as fast as C# on average and faster than all other .Net scripting solutions on average. It accomplishes this by employing a sophisticated type inference algorithm at compile time, so that statically typed bytecode can be generated where possible. ClrScript is directly compiled to CLR (Common Language Runtime) bytecode, which is then JIT compiled at runtime.

Below are ClrScript benchmarks against other popular scripting languages for .Net, with C# as a control.

### Unity Compatible
Compatible with the Unity game engine.

### Thread-Safe
Scripts can be called by multiple threads. Compilation of scripts is also thread-safe.

### Lightweight

Has zero dependencies. It does not rely on Microsoft's DLR (Dynamic Language Runtime), ANTLR, ect.

### Zero Cost Interop

Does not use reflection (unless optimizations cannot be performed) or marshalling to pass data between C# and ClrScript.

### Sandboxing

Powerful sandboxing and security features.

- Configurable execution time constraint (stops a script from running too long)
- Ability to dictate whether custom objects can be allocated.
- Ability to dictate whether arrays can be allocated.
- Standard library was created with security in mind. Standard library modules are configurable for desired level of security.
- And more.

### Ergonomics

VS Code extension which allows:

- Hover documentation for app specific types for your users.
- Automatic deployment API to deploy scripts directly to application. (If desired)
- Langauge syntax highlighting features and more.

## Why Not ClrScript?

ClrScript is a versatile language - but does not have all the features one would find in a more mature offering such as [Jint](https://github.com/sebastienros/jint). ClrScript is also its own language (despite being similar to JavaScript) - where other .Net solutions use existing well-known languages.