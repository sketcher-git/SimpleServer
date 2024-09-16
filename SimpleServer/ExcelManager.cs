using Application.Abstractions.Config;
using Application.Abstractions.Services;
using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SimpleServer;

public sealed partial class ExcelManager : IConfigService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public ExcelManager(IConfiguration configurationm, ILogger logger)
    {
        _configuration = configurationm;
        _logger = logger;
        LoadExcels();
    }

    public bool GetDataTable<T>(out Dictionary<int, T> dataTable)
        where T : IDataEntity
    {
        dataTable = null;

        if (!TryGetValueByKey(_dataEntityStorage, typeof(T).Name, out var dataTableObj))
            return false;

        dataTable = new Dictionary<int, T>();
        var spanDataTableObj = dataTableObj.ToArray().AsSpan();
        int spanLength = spanDataTableObj.Length;
        for (int i = 0; i < spanLength; i++)
        {
            dataTable.Add(spanDataTableObj[i].Key, (T)spanDataTableObj[i].Value);
        }

        return true;
    }

    public T? GetSingleData<T>(int dataId)
        where T : IDataEntity
    {
        if (!TryGetValueByKey(_dataEntityStorage, typeof(T).Name, out var dataTableObj))
            return default(T);

        TryGetValueByKey(dataTableObj, dataId, out var dataObj);
        return (T)dataObj;
    }
}

public sealed partial class ExcelManager
{
    private readonly Dictionary<string, Dictionary<int, IDataEntity>> _dataEntityStorage = new Dictionary<string, Dictionary<int, IDataEntity>>();

    private void LoadExcels()
    {
        TryGetDataTypesMapAndDataPropertiesMap(out var dataTypesMap, out var dataPropertiesMap);

        var path = _configuration.GetValue<string>("ExcelsPath:Path");
        var files = Directory.GetFiles(path, "*.xlsx");
        var spanFile = files.AsSpan();
        int spanFileLength = spanFile.Length;
        for (int index = 0; index < spanFileLength; index++)
        {
            var file = spanFile[index];
            using (var workbook = new XLWorkbook(file))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (!TryGetValueByKey(dataTypesMap, fileName, out var dataType)
                || !TryGetValueByKey(dataPropertiesMap, fileName, out var dataTypeFields))
                    continue;

                var dataTable = new Dictionary<int, IDataEntity>();
                var worksheet = workbook.Worksheet(1);
                var headerRow = worksheet.FirstRowUsed().CellsUsed();
                int count = headerRow.Count();
                dynamic? cellValue = null;
                var rows = worksheet.RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    var dataObj = (IDataEntity?)Activator.CreateInstance(dataType);
                    if (dataObj == null)
                        continue;

                    int id = 0;
                    for (int i = 0; i < count; i++)
                    {
                        var header = headerRow.ElementAt(i).Value.ToString();
                        var cell = row.Cell(i + 1);
                        switch (cell.DataType)
                        {
                            case XLDataType.Number:
                                cellValue = cell.GetValue<int>();
                                if (header == "id")
                                    id = cellValue;
                                break;
                            case XLDataType.Text:
                                cellValue = cell.GetValue<string>();
                                break;
                            case XLDataType.DateTime:
                            default:
                                break;
                        }

                        if (cellValue == null)
                            continue;

                        if (TryGetValueByKey(dataTypeFields, header, out var field))
                            field.SetValue(dataObj, cellValue);
                    }

                    if (id <= 0)
                        continue;

                    dataTable.Add(id, dataObj);
                }

                if (dataTable.Count == 0)
                    continue;

                _dataEntityStorage.Add(fileName, dataTable);
            }
        }
    }

    private bool TryGetValueByKey<TKey, TValue>(Dictionary<TKey, TValue> collectionTable, TKey key, out TValue value)
        where TKey : notnull
    {
        value = default(TValue);
        ref var valueOrNull = ref CollectionsMarshal.GetValueRefOrNullRef(collectionTable, key);
        if (Unsafe.IsNullRef(ref valueOrNull))
            return false;

        value = valueOrNull;
        return true;
    }

    private bool TryGetDataTypesMapAndDataPropertiesMap(out Dictionary<string, Type> dataTypesMap, out Dictionary<string, Dictionary<string, PropertyInfo>> dataPropertiesMap)
    {
        dataTypesMap = new Dictionary<string, Type>();
        dataPropertiesMap = new Dictionary<string, Dictionary<string, PropertyInfo>>();
        var assembly = Application.AssemblyReference.Assembly;
        var interfaceType = typeof(IDataEntity);
        var dataTypes = assembly
                            .GetTypes()
                            .Where(t => t != typeof(IDataEntity) && interfaceType.IsAssignableFrom(t));

        var spanDataType = dataTypes.ToArray().AsSpan();
        int spanDataTypeLength = spanDataType.Length;
        for ( int i = 0; i < spanDataTypeLength; i++)
        {
            var dt = spanDataType[i];
            var fieldsMap = new Dictionary<string, PropertyInfo>();
            var properties = dt.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var spanField = properties.AsSpan();
            int spanPropertyLength = spanField.Length;
            for ( int j = 0; j < spanPropertyLength; j++ )
            {
                fieldsMap.Add(spanField[j].Name, spanField[j]);
            }

            dataPropertiesMap.Add(dt.Name, fieldsMap);
            dataTypesMap.Add(dt.Name, dt);
        }

        return true;
    }
}