using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

/// <summary>
///     <see cref="NotifiableDictionary{TKey,TValue}"/> 上批量操作的语义：
///     <c>AddRange</c> / <c>TryAddRange</c> / <c>PutRange</c> / <c>RemoveRange</c>，
///     外加带 <c>out</c> 的 <c>Remove</c> 和键值对重载的 <c>TryAdd</c>。
/// </summary>
/// <remarks>
///     和逐个调用相比，批量操作的关键差别是<b>通知粒度</b>：一次调用不管改了多少项都只发一次通知，
///     通知里带上本次改动的全部键值对。所以下面大多在钉「发了几次、每次带了什么、字典最后是什么样」。
///     <br/>
///     <c>PutRange</c> 是唯一会发<b>两种</b>通知的：替换和新增各发一次，顺序固定为「先替换、后新增」，
///     两条通知里各只带自己那一类。
/// </remarks>
public class DictionaryBatchChangeTests
{
    /// <summary>
    ///     批量操作收发的都是 ImmutableDictionary，枚举顺序不保证，比较前先按键排一下。
    /// </summary>
    private static KeyValuePair<string, int>[] Sorted(IEnumerable<KeyValuePair<string, int>> pairs) =>
        [.. pairs.OrderBy(p => p.Key, StringComparer.Ordinal)];


    // ---------- AddRange ----------

