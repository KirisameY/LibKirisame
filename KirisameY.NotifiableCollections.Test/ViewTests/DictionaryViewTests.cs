using System.Collections;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

public class DictionaryViewTests
{
    [Fact]
    public void ViewReflectsTheSourceContents()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        var view = dictionary.AsReadOnlyNotifiableDictionary();

        Assert.Equal(2, view.Count);
        Assert.Equal(1, view["a"]);
        Assert.True(view.ContainsKey("b"));
        Assert.True(view.TryGetValue("b", out var value));
        Assert.Equal(2, value);
    }

    [Fact]
    public void ViewIsLive()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();

        dictionary.Add("a", 1);

        Assert.Equal(1, view["a"]);
        Assert.Equal(1, view.Count);
    }

    [Fact]
    public void ViewForwardsNotificationsAndSendsItselfAsSender()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();
        var updates = new List<DictionaryUpdateEventArgs<string, int>>();
        object? sender = null;
        view.DictionaryUpdated += (s, args) =>
        {
            sender = s;
            updates.Add(args);
        };

        dictionary.Add("a", 1);

        Assert.Single(updates);
        Assert.Same(view, sender);
    }

    [Fact]
    public void ViewStopsForwardingAfterUnsubscribing()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();
        var count = 0;
        EventHandler<DictionaryUpdateEventArgs<string, int>> handler = (_, _) => count++;
        view.DictionaryUpdated += handler;
        dictionary.Add("a", 1);

        view.DictionaryUpdated -= handler;
        dictionary.Add("b", 2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void ProjectionViewKeepsKeysAndProjectsValues()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v * 10);

        Assert.Equal(2, view.Count);
        Assert.Equal(10, view["a"]);
        Assert.Equal(["a", "b"], view.Keys);
        Assert.Equal([10, 20], view.Values);
    }

    [Fact]
    public void ProjectionViewReportsMissesAsDefault()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());

        Assert.False(view.TryGetValue("b", out var value));
        Assert.Null(value);
    }

    [Fact]
    public void ProjectionViewMapsTheValuesCarriedByNotifications()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);

        dictionary.Add("a", 1);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, string>>(Assert.Single(updates));
        Assert.Equal("1", added.AddedItems["a"]);
    }

    [Fact]
    public void ProjectionViewMapsReplacedValues()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);

        dictionary["a"] = 2;

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, string>>(Assert.Single(updates));
        Assert.Equal("1", replaced.OldItems["a"]);
        Assert.Equal("2", replaced.NewItems["a"]);
    }

    [Fact]
    public void ProjectionViewMapsRemovedValues()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);

        dictionary.Remove("a");

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, string>>(Assert.Single(updates));
        Assert.Equal("1", removed.RemovedItems["a"]);
    }

    [Fact]
    public void UpcastViewUsesTheDummyTypeArgument()
    {
        var dictionary = new NotifiableDictionary<string, string> { { "a", "x" } };

        IReadOnlyNotifiableDictionary<string, object> view = dictionary.AsReadOnlyNotifiableDictionary(TypeA.Of<object>());

        Assert.Equal("x", view["a"]);
    }

    [Fact]
    public void ViewIsUsableAsAPlainNotifiableCollection()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates<KeyValuePair<string, int>>(view);

        dictionary.Add("a", 1);

        Assert.Single(updates);
    }

    [Fact]
    public void NonGenericEnumerationAlsoYieldsProjectedValues()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v * 10);

        // 非泛型枚举也要走投影，而不是把源键值对原样吐出来
        Assert.Equal(
            [
                new KeyValuePair<string, int>("a", 10),
                new KeyValuePair<string, int>("b", 20)
            ],
            ((IEnumerable)view).Cast<KeyValuePair<string, int>>()
        );
    }

    [Fact]
    public void SameHandlerSubscribedTwiceIsCountedSeparately()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();
        var count = 0;
        EventHandler<DictionaryUpdateEventArgs<string, int>> handler = (_, _) => count++;
        view.DictionaryUpdated += handler;
        view.DictionaryUpdated += handler;

        dictionary.Add("a", 1);
        Assert.Equal(2, count);

        // 退订一次只摘掉一份，另一份还在
        view.DictionaryUpdated -= handler;
        dictionary.Add("b", 2);
        Assert.Equal(3, count);

        view.DictionaryUpdated -= handler;
        dictionary.Add("c", 3);
        Assert.Equal(3, count);
    }
}
