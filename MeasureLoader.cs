using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace XmlGenerator
{
    class MeasureLoader
    {
        public List<Measure> Load(string measuresDirectory, string searchPattern)
        {
            var measureRegEx = new Regex("measure_(.*)?_stp");

            var topLevelFolders = new DirectoryInfo(measuresDirectory).GetDirectories();

            var allRecords = new List<Measure>();

            foreach (var folder in topLevelFolders)
            {
                var files = folder.GetFiles(searchPattern);

                foreach (var file in files)
                {
                    var currentMeasure = measureRegEx.Match(file.Name).Groups[1].Value;

                    using (var reader = new StreamReader(file.FullName))

                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<MeasureMap>();
                        var records = csv.GetRecords<Measure>();

                        var recordsWithKey = records.ToList();
                        recordsWithKey.ForEach(x =>
                        {
                            x.Section = folder.Name;
                            x.MeasureName = currentMeasure;
                        });

                        allRecords.AddRange(recordsWithKey);
                    }
                }
            }
            return allRecords;
        }
    }
}
