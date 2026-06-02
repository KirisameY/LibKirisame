using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace KirisameY.Relinq;

[PublicAPI]
public delegate bool WhileSelector<in TSource, TResult>(TSource item, [MaybeNullWhen(false)] out TResult result);