# Uno.MonoAnalyzers

A set of Roslyn C# analyzers for Xamarin and Mono-based code bases.

## UNOM0001 This API is not available in VS15.8 and earlier

This analyzer prevents the use of some BCL methods found in VS15.9 and later, to ensure build compatibility with VS 15.8 and earlier.

Validated Types:
-  `System.String`

### Description
Mono, Xamarin.iOS, Xamarin.Android, Xamarin.Mac targets do not version the runtime being used, which makes it difficult to build libraries that are backward compatible with previous versions of the runtime and BCL.

For instance, the `System.String` type from Visual Studio 15.9 contains a `TrimStart()` method, where as in only contains `TrimStart(params char[])` in previous releases. 

Building a library using VS 15.9, which makes a call as follows `myString.TrimStart()` the compiler will choose the method without parameters, which does not exist in VS15.8 and earlier. causing runtime exceptions such as `MissingMethodException` or linker errors which tries to determine the use of a method but cannot find it.

### Common fixes

#### System.String.TrimStart()
```csharp
var s = "";
s.TrimStart();
```
Should become:
```csharp
var s = "";
s.TrimStart(new char[0]);
```

## UNOM0003 Large number of static methods in a type with a static initializer

This analyzer flags type with a static initializer and a large number of static methods.

Such a pattern can create a very large set of boiler plate code that can be avoided by either removing static type initializers, or move static methods to instance methods and use a singleton.
