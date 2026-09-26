using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KirisameY.BindingBridge.PropertyBinding.Resolver;

public class DefaultPropertyUpdateNotifyResolver : IPropertyUpdateNotifyResolver
{
    private DefaultPropertyUpdateNotifyResolver() { }
    public static DefaultPropertyUpdateNotifyResolver Instance => field ??= new();

    public PropertyUpdateNotifyProxy? Resolve(Type type, MemberInfo? memberInfo)
    {
        if (!type.IsAssignableTo(typeof(INotifyPropertyChanged))) return null;
        if (memberInfo is null) return null;

        var name = memberInfo.Name;
        if (memberInfo is PropertyInfo property && property.GetIndexParameters() is not []) name = $"{name}[]";

        return new(CreateSubscriber(name), CreateUnsubscriber(name));
    }


    private class HandlerRecord(PropertyChangedEventHandler observer, Dictionary<string, List<Action>> dict)
    {
        public static HandlerRecord Create()
        {
            Dictionary<string, List<Action>> dict = [];
            PropertyChangedEventHandler observer = (_, args) =>
            {
                if (!dict.TryGetValue(args.PropertyName!, out var list)) return;
                list.ForEach(a => a.Invoke());
            };

            return new(observer, dict);
        }

        public PropertyChangedEventHandler Observer => observer;
        public int Count = 0;

        private readonly Lock _lock = new();

        public int Add(string name, Action handler)
        {
            using var _ = _lock.EnterScope();
            if (!dict.TryGetValue(name, out var list))
                dict[name] = list = [];
            list.Add(handler);
            Count++;
            return Count;
        }

        public bool Remove(string name, Action handler, out int count)
        {
            using var _ = _lock.EnterScope();
            count = Count;
            if (!dict.TryGetValue(name, out var list)) return false;
            if (!list.Remove(handler)) return false;
            count = Count -= 1;
            return true;
        }
    }

    private readonly ConditionalWeakTable<
        INotifyPropertyChanged,
        HandlerRecord
    > _handlerTable = [];

    private readonly Lock _lock = new();

    private Action<object, Action> CreateSubscriber(string name) => (obj, handler) =>
    {
        var notifier = (INotifyPropertyChanged)obj!;

        HandlerRecord? record;
        using (_lock.EnterScope())
        {
            if (!_handlerTable.TryGetValue(notifier, out record))
            {
                _handlerTable.Add(notifier, record = HandlerRecord.Create());
            }
        }

        if (record.Add(name, handler) == 0) notifier.PropertyChanged += record.Observer;
    };

    private Action<object, Action> CreateUnsubscriber(string name) => (obj, handler) =>
    {
        var notifier = (INotifyPropertyChanged)obj!;

        HandlerRecord? record;
        using (_lock.EnterScope())
        {
            if (!_handlerTable.TryGetValue(notifier, out record)) return;
        }

        if (record.Remove(name, handler, out var c) && c == 0) notifier.PropertyChanged -= record.Observer;
    };
}