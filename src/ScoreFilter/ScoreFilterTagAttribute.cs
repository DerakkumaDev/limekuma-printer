namespace Limekuma.ScoreFilter;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScoreFilterTagAttribute : Attribute
{
    public string Tag { get; }

    public ScoreFilterTagAttribute(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag is required.", nameof(tag));
        }

        Tag = tag;
    }
}