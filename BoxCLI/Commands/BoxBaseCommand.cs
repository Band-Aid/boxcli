using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Box.V2;
using Box.V2.Converter;
using Box.V2.Models;
using BoxCLI.BoxHome;
using BoxCLI.BoxHome.BoxHomeFiles;
using BoxCLI.BoxPlatform.Service;
using BoxCLI.BoxPlatform.Utilities;
using BoxCLI.CommandUtilities;
using BoxCLI.CommandUtilities.Globalization;
using CsvHelper;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace BoxCLI.Commands
{
    public abstract class BoxBaseCommand : HelpCommandBaseAsync
    {
        private readonly IBoxHome _boxHome;
        private readonly BoxDefaultSettings _settings;
        private readonly IBoxPlatformServiceBuilder _boxPlatformBuilder;
        private IBoxPlatformServiceBuilder boxPlatformBuilder;
        private IBoxHome boxHome;
        protected readonly LocalizedStringsResource _names;

        public BoxBaseCommand(IBoxPlatformServiceBuilder boxPlatformBuilder, IBoxHome boxHome, LocalizedStringsResource names)
        {
            _boxPlatformBuilder = boxPlatformBuilder;
            _boxHome = boxHome;
            _settings = boxHome.GetBoxHomeSettings();
            _names = names;
        }

        public BoxBaseCommand(IBoxPlatformServiceBuilder boxPlatformBuilder, IBoxHome boxHome)
        {
            this.boxPlatformBuilder = boxPlatformBuilder;
            this.boxHome = boxHome;
        }

        protected virtual string ConstructReportPath(string fileName, string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = _settings.GetBoxReportsFolderPath();
            }
            if (!filePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                filePath = $"{filePath}{Path.DirectorySeparatorChar}";
            }
            return $"{filePath}{fileName}";
        }
        protected virtual string ConstructDownloadsPath(string fileName, string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = _settings.GetBoxDownloadsFolderPath();
            }
            if (!filePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                filePath = $"{filePath}{Path.DirectorySeparatorChar}";
            }
            return $"{filePath}{fileName}";
        }

        protected virtual BoxClient ConfigureBoxClient(string oneCallAsUserId = null, bool returnAdmin = false)
        {
            var Box = _boxPlatformBuilder.Build();
            if (!string.IsNullOrEmpty(oneCallAsUserId) && !returnAdmin)
            {
                return Box.AsUserClient(oneCallAsUserId);
            }
            else if (_settings.GetBoxReportsUseDefaultAsUserSetting() && !returnAdmin)
            {
                return Box.AsUserClient(_settings.GetBoxReportsDefaultAsUserIdSetting());
            }
            else if (_settings.GetBoxReportsUseTempAsUserSetting() && !returnAdmin)
            {
                return Box.AsUserClient(_settings.GetBoxReportsTempAsUserIdSetting());
            }
            else
            {
                return Box.AdminClient();
            }
        }

        protected virtual IBoxCollectionsIterators GetIterators()
        {
            var Box = _boxPlatformBuilder.Build();
            return Box.BoxCollectionsIterators;
        }

        protected virtual void OutputJson<T>(T entity)
        {
            var converter = new BoxJsonConverter();
            Reporter.WriteInformation(converter.Serialize<T>(entity));
        }

        protected virtual bool WriteResultsToReport<T>(T entity, string fileName, string filePath = "", string fileFormat = "")
        {
            filePath = ConstructReportPath(fileName, filePath);
            if (string.IsNullOrEmpty(fileFormat))
            {
                fileFormat = _settings.GetBoxReportsFileFormatSetting();
            }
            filePath = $"{filePath}.{fileFormat}";
            if (fileFormat == _settings.FILE_FORMAT_JSON)
            {
                var converter = new BoxJsonConverter();
                File.WriteAllText(filePath, converter.Serialize<T>(entity));
                return true;
            }
            else if (fileFormat == _settings.FILE_FORMAT_CSV)
            {
                try
                {
                    using (StreamWriter fs = File.CreateText(filePath))
                    using (var csv = new CsvWriter(fs))
                    {
                        csv.WriteRecord(entity);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected virtual bool WriteCollectionResultsToReport<T, M>(BoxCollection<T> entity, string fileName, string filePath = "", string fileFormat = "")
            where T : BoxEntity, new()
        {
            System.Console.WriteLine("Starting writer...");
            filePath = ConstructReportPath(fileName, filePath);
            System.Console.WriteLine($"File path: {filePath}");
            if (string.IsNullOrEmpty(fileFormat))
            {
                System.Console.WriteLine("Finding default file format...");
                fileFormat = _settings.GetBoxReportsFileFormatSetting();
                System.Console.WriteLine($"Default file format: {fileFormat}");
            }
            fileFormat = fileFormat.ToLower();
            filePath = $"{filePath}.{fileFormat}";
            System.Console.WriteLine($"File Format: {fileFormat}");
            if (fileFormat == _settings.FILE_FORMAT_JSON)
            {
                try
                {
                    System.Console.WriteLine("Writing JSON file...");
                    var converter = new BoxJsonConverter();
                    File.WriteAllText(filePath, converter.Serialize<BoxCollection<T>>(entity));
                    return true;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    return false;
                }
            }
            else if (fileFormat == _settings.FILE_FORMAT_CSV)
            {
                try
                {
                    using (StreamWriter fs = File.CreateText(filePath))
                    using (var csv = new CsvWriter(fs))
                    {
                        csv.Configuration.RegisterClassMap(typeof(M));
                        csv.WriteRecords(entity.Entries);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        protected virtual bool WriteMarkerCollectionResultsToReport<T, M>(BoxCollectionMarkerBased<T> entity, string fileName, string filePath = "", string fileFormat = "")
            where T : BoxEntity, new()
        {
            System.Console.WriteLine("Starting writer...");
            filePath = ConstructReportPath(fileName, filePath);
            System.Console.WriteLine($"File path: {filePath}");
            if (string.IsNullOrEmpty(fileFormat))
            {
                System.Console.WriteLine("Finding default file format...");
                fileFormat = _settings.GetBoxReportsFileFormatSetting();
                System.Console.WriteLine($"Default file format: {fileFormat}");
            }
            fileFormat = fileFormat.ToLower();
            filePath = $"{filePath}.{fileFormat}";
            System.Console.WriteLine($"File Format: {fileFormat}");
            if (fileFormat == _settings.FILE_FORMAT_JSON)
            {
                try
                {
                    System.Console.WriteLine("Writing JSON file...");
                    var converter = new BoxJsonConverter();
                    File.WriteAllText(filePath, converter.Serialize<BoxCollectionMarkerBased<T>>(entity));
                    return true;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    return false;
                }
            }
            else if (fileFormat == _settings.FILE_FORMAT_CSV)
            {
                try
                {
                    using (StreamWriter fs = File.CreateText(filePath))
                    using (var csv = new CsvWriter(fs))
                    {
                        csv.Configuration.RegisterClassMap(typeof(M));
                        csv.WriteRecords(entity.Entries);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        private string ProcessFileFormatFromPath(string path)
        {
            var fileFormat = Path.GetExtension(path);
            if (fileFormat.StartsWith("."))
            {
                fileFormat = fileFormat.Substring(1);
            }
            return fileFormat;
        }

        protected virtual List<string> ReadFileForIds(string path)
        {
            var fileFormat = this.ProcessFileFormatFromPath(path);
            var ids = new List<string>();
            if (fileFormat == _settings.FILE_FORMAT_JSON)
            {
                var jsonString = File.ReadAllText(path);
                using (var stringReader = new StringReader(jsonString))
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType == JsonToken.PropertyName
                            && (string)jsonReader.Value == "id")
                        {
                            jsonReader.Read();

                            var serializer = new JsonSerializer();
                            ids.Add(serializer.Deserialize<string>(jsonReader));
                        }
                    }
                }
            }
            else if (fileFormat == _settings.FILE_FORMAT_CSV)
            {
                using (var fs = File.OpenText(path))
                using (var csv = new CsvReader(fs))
                {
                    while (csv.Read())
                    {
                        ids.Add(csv.GetField<string>("Id"));
                    }
                }
            }
            else
            {
                throw new Exception($"File format {fileFormat} is not currently supported.");
            }
            return ids;
        }

        protected virtual List<T> ReadFile<T, M>(string path)
        {
            System.Console.WriteLine("Inside reader...");
            var fileFormat = this.ProcessFileFormatFromPath(path);
            System.Console.WriteLine($"File is {fileFormat}");
            if (fileFormat == _settings.FILE_FORMAT_JSON)
            {
                var jsonString = File.ReadAllText(path);
                var converter = new BoxJsonConverter();
                return converter.Parse<List<T>>(jsonString);
            }
            else if (fileFormat == _settings.FILE_FORMAT_CSV)
            {
                System.Console.WriteLine("Found csv file...");
                using (var fs = File.OpenText(path))
                using (var csv = new CsvReader(fs))
                {
                    System.Console.WriteLine("Processing csv...");
                    csv.Configuration.RegisterClassMap(typeof(M));
                    return csv.GetRecords<T>().ToList();
                }
            }
            else
            {
                throw new Exception("File format {fileFormat} is not currently supported.");
            }
        }

        protected virtual List<string> ProcessFields(string rawFields, List<string> baseFields)
        {
            var fields = new List<string>();
            if (string.IsNullOrEmpty(rawFields))
            {
                fields = baseFields;
            }
            else
            {
                fields = new List<string>(rawFields.Split(','));
            }
            return fields;
        }
    }
}