using SharedKernel;

namespace Network;

internal record ResponseInfomation
{
    public Guid PlayerId { get; set; }
    public short ProtocolId { get; set; }
    public object Protocol { get; set; }
    public ResponseType ResponseType { get; set; }
}