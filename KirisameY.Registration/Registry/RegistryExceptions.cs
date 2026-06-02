namespace KirisameY.Registration.Registry;

public class GettingFallbackValueFailedException(IRegistry registry,object key, Exception inner) : Exception($"Failed to get fallback value in {registry} for item: ID: {key}", inner);