using JetBrains.Annotations;

namespace KirisameY.Relinq.Extensions;

public static class ListExtensions
{
    extension<T>(IReadOnlyList<T> list)
    {
        [PublicAPI] public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < list.Count; i++)
            {
                if (comparer.Equals(item, list[i])) return i;
            }
            return -1;
        }
    }
}