    [Fact]
    public void AddRangeAddsEveryPairAndReportsThemInOneEvent()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.AddRange([new("a", 1), new("b", 2), new("c", 3)]);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(Sorted(dictionary), Sorted(added.AddedItems));
        Assert.Equal(Sorted(dictionary), Sorted(added.DictionaryView));
        var count = dictionary.Count;
        Assert.Equal(3, count);
    }

    [Fact]
    public void AddRangeWithNoPairsIsANoOp()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.AddRange([]);

        Assert.Empty(updates);
        Assert.Single(dictionary);
    }

    [Fact]
    public void AddRangeThrowsForAKeyThatIsAlreadyPresent()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        // 和逐个 Add 一样，撞上已有的键是错误用法，而不是「跳过」
        Assert.Throws<ArgumentException>(() => dictionary.AddRange([new("a", 9)]));

        Assert.Equal(1, dictionary["a"]);
        Assert.Empty(updates);
    }

    [Fact]
    public void AddRangeThrowsForDuplicateKeysInsideTheInputAndChangesNothing()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        // 入参先被整体快照，所以发现重复键时一个都还没写进去
        Assert.Throws<ArgumentException>(() => dictionary.AddRange([new("a", 1), new("a", 2)]));

        Assert.Empty(dictionary);
        Assert.Empty(updates);
    }

    [Fact]
    public void AddRangeRollsBackWhatItAlreadyWroteWhenAKeyTurnsOutToBeDuplicated()
    {
        // 冲突会落在入参的第几个位置，取决于 ImmutableDictionary 的枚举顺序（不保证），
        // 所以让每个键轮流当一次「那个冲突键」，确保「已经写进去几项再回退」这条路径一定被走到
        string[] keys = ["a", "b", "c", "d", "e", "f", "g", "h"];
        foreach (var conflicting in keys)
        {
            var dictionary = new NotifiableDictionary<string, int> { { conflicting, 0 } };
            var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

            Assert.Throws<ArgumentException>(
                () => dictionary.AddRange(keys.Select(k => new KeyValuePair<string, int>(k, 1))));

            // 抛异常时字典要回到调用前的样子：只剩原本那一项，它的值也没被覆盖
            Assert.Single(dictionary);
            Assert.Equal(0, dictionary[conflicting]);

            // 回退是悄悄发生的，不能留下通知
            Assert.Empty(updates);
        }
    }


    // ---------- TryAdd ----------

    [Fact]
    public void TryAddByPairBehavesLikeTheKeyAndValueOverload()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.False(dictionary.TryAdd(new KeyValuePair<string, int>("a", 9)));
        Assert.Empty(updates);
        Assert.Equal(1, dictionary["a"]);

        Assert.True(dictionary.TryAdd(new KeyValuePair<string, int>("b", 2)));

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(2, added.AddedItems["b"]);
        var addedCount = added.AddedItems.Count;
        Assert.Equal(1, addedCount);
    }


    // ---------- TryAddRange ----------

    [Fact]
    public void TryAddRangeAddsEveryPairAndHandsBackWhatItAdded()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var returned = dictionary.TryAddRange([new("a", 1), new("b", 2), new("c", 3)]);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(Sorted(dictionary), Sorted(added.AddedItems));
        Assert.Equal(Sorted(dictionary), Sorted(returned));
    }

    [Fact]
    public void TryAddRangeOnlyTouchesKeysThatAreNotThereYet()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var returned = dictionary.TryAddRange([new("a", 9), new("b", 2)]);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(2, added.AddedItems["b"]);
        var addedCount = added.AddedItems.Count;
        Assert.Equal(1, addedCount);

        // 已有的键原封不动，也不会出现在返回值里
        Assert.Equal(1, dictionary["a"]);
        Assert.Equal(2, returned["b"]);
        var returnedCount = returned.Count;
        Assert.Equal(1, returnedCount);
    }

    [Fact]
    public void TryAddRangeWithNothingToAddReportsNothing()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var returned = dictionary.TryAddRange([new("a", 9)]);

        Assert.Empty(updates);
        Assert.Empty(returned);
        Assert.Equal(1, dictionary["a"]);
    }

    [Fact]
    public void TryAddRangeWithNoPairsIsANoOp()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.Empty(dictionary.TryAddRange([]));

        Assert.Empty(updates);
        Assert.Empty(dictionary);
    }

    [Fact]
    public void TryAddRangeKeepsTheFirstOfDuplicateKeysInsideTheInput()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        // 第二项再 TryAdd 时键已经在了，于是被当成「已存在」跳过
        dictionary.TryAddRange([new("a", 1), new("a", 2)]);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, added.AddedItems["a"]);
        Assert.Equal(1, dictionary["a"]);
    }


    // ---------- PutRange ----------

    [Fact]
    public void PutRangeWithOnlyNewKeysReportsASingleAdd()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.PutRange([new("a", 1), new("b", 2)]);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(Sorted(dictionary), Sorted(added.AddedItems));
    }

    [Fact]
    public void PutRangeWithOnlyKnownKeysReportsASingleReplace()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.PutRange([new("a", 9), new("b", 8)]);

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, replaced.OldItems["a"]);
        Assert.Equal(2, replaced.OldItems["b"]);
        Assert.Equal(9, replaced.NewItems["a"]);
        Assert.Equal(8, replaced.NewItems["b"]);
        Assert.Equal(9, dictionary["a"]);
        Assert.Equal(8, dictionary["b"]);
    }

    [Fact]
    public void PutRangePairsEachReplacedKeyWithItsOldAndNewValue()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.PutRange([new("b", 8), new("a", 9)]);

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        var changes = replaced.ItemChanges.OrderBy(c => c.Key, StringComparer.Ordinal);
        Assert.Collection(
            changes,
            change =>
            {
                Assert.Equal("a", change.Key);
                Assert.Equal(1, change.OldValue);
                Assert.Equal(9, change.NewValue);
            },
            change =>
            {
                Assert.Equal("b", change.Key);
                Assert.Equal(2, change.OldValue);
                Assert.Equal(8, change.NewValue);
            }
        );
    }

    [Fact]
    public void PutRangeReportsReplacementsBeforeAdditions()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.PutRange([new("a", 9), new("b", 2)]);

        var updateCount = updates.Count;
        Assert.Equal(2, updateCount);

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(updates[0]);
        Assert.Equal(1, replaced.OldItems["a"]);
        Assert.Equal(9, replaced.NewItems["a"]);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(updates[1]);
        Assert.Equal(2, added.AddedItems["b"]);

        // 两类通知各带各的，互不重复
        var replacedCount = replaced.NewItems.Count;
        var addedCount = added.AddedItems.Count;
        Assert.Equal(1, replacedCount);
        Assert.Equal(1, addedCount);
        Assert.Equal(9, dictionary["a"]);
        Assert.Equal(2, dictionary["b"]);
    }

    [Fact]
    public void PutRangeWithNoPairsIsANoOp()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.PutRange([]);

        Assert.Empty(updates);
        Assert.Single(dictionary);
    }


    // ---------- RemoveRange ----------

    [Fact]
    public void RemoveRangeRemovesEveryKeyAndHandsBackTheirValues()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var returned = dictionary.RemoveRange(["a", "b"]);

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);
        Assert.Equal(2, removed.RemovedItems["b"]);
        Assert.Equal(1, returned["a"]);
        Assert.Equal(2, returned["b"]);
        Assert.Equal(3, dictionary["c"]);
        Assert.Single(dictionary);
    }

    [Fact]
    public void RemoveRangeSkipsKeysThatAreNotThere()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var returned = dictionary.RemoveRange(["a", "zz"]);

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);
        var removedCount = removed.RemovedItems.Count;
        var returnedCount = returned.Count;
        Assert.Equal(1, removedCount);
        Assert.Equal(1, returnedCount);
        Assert.Empty(dictionary);
    }

    [Fact]
    public void RemoveRangeWithNothingToRemoveReportsNothing()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var returned = dictionary.RemoveRange(["zz"]);

        Assert.Empty(updates);
        Assert.Empty(returned);
        Assert.Single(dictionary);
    }

    [Fact]
    public void RemoveRangeWithNoKeysIsANoOp()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.Empty(dictionary.RemoveRange([]));

        Assert.Empty(updates);
        Assert.Single(dictionary);
    }

    [Fact]
    public void RemoveRangeRemovesARepeatedKeyOnlyOnce()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.RemoveRange(["a", "a"]);

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);
        var removedCount = removed.RemovedItems.Count;
        Assert.Equal(1, removedCount);
        Assert.Empty(dictionary);
    }


    // ---------- Remove(key, out value) ----------

    [Fact]
    public void RemoveWithOutValueHandsBackTheValueItRemoved()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.True(dictionary.Remove("a", out var value));

        Assert.Equal(1, value);
        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);
    }

    [Fact]
    public void RemoveWithOutValueReportsNothingForAKeyThatIsNotThere()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.False(dictionary.Remove("zz", out var value));

        Assert.Equal(0, value);
        Assert.Empty(updates);
        Assert.Single(dictionary);
    }


    // ---------- Keys / Values 视图 ----------

    [Fact]
    public void KeysViewReportsEveryKeyOfABatchAdd()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);

        dictionary.AddRange([new("a", 1), new("b", 2), new("c", 3)]);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<string>>(Assert.Single(updates));
        Assert.Equal(["a", "b", "c"], added.AddedItems.OrderBy(k => k, StringComparer.Ordinal));
    }

    [Fact]
    public void KeysViewReportsEveryKeyOfABatchRemove()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);

        dictionary.RemoveRange(["a", "c"]);

        var removed = Assert.IsAssignableFrom<ICollectionItemRemovedEventArgs<string>>(Assert.Single(updates));
        Assert.Equal(["a", "c"], removed.RemovedItems.OrderBy(k => k, StringComparer.Ordinal));
    }

    [Fact]
    public void KeysViewIgnoresTheReplacementsInsideAPutRange()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);

        dictionary.PutRange([new("a", 9), new("b", 2)]);

        // 键集合只在「新增」那一半上有反应，替换那一半与它无关
        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<string>>(Assert.Single(updates));
        Assert.Equal(["b"], added.AddedItems);
    }

    [Fact]
    public void ValuesViewReportsEveryValueOfABatchAdd()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Values);

        dictionary.AddRange([new("a", 1), new("b", 2), new("c", 3)]);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], added.AddedItems.OrderBy(v => v));
    }

    [Fact]
    public void ValuesViewReportsEveryValueOfABatchRemove()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Values);

        dictionary.RemoveRange(["a", "c"]);

        var removed = Assert.IsAssignableFrom<ICollectionItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 3], removed.RemovedItems.OrderBy(v => v));
    }
}
