using JetBrains.Annotations;

namespace KirisameY.EventBus.EventComponents;

public interface ICancelableEvent
{
    // todo: 这里回头改成 ref 属性并写一个生成器为实现这个接口的自动生成ref属性，隔壁Variable也设定成用户输入Variable属性后自动在另外生成为ref属性
    EventCancelToken CancelToken { get; }
}

public static class CancelableEventExtensions
{
    extension(ICancelableEvent e)
    {
        [PublicAPI] public bool Canceled => e.CancelToken.Canceled;
        [PublicAPI] public bool Cancel() => e.CancelToken.Cancel();
    }
}