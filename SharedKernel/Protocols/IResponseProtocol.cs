namespace SharedKernel.Protocols;

public interface IResponseProtocol : INotificationProtocol
{
    ErrorType ErrorType { get; set; }
}