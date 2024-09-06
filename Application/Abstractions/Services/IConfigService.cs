using Application.Abstractions.Config;

namespace Application.Abstractions.Services;

public interface IConfigService
{
    public bool GetDataTable<T>(out Dictionary<int, T> dataTable)
        where T : IDataEntity;
    public T? GetSingleData<T>(int dataId)
        where T : IDataEntity;
}