using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

using KirisameY.Relinq.Extensions;

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


    private class HandlerRecord()
    {
        private ImmutableDictionary<string, ImmutableList<Action>> _dict = [];


        public PropertyChangedEventHandler Observer => field ??= (_, args) =>
        {
            if (string.IsNullOrEmpty(args.PropertyName))
            {
                _dict.Values.Flatten().ForEach(a => a.Invoke());
                return;
            }

            if (!_dict.TryGetValue(args.PropertyName, out var list)) return;

            list.ForEach(a => a.Invoke());
        };

        private int _count = 0;

        private readonly Lock _lock = new();

        public int Add(string name, Action handler)
        {
            using var _ = _lock.EnterScope();
            if (!_dict.TryGetValue(name, out var list)) list = [];
            _dict = _dict.SetItem(name, list.Add(handler));
            return _count++;
        }

        public bool Remove(string name, Action handler, out int count)
        {
            using var _ = _lock.EnterScope();
            count = _count;
            if (!_dict.TryGetValue(name, out var list)) return false;
            var removed = list.Remove(handler);
            if (removed == list) return false;
            _dict = _dict.SetItem(name, removed);
            count = _count -= 1;
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
        var notifier = (INotifyPropertyChanged)obj;

        HandlerRecord? record;
        using (_lock.EnterScope())
        {
            if (!_handlerTable.TryGetValue(notifier, out record))
            {
                _handlerTable.Add(notifier, record = new HandlerRecord());
            }
        }

        if (record.Add(name, handler) == 0) notifier.PropertyChanged += record.Observer;
    };

    private Action<object, Action> CreateUnsubscriber(string name) => (obj, handler) =>
    {
        var notifier = (INotifyPropertyChanged)obj;

        HandlerRecord? record;
        using (_lock.EnterScope())
        {
            if (!_handlerTable.TryGetValue(notifier, out record)) return;
        }

        if (record.Remove(name, handler, out var c) && c == 0) notifier.PropertyChanged -= record.Observer;
    };
}