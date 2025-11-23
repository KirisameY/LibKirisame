namespace KirisameLib.Data.Registers;

public class GettingFallbackValueFailedException(object id, Exception inner) : Exception($"Failed to get fallback value for item: ID: {id}", inner);