using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

/// <summary>
///     退订一个从未订阅过的处理器。
///     包装器内部是一张「处理器 → 转换用委托」的弱引用缓存表，
///     这种情况压根查不到记录，整条路径应当安静地什么都不做：
///     既不抛异常，也不能顺手把别人的订阅摘掉。
/// </summary>
public class UnregisteredHandlerUnsubscribeTests
{
    [Fact]
    public void UnknownHandlerOnACollectionViewIsIgnored()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableCollection();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);
        EventHandler<CollectionUpdateEventArgs<int>> unknown = (_, _) => { };
        list.Add(1);

        var exception = Record.Exception(() => view.CollectionUpdated -= unknown);
        list.Add(2);

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void UnknownHandlerOnAListViewIsIgnored()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);
        EventHandler<ListUpdateEventArgs<int>> unknown = (_, _) => { };
        list.Add(1);

        var exception = Record.Exception(() => view.ListUpdated -= unknown);
        list.Add(2);

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void UnknownHandlerOnADictionaryViewIsIgnored()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);
        EventHandler<DictionaryUpdateEventArgs<string, int>> unknown = (_, _) => { };
        dictionary.Add("a", 1);

        var exception = Record.Exception(() => view.DictionaryUpdated -= unknown);
        dictionary.Add("b", 2);

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void UnknownHandlerOnAProjectingCollectionViewIsIgnored()
    {
        var list = new NotifiableList<string>();
        var view = list.AsReadOnlyNotifiableCollection(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);
        EventHandler<CollectionUpdateEventArgs<int>> unknown = (_, _) => { };
        list.Add("a");

        var exception = Record.Exception(() => view.CollectionUpdated -= unknown);
        list.Add("bb");

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void UnknownHandlerOnAProjectingListViewIsIgnored()
    {
        var list = new NotifiableList<string>();
        var view = list.AsReadOnlyNotifiableList(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);
        EventHandler<ListUpdateEventArgs<int>> unknown = (_, _) => { };
        list.Add("a");

        var exception = Record.Exception(() => view.ListUpdated -= unknown);
        list.Add("bb");

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void UnknownHandlerOnAProjectingDictionaryViewIsIgnored()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);
        EventHandler<DictionaryUpdateEventArgs<string, string>> unknown = (_, _) => { };
        dictionary.Add("a", 1);

        var exception = Record.Exception(() => view.DictionaryUpdated -= unknown);
        dictionary.Add("b", 2);

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void UnknownHandlerOnTheDictionaryKeysViewIsIgnored()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);
        EventHandler<CollectionUpdateEventArgs<string>> unknown = (_, _) => { };
        dictionary.Add("a", 1);

        var exception = Record.Exception(() => dictionary.Keys.CollectionUpdated -= unknown);
        dictionary.Add("b", 2);

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void UnsubscribingTheSameHandlerTwiceIsHarmless()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableCollection();
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        view.CollectionUpdated += handler;
        list.Add(1);

        // 第一次退订会把缓存里的记录一并删掉（计数归零）
        view.CollectionUpdated -= handler;
        list.Add(2);

        // 第二次就查不到记录了，应当安静地什么都不做
        var exception = Record.Exception(() => view.CollectionUpdated -= handler);
        list.Add(3);

        Assert.Null(exception);
        Assert.Equal(1, count);
    }

    [Fact]
    public void UnsubscribingTheSameHandlerTwiceIsHarmlessOnADictionaryView()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();
        var count = 0;
        EventHandler<DictionaryUpdateEventArgs<string, int>> handler = (_, _) => count++;
        view.DictionaryUpdated += handler;
        dictionary.Add("a", 1);

        view.DictionaryUpdated -= handler;
        dictionary.Add("b", 2);

        var exception = Record.Exception(() => view.DictionaryUpdated -= handler);
        dictionary.Add("c", 3);

        Assert.Null(exception);
        Assert.Equal(1, count);
    }
}
