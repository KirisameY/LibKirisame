using System.ComponentModel;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     标准的 <see cref="INotifyPropertyChanged"/> 数据源，用于默认解析器与端到端绑定测试。
/// </summary>
/// <remarks>
///     每个 Setter 都做了相等判断——值没变就不发通知。这是双向绑定能在一轮之后收敛、
///     而不是无限递归的前提，相关测试依赖这个性质。
/// </remarks>
public class NotifyObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _number;

    public int Number
    {
        get => _number;
        set
        {
            if (_number == value) return;
            _number = value;
            Raise(nameof(Number));
        }
    }

    private string _text = "";

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            Raise(nameof(Text));
        }
    }

    /// <summary>只读属性：解析不出 setter，因此不构成可写端点。</summary>
    public int ReadOnlyNumber => _number;

    /// <summary>公开字段：表达式树里同样是 <c>MemberExpression</c>，一样受支持。</summary>
    public int Field;

    /// <summary>只读字段：<c>Expression.Assign</c> 会拒绝它，因此不构成可写端点。</summary>
    public readonly int ReadOnlyField = 0;

    private readonly Dictionary<string, int> _map = [];

    /// <summary>索引器：默认解析器把它映射成 <c>"Item[]"</c> 这个通知名。</summary>
    public int this[string key]
    {
        get => _map.GetValueOrDefault(key);
        set
        {
            if (_map.GetValueOrDefault(key) == value) return;
            _map[key] = value;
            Raise("Item[]");
        }
    }

    /// <summary>手动发出指定名字的通知，用来测试通知名的匹配规则。</summary>
    public void Raise(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
