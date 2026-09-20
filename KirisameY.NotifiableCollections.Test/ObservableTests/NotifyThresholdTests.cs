using System.Collections.Specialized;

using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.ObservableTests;

/// <summary>
///     <c>notifyThreshold</c> 的语义：一次变更到底该逐项发出通知，还是塌缩成单次 Reset。
/// </summary>
/// <remarks>
///     <para>
///         三档约定为：负值不设上限（始终逐项）、<c>0</c> 始终 Reset、正值在变化项数<b>超过</b>阈值时 Reset。
///         阈值只作用于 <c>CollectionChanged</c>，对 <c>PropertyChanged</c> 没有影响。
///     </para>
///     <para>
///         逐项与 Reset 的区别靠"一次变更携带多个项"来触发，所以用各集合的批量操作
///         （<c>AddRange</c> / <c>Clear</c>）制造多顶变更，而不是连调多次单项 API。
///     </para>
/// </remarks>
public class NotifyThresholdTests
{
    [Fact]
    public void DefaultThresholdNeverCollapsesToReset()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.AddRange(new List<int> { 1, 2, 3, 4, 5 });

        Assert.Equal([1, 2, 3, 4, 5], ObservableTestUtils.NewItemsOf<int>(events));
        Assert.All(events, e => Assert.Equal(NotifyCollectionChangedAction.Add, e.Action));
    }

    [Fact]
    public void AnyNegativeThresholdNeverCollapsesToReset()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection(-3);
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.AddRange(new List<int> { 1, 2, 3, 4, 5 });

        // 负值一律视为"不设上限"，不是只有 -1 特殊
        Assert.Equal([1, 2, 3, 4, 5], ObservableTestUtils.NewItemsOf<int>(events));
        Assert.All(events, e => Assert.Equal(NotifyCollectionChangedAction.Add, e.Action));
    }

    [Fact]
    public void ZeroThresholdAlwaysRaisesResetEvenForASingleItem()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection(0);
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Add(1);

        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
    }

    [Fact]
    public void PositiveThresholdKeepsPerItemNotificationsUpToTheThreshold()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection(3);
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.AddRange(new List<int> { 1, 2, 3 });

        // 边界是"超过"而非"达到"，项数正好等于阈值时仍然逐项发出
        Assert.Equal([1, 2, 3], ObservableTestUtils.NewItemsOf<int>(events));
        Assert.All(events, e => Assert.Equal(NotifyCollectionChangedAction.Add, e.Action));
    }

    [Fact]
    public void PositiveThresholdCollapsesToResetBeyondTheThreshold()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection(3);
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.AddRange(new List<int> { 1, 2, 3, 4 });

        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
    }

    [Fact]
    public void ThresholdAppliesToRemoveAndReplaceToo()
    {
        NotifiableList<int> list = [1, 2];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection(0);
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list[0] = 9;
        list.RemoveAt(1);

        Assert.Equal(
            [NotifyCollectionChangedAction.Reset, NotifyCollectionChangedAction.Reset],
            events.Select(e => e.Action)
        );
    }

    [Fact]
    public void ThresholdDoesNotAffectPropertyChanged()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection(0);
        var events = ObservableTestUtils.RecordCollectionChanged(view);
        var names = ObservableTestUtils.RecordPropertyChanged(view);

        list.Add(1);

        // CollectionChanged 塌缩成 Reset，但 PropertyChanged 照旧按 Count 发出
        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
        Assert.Equal(["Count"], names);
    }

    [Fact]
    public void ListThresholdCollapsesClearToResetBeyondTheThreshold()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection(2);
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Clear();

        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
    }

    [Fact]
    public void ListZeroThresholdAlwaysRaisesReset()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection(0);
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Add(1);

        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
    }

    [Fact]
    public void ListSortRaisesResetEvenWithAnUnlimitedThreshold()
    {
        NotifiableList<int> list = [2, 1];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Sort();

        // 排序无法逐项表达，跟阈值无关，一律 Reset
        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
    }

    [Fact]
    public void DictionaryDefaultThresholdNeverCollapsesClearToReset()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);
        dict.Add("c", 3);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        dict.Clear();

        Assert.All(events, e => Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action));
        Assert.Equal(
            ["a", "b", "c"],
            events.Select(e => ((KeyValuePair<string, int>)e.OldItems![0]!).Key).OrderBy(k => k)
        );
    }

    [Fact]
    public void DictionaryZeroThresholdCollapsesClearToReset()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);
        dict.Add("c", 3);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary(0);
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        dict.Clear();

        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
    }

    [Fact]
    public void DictionaryPassesTheThresholdToItsKeysView()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var keys = ObservableTestUtils.RecordCollectionChanged(view.Keys);
        var values = ObservableTestUtils.RecordCollectionChanged(view.Values);

        dict.Add("a", 1);

        // 默认阈值下两个子视图都逐项发出
        Assert.Equal(NotifyCollectionChangedAction.Add, Assert.Single(keys).Action);
        Assert.Equal(NotifyCollectionChangedAction.Add, Assert.Single(values).Action);
    }

    [Fact]
    public void DictionaryPassesAZeroThresholdToItsKeysAndValuesViews()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary(0);
        var keys = ObservableTestUtils.RecordCollectionChanged(view.Keys);
        var values = ObservableTestUtils.RecordCollectionChanged(view.Values);

        dict.Add("a", 1);

        // 阈值 0 一并传给子视图，于是单项变更也塌缩成 Reset
        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(keys).Action);
        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(values).Action);
    }
}
