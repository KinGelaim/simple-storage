using System;

namespace SimpleStorage.Generators;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateBinarySerializerAttribute : Attribute;