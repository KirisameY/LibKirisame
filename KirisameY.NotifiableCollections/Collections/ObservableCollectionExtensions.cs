using System.Collections.Specialized;

using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     把标准库风格的 <see cref="INotifyCollectionChanged"/> 集合包装成只读 Notifiable 视图的扩展方法。
///     <br/>
///     Extension methods that wrap a standard library-style <see cref="INotifyCollectionChanged"/>
///     collection into a read-only Notifiable view.
/// </summary>
public static class ObservableCollectionExtensions
{
    extension<TElement, TCollection>(TCollection collection) where TCollection : IReadOnlyCollection<TElement>, INotifyCollectionChanged
    {
        /// <summary>
        ///     将给定的任意实现了 <see cref="INotifyCollectionChanged"/> 的 <see cref="IReadOnlyCollection{T}"/> 包装为 <see cref="IReadOnlyNotifiableCollection{T}"/>，
        ///     即以该库的通知模型对外报告变更的只读视图。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyCollection{T}"/> that implements <see cref="INotifyCollectionChanged"/>
        ///     into an <see cref="IReadOnlyNotifiableCollection{T}"/>:
        ///     a read-only view that reports changes through the library's notification model.
        /// </summary>
        /// <returns>
        ///     与源集合同步的只读可通知视图。
        ///     <br/>
        ///     A read-only notifiable view kept in sync with source collection.
        /// </returns>
        [PublicAPI]
        public IReadOnlyNotifiableCollection<TElement> AsNotifiableCollection(TypeA<TElement> elementType = default) =>
            new NotifiableCollectionWrapper<TElement, TCollection>(collection);
    }

    extension<TItem, TList>(TList collection) where TList : IReadOnlyList<TItem>, INotifyCollectionChanged
    {
        /// <summary>
        ///     将给定的任意实现了 <see cref="INotifyCollectionChanged"/> 的 <see cref="IReadOnlyList{T}"/> 包装为 <see cref="IReadOnlyNotifiableList{T}"/>，
        ///     即以该库的通知模型对外报告变更的只读视图。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyList{T}"/> that implements <see cref="INotifyCollectionChanged"/>
        ///     into an <see cref="IReadOnlyNotifiableList{T}"/>:
        ///     a read-only view that reports changes through the library's notification model.
        /// </summary>
        /// <returns>
        ///     与源列表同步的只读可通知视图。
        ///     <br/>
        ///     A read-only notifiable view kept in sync with source list.
        /// </returns>
        [PublicAPI]
        public IReadOnlyNotifiableList<TItem> AsNotifiableList(TypeA<TItem> itemType = default) =>
            new NotifiableListWrapper<TItem, TList>(collection);
    }

    extension<TKey, TValue, TDictionary>(TDictionary collection)
        where TKey : notnull
        where TDictionary : IReadOnlyDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        /// <summary>
        ///     将给定的任意实现了 <see cref="INotifyCollectionChanged"/> 的 <see cref="IReadOnlyDictionary{TKey,TValue}"/>
        ///     包装为 <see cref="IReadOnlyNotifiableDictionary{TKey,TValue}"/>，即以该库的通知模型对外报告变更的只读视图。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyDictionary{TKey,TValue}"/> that implements <see cref="INotifyCollectionChanged"/>
        ///     into an <see cref="IReadOnlyNotifiableDictionary{TKey,TValue}"/>:
        ///     a read-only view that reports changes through the library's notification model.
        /// </summary>
        /// <returns>
        ///     与源字典同步的只读可通知视图。
        ///     <br/>
        ///     A read-only notifiable view kept in sync with source dictionary.
        /// </returns>
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsNotifiableDictionary(TypeA<TKey> keyType = default, TypeA<TValue> valueType = default) =>
            new NotifiableDictionaryWrapper<TKey, TValue, TDictionary>(collection);
    }
}