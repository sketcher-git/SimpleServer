namespace Test;

internal sealed record MessageObject
{
    public int PlayerIndex { get; set; }
    public byte[] Message {  get; set; }
}