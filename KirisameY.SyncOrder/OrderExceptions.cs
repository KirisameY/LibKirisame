using JetBrains.Annotations;

namespace KirisameY.SyncOrder;

public class OrderConsumedException(Order order) : Exception
{
    [PublicAPI] public Order Order => order;

    internal static void TryConsume(Order order, ref bool consumed)
    {
        if (consumed) throw new OrderConsumedException(order);
        consumed = true;
    }
}

public class OrderUncompletedException(Order order) : Exception
{
    [PublicAPI] public Order Order => order;
}

public class OrderUnsubmittedException(Order order) : Exception
{
    [PublicAPI] public Order Order => order;
}

public class OrderDuplicateCompleteException(Order order) : Exception
{
    [PublicAPI] public Order Order => order;
}

public class OrderDuplicateSubmitException(Order order) : Exception
{
    [PublicAPI] public Order Order => order;
}