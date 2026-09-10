using KirisameY.SyncOrder.Await;

namespace KirisameY.SyncOrder.Async;

public class OrderAsyncMethodAwaitOtherAwaitableException(Type awaiterType) : Exception($"Expected {typeof(OrderAwaiter).FullName}, got {awaiterType.FullName}");

public class OrderAsyncMethodRunningException(Exception innerException) : AggregateException(innerException);