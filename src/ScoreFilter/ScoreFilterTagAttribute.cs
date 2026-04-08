namespace Limekuma.ScoreFilter;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScoreFilterTagAttribute : Attribute
{
    public string Tag { get; }

    public bool MaskMutex { get; }

    public ScoreFilterTagAttribute(string tag) : this(tag, false) { }

    public ScoreFilterTagAttribute(string tag, bool maskMutex)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag is required.", nameof(tag));
        }

        Tag = tag;
        MaskMutex = maskMutex;
    }
}