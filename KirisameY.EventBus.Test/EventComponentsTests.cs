using KirisameY.EventBus.Bus;
using KirisameY.EventBus.EventComponents;

namespace KirisameY.EventBus.Test;

public class EventComponentsTests
{
    // ---------- EventCancelToken ----------

    [Fact]
    public void NewCancelTokenIsNotCanceled()
    {
        var token = new EventCancelToken();

        Assert.False(token.Canceled);
    }

    [Fact]
    public void CancelMarksTheTokenCanceled()
    {
        var token = new EventCancelToken();

        token.Cancel();

        Assert.True(token.Canceled);
    }

    [Fact]
    public void FirstCancelReturnsTrue()
    {
        var token = new EventCancelToken();

        // 返回 true 表示这次调用确实把 token 取消了
        Assert.True(token.Cancel());
    }

    [Fact]
    public void CancelReturnsFalseWhenAlreadyCanceled()
    {
        var token = new EventCancelToken();
        token.Cancel();

        // 已经取消过了，这次调用没有产生任何效果
        Assert.False(token.Cancel());
    }

    [Fact]
    public void RepeatedCancelSucceedsOnlyOnce()
    {
        var token = new EventCancelToken();

        Assert.True(token.Cancel());
        Assert.False(token.Cancel());
        Assert.False(token.Cancel());

        Assert.True(token.Canceled);
    }

    // ---------- EventVariable ----------

    [Fact]
    public void EventVariableExposesInitialValue()
    {
        var variable = new EventVariable<int>(42);

        Assert.Equal(42, variable.Value);
    }

    [Fact]
    public void EventVariableValueIsWritable()
    {
        var variable = new EventVariable<int>(42);

        variable.Value = 7;

        Assert.Equal(7, variable.Value);
    }

    [Fact]
    public void EventVariableAcceptsNullForReferenceTypes()
    {
        var variable = new EventVariable<string?>(null);

        Assert.Null(variable.Value);

        variable.Value = "text";

        Assert.Equal("text", variable.Value);
    }

    // ---------- 组件放进真实事件 ----------

    private record MutableTestEvent : BaseEvent, ICancelableEvent
    {
        private readonly EventCancelToken _cancelToken = new();
        private readonly EventVariable<string> _setting = new("initial");

        public EventCancelToken CancelToken => _cancelToken;

        public EventVariable<string> Setting => _setting;
    }

    [Fact]
    public void EventIsNotCanceledByDefault()
    {
        var @event = new MutableTestEvent();

        Assert.False(@event.Canceled);
    }

    [Fact]
    public void CanceledReflectsDirectCancelOnTheToken()
    {
        var @event = new MutableTestEvent();

        @event.CancelToken.Cancel();

        Assert.True(@event.Canceled);
    }

    [Fact]
    public void CancellingThroughTheEventCancelsIt()
    {
        var @event = new MutableTestEvent();

        @event.Cancel();

        Assert.True(@event.Canceled);
    }

    [Fact]
    public void CancelThroughTheEventReturnsTrueOnlyOnce()
    {
        var @event = new MutableTestEvent();

        Assert.True(@event.Cancel());
        Assert.False(@event.Cancel());
    }

    [Fact]
    public void SettingTakenFromTheEventCanBeUpdated()
    {
        var @event = new MutableTestEvent();

        @event.Setting.Value = "updated";

        Assert.Equal("updated", @event.Setting.Value);
    }

    [Fact]
    public void HandlerCanUpdateSettingThroughTheEvent()
    {
        var bus = new SimpleEventBus();
        bus.Subscribe<MutableTestEvent>(e => e.Setting.Value = "handled");
        var @event = new MutableTestEvent();

        bus.Publish(@event);

        Assert.Equal("handled", @event.Setting.Value);
    }
}
