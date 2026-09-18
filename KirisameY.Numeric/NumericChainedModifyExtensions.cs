using JetBrains.Annotations;

namespace KirisameY.Numeric;

public static class NumericChainedModifyExtensions
{
    extension<TNumeric>(TNumeric numeric) where TNumeric : INumeric
    {
        [PublicAPI]
        public TNumeric WithUpdateHandler(EventHandler handler)
        {
            numeric.Updated += handler;
            return numeric;
        }
    }

    extension<TNumeric, TOrder>(TNumeric numeric)
        where TNumeric : IModifierEditableNumeric<TOrder>
        where TOrder : Enum
    {
        [PublicAPI]
        public TNumeric WithModifier(INumericModifier<TOrder> modifier)
        {
            numeric.AddModifier(modifier);
            return numeric;
        }
    }
}