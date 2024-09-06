namespace Test;

internal record ClientInformation
{
    public Guid PlayerId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}