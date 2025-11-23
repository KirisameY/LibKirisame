using Microsoft.CodeAnalysis;

namespace KirisameLib.GeneratorTools.Extensions;

public static class ProviderExtensions
{
    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> provider) =>
        provider.Where(x => x is not null).Select((x, _) => x!);
}