using System;
using System.Linq;
using System.Text;

namespace KirisameLib.GeneratorTools;

public class IndentStringBuilder(string indentContent)
{
    public IndentStringBuilder(int indentLength = 4) : this(new string(' ', indentLength)) { }

    private readonly StringBuilder _builder = new();

    public uint IndentLevel { get; private set; } = 0;
    private string _indentString = string.Empty;

    private bool _currentLineIntended = false;

    private void UpdateIndent() =>
        _indentString = string.Concat(Enumerable.Repeat(indentContent, (int)IndentLevel));

    public IndentStringBuilder SetIndent(uint indent)
    {
        IndentLevel = indent;
        UpdateIndent();
        return this;
    }

    public IndentStringBuilder IncreaseIndent(uint indent = 1)
    {
        IndentLevel += indent;
        UpdateIndent();
        return this;
    }

    public IndentStringBuilder DecreaseIndent(uint indent = 1)
    {
        IndentLevel -= indent;
        UpdateIndent();
        return this;
    }

    public readonly struct IndentDisposable(IndentStringBuilder builder, uint indent) : IDisposable
    {
        public void Dispose()
        {
            builder.DecreaseIndent(indent);
        }
    }

    public IndentDisposable Indent(uint indent = 1)
    {
        IncreaseIndent(indent);
        return new(this, indent);
    }

    public readonly struct IndentWithDisposable(IndentStringBuilder builder, string append, uint indent) : IDisposable
    {
        public void Dispose()
        {
            builder.DecreaseIndent(indent);
            builder.AppendLine(append);
        }
    }

    public IndentWithDisposable IndentWith(string prepend, string append, uint indent = 1)
    {
        AppendLine(prepend);
        IncreaseIndent(indent);
        return new(this, append, indent);
    }

    public IndentWithDisposable IndentWithBrace(uint indent = 1) => IndentWith("{", "}", indent);

    public IndentStringBuilder Append(string content)
    {
        if (!_currentLineIntended)
        {
            _builder.Append(_indentString);
            _currentLineIntended = true;
        }
        _builder.Append(content);
        return this;
    }

    public IndentStringBuilder AppendLine(string content)
    {
        if (!_currentLineIntended)
            _builder.Append(_indentString);
        _builder.AppendLine(content);
        _currentLineIntended = false;
        return this;
    }

    public IndentStringBuilder AppendLine() => AppendLine(string.Empty);

    public override string ToString() => _builder.ToString();
}