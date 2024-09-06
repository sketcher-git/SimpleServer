using SharedKernel.Protocols;

namespace Network;

public sealed record RequestInfomation
{
    public Guid PlayerId { get; set; }
    public IRequestProtocol Protocol { get; set; }
}