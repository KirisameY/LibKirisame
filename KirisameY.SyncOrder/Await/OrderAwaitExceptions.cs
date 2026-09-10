namespace KirisameY.SyncOrder.Await;

public class OrderAwaitedNotInOrderMethodException() : Exception($"Tried to awaited an order not in a async method that returns {nameof(Order)} or {nameof(Order<>)}") { }