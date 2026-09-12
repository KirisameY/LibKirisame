using System.Diagnostics;

using JetBrains.Annotations;

namespace KirisameY.SyncOrder;

public class OrderConsumedException(Order order) : InvalidOperationException
{
    [PublicAPI] public Order Order => order;

    [StackTraceHidden]
    internal static void TryConsume(Order order, ref bool consumed)
    {
        if (consumed) throw new OrderConsumedException(order);
        consumed = true;
    }
}

public class OrderUncompletedException(Order order) : InvalidOperationException
{
    [PublicAPI] public Order Order => order;
}

public class OrderUnsubmittedException(Order order) : InvalidOperationException
{
    [PublicAPI] public Order Order => order;
}

public class OrderDuplicateCompleteException(Order order) : InvalidOperationException
{
    [PublicAPI] public Order Order => order;
}

public class OrderDuplicateSubmitException(Order order) : Exception // 非用户操作
{
    [PublicAPI] public Order Order => order;
}