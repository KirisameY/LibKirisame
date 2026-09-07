namespace KirisameY.SyncOrder;

public class OrderConsumedException(Order order) : Exception
{
    public Order Order => order;

    public static void TryConsume(Order order, ref bool consumed)
    {
        if (consumed) throw new OrderConsumedException(order);
        consumed = true;
    }
}

public class OrderUncompletedException(Order order) : Exception
{
    public Order Order => order;
}

public class OrderAlreadyCompletedException(Order order) : Exception
{
    public Order Order => order;
}