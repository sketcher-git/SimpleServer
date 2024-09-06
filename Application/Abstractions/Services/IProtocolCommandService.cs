using SharedKernel;
using System.Collections.Concurrent;

namespace Application.Abstractions.Services;

public interface IProtocolCommandService
{
    Type? GetCommandByProtocolId(ProtocolId protocolId);
    ConcurrentDictionary<ProtocolId, Type> GetProtocolCommandMap();
}