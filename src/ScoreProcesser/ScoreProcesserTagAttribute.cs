namespace Limekuma.ScoreProcesser;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScoreProcesserTagAttribute : Attribute
{
    public ScoreProcesserTagAttribute(string tag) : this(tag, false, false)
    {
    }

    public ScoreProcesserTagAttribute(string tag, bool maskMutex) : this(tag, maskMutex, false)
    {
    }

    public ScoreProcesserTagAttribute(string tag, bool maskMutex, bool requireSecondData)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag is required.", nameof(tag));
        }

        Tag = tag;
        MaskMutex = maskMutex;
        RequireSecondData = requireSecondData;
    }

    public string Tag { get; }

    public bool MaskMutex { get; }

    public bool RequireSecondData { get; }
}
