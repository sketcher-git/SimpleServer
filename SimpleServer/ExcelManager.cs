using Application.Abstractions.Config;
using Application.Abstractions.Services;
using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;

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
        if (!_dataEntityStorage.TryGetValue(typeof(T).Name, out var dataTableObj))
            return false;

        dataTable = new Dictionary<int, T>();
        foreach (var data in dataTableObj)
        {
            dataTable.Add(data.Key, (T)data.Value);
        }

        return true;
    }

    public T? GetSingleData<T>(int dataId)
        where T : IDataEntity
    {
        if (!_dataEntityStorage.TryGetValue(typeof(T).Name, out var dataTableObj))
            return default(T);

        if (!dataTableObj.TryGetValue(dataId, out var dataObj))
            return default(T);

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
        foreach (var file in files)
        {
            using (var workbook = new XLWorkbook(file))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (!dataTypesMap.TryGetValue(fileName, out var dataType)
                    || !dataPropertiesMap.TryGetValue(fileName, out var dataTypeFields))
                    continue;

                var dataTable = new Dictionary<int, IDataEntity>();
                var worksheet = workbook.Worksheet(1);
                var headerRow = worksheet.FirstRowUsed().CellsUsed();
                int count = headerRow.Count();
                dynamic? cellValue = null;
                foreach (var row in worksheet.RowsUsed().Skip(1))
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

                        if (dataTypeFields.TryGetValue(header, out var field))
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

    private bool TryGetDataTypesMapAndDataPropertiesMap(out Dictionary<string, Type> dataTypesMap, out Dictionary<string, Dictionary<string, PropertyInfo>> dataPropertiesMap)
    {
        dataTypesMap = new Dictionary<string, Type>();
        dataPropertiesMap = new Dictionary<string, Dictionary<string, PropertyInfo>>();
        var assembly = Application.AssemblyReference.Assembly;
        var interfaceType = typeof(IDataEntity);
        var dataTypes = assembly
                                   .GetTypes()
                                   .Where(t => interfaceType.IsAssignableFrom(t));

        foreach (var dt in dataTypes)
        {
            var fieldsMap = new Dictionary<string, PropertyInfo>();
            foreach (var f in dt.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                fieldsMap.Add(f.Name, f);
            }

            dataPropertiesMap.Add(dt.Name, fieldsMap);
            dataTypesMap.Add(dt.Name, dt);
        }

        return true;
    }
}