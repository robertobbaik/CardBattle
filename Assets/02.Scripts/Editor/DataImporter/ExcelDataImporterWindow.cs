using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

public class ExcelDataImporterWindow : EditorWindow
{
    private const string GeneratedClassPath = "Assets/02.Scripts/Data/Generated/GlobalData.cs";
    private const string GeneratedJsonDirectory = "Assets/Resources/Data";

    private string _excelPath;

    [MenuItem("Tools/CardBattle/Import Excel Data")]
    private static void Open()
    {
        GetWindow<ExcelDataImporterWindow>("Excel Data Importer");
    }

    private void OnEnable()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        _excelPath = Path.Combine(projectRoot, "Data/CardBattle_RuntimeCardData_IntCardType.xlsx");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Excel Source", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            _excelPath = EditorGUILayout.TextField(_excelPath);

            if (GUILayout.Button("Browse", GUILayout.Width(80f)))
            {
                string selectedPath = EditorUtility.OpenFilePanel("Select Excel File", Path.GetDirectoryName(_excelPath), "xlsx,xls");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _excelPath = selectedPath;
                }
            }
        }

        EditorGUILayout.Space(8f);

        if (GUILayout.Button("Import Excel Data"))
        {
            Import();
        }
    }

    private void Import()
    {
        if (!File.Exists(_excelPath))
        {
            EditorUtility.DisplayDialog("Import Failed", $"Excel file does not exist.\n{_excelPath}", "OK");
            return;
        }

        Type factoryType = Type.GetType("ExcelDataReader.ExcelReaderFactory, ExcelDataReader");
        if (factoryType == null)
        {
            EditorUtility.DisplayDialog(
                "Import Failed",
                "ExcelDataReader is not loaded. Open Unity and let NuGetForUnity restore ExcelDataReader from Assets/packages.config, then try again.",
                "OK");
            return;
        }

        List<SheetData> sheets = ReadSheets(factoryType);
        if (sheets.Count == 0)
        {
            EditorUtility.DisplayDialog("Import Failed", "No valid data sheets were found. Data sheets need field names in row 1 and C# types in row 2.", "OK");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(GeneratedClassPath));
        Directory.CreateDirectory(GeneratedJsonDirectory);

        File.WriteAllText(GeneratedClassPath, GenerateGlobalData(sheets), Encoding.UTF8);

        foreach (SheetData sheet in sheets)
        {
            string jsonPath = Path.Combine(GeneratedJsonDirectory, $"{SanitizeFileName(sheet.SheetName)}.json");
            File.WriteAllText(jsonPath, GenerateJson(sheet), Encoding.UTF8);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Import Complete", $"Generated {sheets.Count} data sheet(s).\nClasses: {GeneratedClassPath}\nJSON: {GeneratedJsonDirectory}", "OK");
    }

    private List<SheetData> ReadSheets(Type factoryType)
    {
        List<SheetData> sheets = new List<SheetData>();
        MethodInfo createReader = FindCreateReader(factoryType);
        if (createReader == null)
        {
            Debug.LogError("ExcelDataReader.ExcelReaderFactory.CreateReader(Stream) was not found.");
            return sheets;
        }

        using FileStream stream = File.Open(_excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using IDisposable reader = (IDisposable)createReader.Invoke(null, new object[] { stream });

        Type readerType = reader.GetType();
        MethodInfo read = readerType.GetMethod("Read");
        MethodInfo nextResult = readerType.GetMethod("NextResult");
        MethodInfo getValue = readerType.GetMethod("GetValue");
        PropertyInfo fieldCountProperty = readerType.GetProperty("FieldCount");
        PropertyInfo nameProperty = readerType.GetProperty("Name");

        do
        {
            string sheetName = (string)nameProperty.GetValue(reader);
            List<List<object>> rows = new List<List<object>>();

            while ((bool)read.Invoke(reader, null))
            {
                int fieldCount = (int)fieldCountProperty.GetValue(reader);
                List<object> row = new List<object>(fieldCount);

                for (int i = 0; i < fieldCount; i++)
                {
                    row.Add(getValue.Invoke(reader, new object[] { i }));
                }

                rows.Add(TrimTrailingEmpty(row));
            }

            SheetData sheetData = CreateSheetData(sheetName, rows);
            if (sheetData != null)
            {
                sheets.Add(sheetData);
            }
        }
        while ((bool)nextResult.Invoke(reader, null));

        return sheets;
    }

    private static SheetData CreateSheetData(string sheetName, List<List<object>> rows)
    {
        if (rows.Count < 3 || rows[0].Count == 0 || rows[1].Count == 0)
        {
            return null;
        }

        List<string> fields = ToStringRow(rows[0]);
        List<string> types = ToStringRow(rows[1]);
        if (fields.Count != types.Count)
        {
            return null;
        }

        foreach (string type in types)
        {
            if (!IsSupportedType(type))
            {
                return null;
            }
        }

        return new SheetData(sheetName, fields, types, rows, CreateFieldGroups(fields, types));
    }

    private static MethodInfo FindCreateReader(Type factoryType)
    {
        foreach (MethodInfo method in factoryType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (method.Name == "CreateReader" && parameters.Length == 1 && parameters[0].ParameterType == typeof(Stream))
            {
                return method;
            }
        }

        return null;
    }

    private static List<object> TrimTrailingEmpty(List<object> row)
    {
        for (int i = row.Count - 1; i >= 0; i--)
        {
            if (row[i] != null && !string.IsNullOrWhiteSpace(row[i].ToString()))
            {
                return row.GetRange(0, i + 1);
            }
        }

        return new List<object>();
    }

    private static List<string> ToStringRow(List<object> row)
    {
        List<string> values = new List<string>(row.Count);
        foreach (object value in row)
        {
            values.Add(value?.ToString() ?? string.Empty);
        }

        return values;
    }

    private static string GenerateGlobalData(List<SheetData> sheets)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();

        foreach (SheetData sheet in sheets)
        {
            string className = SanitizeIdentifier(sheet.SheetName);
            string keyFieldName = GetKeyFieldName(sheet);

            builder.AppendLine("[Serializable]");
            builder.AppendLine($"public class {className}");
            builder.AppendLine("{");

            foreach (FieldGroup fieldGroup in sheet.FieldGroups)
            {
                string fieldType = MapType(fieldGroup.Type);
                if (fieldGroup.IsList)
                {
                    fieldType = $"List<{fieldType}>";
                }

                builder.AppendLine($"    public {fieldType} {fieldGroup.Name};");
            }

            builder.AppendLine();
            builder.AppendLine("    [Serializable]");
            builder.AppendLine("    public class Table");
            builder.AppendLine("    {");
            builder.AppendLine($"        public List<{className}> items = new List<{className}>();");
            builder.AppendLine();
            builder.AppendLine($"        public Dictionary<int, {className}> ToDictionary()");
            builder.AppendLine("        {");
            builder.AppendLine($"            Dictionary<int, {className}> dataById = new Dictionary<int, {className}>(items.Count);");
            builder.AppendLine();
            builder.AppendLine($"            foreach ({className} data in items)");
            builder.AppendLine("            {");
            builder.AppendLine($"                dataById.Add(data.{keyFieldName}, data);");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine("            return dataById;");
            builder.AppendLine("        }");
            builder.AppendLine("    }");

            if (className == "CardTextData")
            {
                builder.AppendLine();
                builder.AppendLine("    public string GetName(string languageCode)");
                builder.AppendLine("    {");
                builder.AppendLine("        return NormalizeLanguageCode(languageCode) == \"ko\" ? koreanName : englishName;");
                builder.AppendLine("    }");
                builder.AppendLine();
                builder.AppendLine("    public string GetDescription(string languageCode)");
                builder.AppendLine("    {");
                builder.AppendLine("        return NormalizeLanguageCode(languageCode) == \"ko\" ? koreanDescription : englishDescription;");
                builder.AppendLine("    }");
                builder.AppendLine();
                builder.AppendLine("    private static string NormalizeLanguageCode(string languageCode)");
                builder.AppendLine("    {");
                builder.AppendLine("        return string.Equals(languageCode, \"ko\", StringComparison.OrdinalIgnoreCase) ? \"ko\" : \"en\";");
                builder.AppendLine("    }");
            }

            builder.AppendLine("}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string GetKeyFieldName(SheetData sheet)
    {
        foreach (FieldGroup fieldGroup in sheet.FieldGroups)
        {
            if (!fieldGroup.IsList && MapType(fieldGroup.Type) == "int" && fieldGroup.Name.EndsWith("Id", StringComparison.Ordinal))
            {
                return fieldGroup.Name;
            }
        }

        return sheet.FieldGroups[0].Name;
    }

    private static string GenerateJson(SheetData sheet)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"items\": [");

        for (int rowIndex = 2; rowIndex < sheet.Rows.Count; rowIndex++)
        {
            List<object> row = sheet.Rows[rowIndex];
            builder.AppendLine("    {");

            for (int fieldIndex = 0; fieldIndex < sheet.FieldGroups.Count; fieldIndex++)
            {
                FieldGroup fieldGroup = sheet.FieldGroups[fieldIndex];
                string comma = fieldIndex == sheet.FieldGroups.Count - 1 ? string.Empty : ",";

                if (fieldGroup.IsList)
                {
                    builder.AppendLine($"      \"{fieldGroup.Name}\": {FormatJsonList(row, fieldGroup)}{comma}");
                    continue;
                }

                int columnIndex = fieldGroup.ColumnIndexes[0];
                object value = columnIndex < row.Count ? row[columnIndex] : null;
                builder.AppendLine($"      \"{fieldGroup.Name}\": {FormatJsonValue(value, fieldGroup.Type)}{comma}");
            }

            string rowComma = rowIndex == sheet.Rows.Count - 1 ? string.Empty : ",";
            builder.AppendLine($"    }}{rowComma}");
        }

        builder.AppendLine("  ]");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static List<FieldGroup> CreateFieldGroups(List<string> fields, List<string> types)
    {
        List<FieldGroup> fieldGroups = new List<FieldGroup>();

        for (int i = 0; i < fields.Count; i++)
        {
            string fieldName = SanitizeFieldName(fields[i]);
            FieldGroup fieldGroup = FindFieldGroup(fieldGroups, fieldName);

            if (fieldGroup == null)
            {
                fieldGroups.Add(new FieldGroup(fieldName, types[i], i));
                continue;
            }

            fieldGroup.AddColumn(types[i], i);
        }

        return fieldGroups;
    }

    private static FieldGroup FindFieldGroup(List<FieldGroup> fieldGroups, string fieldName)
    {
        foreach (FieldGroup fieldGroup in fieldGroups)
        {
            if (fieldGroup.Name == fieldName)
            {
                return fieldGroup;
            }
        }

        return null;
    }

    private static bool IsSupportedType(string excelType)
    {
        return excelType.Trim().ToLowerInvariant() is "int" or "float" or "bool" or "string";
    }

    private static string MapType(string excelType)
    {
        return excelType.Trim().ToLowerInvariant() switch
        {
            "int" => "int",
            "float" => "float",
            "bool" => "bool",
            "string" => "string",
            _ => "string"
        };
    }

    private static string FormatJsonValue(object value, string excelType)
    {
        string type = MapType(excelType);
        string text = value?.ToString() ?? string.Empty;

        return type switch
        {
            "int" => string.IsNullOrWhiteSpace(text) ? "0" : Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            "float" => string.IsNullOrWhiteSpace(text) ? "0" : Convert.ToSingle(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            "bool" => string.IsNullOrWhiteSpace(text) ? "false" : Convert.ToBoolean(value, CultureInfo.InvariantCulture).ToString().ToLowerInvariant(),
            _ => $"\"{EscapeJson(text)}\""
        };
    }

    private static string FormatJsonList(List<object> row, FieldGroup fieldGroup)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("[");

        for (int i = 0; i < fieldGroup.ColumnIndexes.Count; i++)
        {
            int columnIndex = fieldGroup.ColumnIndexes[i];
            object value = columnIndex < row.Count ? row[columnIndex] : null;

            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(FormatJsonValue(value, fieldGroup.Type));
        }

        builder.Append("]");
        return builder.ToString();
    }

    private static string SanitizeIdentifier(string value)
    {
        StringBuilder builder = new StringBuilder();

        foreach (char character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) || character == '_' ? character : '_');
        }

        if (builder.Length == 0 || char.IsDigit(builder[0]))
        {
            builder.Insert(0, '_');
        }

        return builder.ToString();
    }

    private static string SanitizeFieldName(string value)
    {
        string identifier = SanitizeIdentifier(value);
        if (string.IsNullOrEmpty(identifier))
        {
            return identifier;
        }

        return char.ToLowerInvariant(identifier[0]) + identifier.Substring(1);
    }

    private static string SanitizeFileName(string value)
    {
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidCharacter, '_');
        }

        return value;
    }

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private class SheetData
    {
        public SheetData(string sheetName, List<string> fields, List<string> types, List<List<object>> rows, List<FieldGroup> fieldGroups)
        {
            SheetName = sheetName;
            Fields = fields;
            Types = types;
            Rows = rows;
            FieldGroups = fieldGroups;
        }

        public string SheetName { get; }
        public List<string> Fields { get; }
        public List<string> Types { get; }
        public List<List<object>> Rows { get; }
        public List<FieldGroup> FieldGroups { get; }
    }

    private class FieldGroup
    {
        public FieldGroup(string name, string type, int columnIndex)
        {
            Name = name;
            Type = type;
            ColumnIndexes.Add(columnIndex);
        }

        public string Name { get; }
        public string Type { get; }
        public List<int> ColumnIndexes { get; } = new List<int>();
        public bool IsList => ColumnIndexes.Count > 1;

        public void AddColumn(string type, int columnIndex)
        {
            ColumnIndexes.Add(columnIndex);
        }
    }
}
