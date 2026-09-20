using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

/// <summary>
///     包装器读侧的零散覆盖：投影列表视图的 Count 与索引器，
///     以及字典视图（原样 / 投影两条路）的 ContainsKey、TryGetValue、Keys、Values 和枚举。
/// </summary>
public class ViewReadTests
{
    [Fact]
    public void ProjectingListViewExposesCountAndIndexer()
    {
        var list = new NotifiableList<string> { "a", "bb", "ccc" };
        var view = list.AsReadOnlyNotifiableList(s => s.Length);

        Assert.Equal(3, view.Count);
        Assert.Equal(1, view[0]);
        Assert.Equal(2, view[1]);
        Assert.Equal(3, view[2]);
    }

    [Fact]
    public void ProjectingListViewCountAndIndexerTrackTheSource()
    {
        var list = new NotifiableList<string> { "a" };
        var view = list.AsReadOnlyNotifiableList(s => s.Length);

        list.Add("bbbb");

        // 活视图：源变了 Count 和索引器都跟着变
        Assert.Equal(2, view.Count);
        Assert.Equal(4, view[1]);
    }

    [Fact]
    public void DictionaryViewExposesKeysValuesAndCount()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary();

        Assert.Equal(["a", "b"], view.Keys);
        Assert.Equal([1, 2], view.Values);
        Assert.Equal(2, view.Keys.Count);
        Assert.Equal(2, view.Values.Count);
    }

    [Fact]
    public void DictionaryViewEnumeratesPairs()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary();

        Assert.Equal(
            [
                new KeyValuePair<string, int>("a", 1),
                new KeyValuePair<string, int>("b", 2)
            ],
            view
        );
    }

    [Fact]
    public void DictionaryViewReportsMisses()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary();

        Assert.False(view.ContainsKey("z"));
        Assert.False(view.TryGetValue("z", out var value));
        Assert.Equal(0, value);
    }

    [Fact]
    public void DictionaryViewThrowsForAMissingKey()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();

        Assert.Throws<KeyNotFoundException>(() => _ = view["z"]);
    }

    [Fact]
    public void ProjectingDictionaryViewContainsKeyAndTryGetValue()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());

        Assert.True(view.ContainsKey("a"));
        Assert.False(view.ContainsKey("z"));

        Assert.True(view.TryGetValue("b", out var hit));
        Assert.Equal("2", hit);

        // 未命中时值应当是 default，而不是把投影函数套在缺省值上跑一遍
        Assert.False(view.TryGetValue("z", out var miss));
        Assert.Null(miss);
    }

    [Fact]
    public void ProjectingDictionaryViewDoesNotProjectKeys()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());

        Assert.Equal(["a", "b"], view.Keys);
    }

    [Fact]
    public void ProjectingDictionaryViewCountTracksTheSource()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());

        dictionary.Add("a", 1);

        Assert.Single(view);
        Assert.Equal("1", view["a"]);
    }
}
