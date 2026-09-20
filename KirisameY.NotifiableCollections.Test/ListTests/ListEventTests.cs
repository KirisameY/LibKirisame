using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

public class ListEventTests
{
    [Fact]
    public void SenderIsTheListItself()
    {
        var list = new NotifiableList<int>();
        object? sender = null;
        list.ListUpdated += (s, _) => sender = s;

        list.Add(1);

        Assert.Same(list, sender);
    }

    [Fact]
    public void ListViewReflectsTheStateAfterTheChange()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        IReadOnlyList<int>? view = null;
        list.ListUpdated += (_, args) => view = args.ListView;

        list.RemoveAt(0);

        // 事件在内部列表改完之后才触发，所以视图是变更后的状态
        Assert.Equal([2, 3], view);
    }

    [Fact]
    public void ListViewIsLiveNotSnapshot()
    {
        var list = new NotifiableList<int>();
        IReadOnlyList<int>? view = null;
        list.ListUpdated += (_, args) => view = args.ListView;

        list.Add(1);
        Assert.Equal([1], view);

        // 事件里拿到的是活视图，之后列表再变它也跟着变
        list.Add(2);
        Assert.Equal([1, 2], view);
    }

    [Fact]
    public void HandlersObserveTheListAfterTheChange()
    {
        var list = new NotifiableList<int>();
        List<int> observed = [];
        list.ListUpdated += (_, _) => observed.Add(list.Count);

        list.Add(1);
        list.Add(2);

        Assert.Equal([1, 2], observed);
    }

    [Fact]
    public void UnsubscribedHandlerStopsReceiving()
    {
        var list = new NotifiableList<int>();
        var count = 0;
        EventHandler<ListUpdateEventArgs<int>> handler = (_, _) => count++;
        list.ListUpdated += handler;
        list.Add(1);

        list.ListUpdated -= handler;
        list.Add(2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void CollectionUpdatedAndListUpdatedAreTheSameEvent()
    {
        var list = new NotifiableList<int>();
        ListUpdateEventArgs<int>? fromList = null;
        CollectionUpdateEventArgs<int>? fromCollection = null;
        list.ListUpdated += (_, args) => fromList = args;
        ((ICollectionUpdateNotifier<int>)list).CollectionUpdated += (_, args) => fromCollection = args;

        list.Add(1);

        // 集合级事件是显式实现转发到 ListUpdated 的，两边收到的是同一个 args 实例
        Assert.Same(fromList, fromCollection);
    }

    [Fact]
    public void ListIsUsableAsAPlainNotifiableCollection()
    {
        var list = new NotifiableList<int>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates<int>(list);

        list.Add(1);
        list.Add(2);

        Assert.Equal(2, updates.Count);
    }
}
