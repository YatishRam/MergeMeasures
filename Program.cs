using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MergeMeasures
{
    class Program
    {
        static string _path = "C:/Users/yram/source/repos/YatishRam/OpenSafely/output/measures";
        static string searchPattern = "*_stp.csv";

        static string _xmlContainer = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Surveys xsi:noNamespaceSchemaLocation=\"COVID_AT-RISK_2021-22_W.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">{0}</Surveys>";
        static string _xmlSubmissionTemplate = "<Svy SurveyName=\"COVID_AT-RISK_2021-22_W_21-01\" OrgCode=\"{0}\" DataSource=\"TPP\">{1}</Svy>";

        static void Main(string[] args)
        {
            var measureRegEx = new Regex("measure_(.*)?_stp");

            var topLevelFolders = new DirectoryInfo(_path).GetDirectories();

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

            IDictionary<string, string> readCodes = default;

            using (var reader = new StreamReader("readcodes.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                readCodes = csv.GetRecords<KeyValue>()
                  .ToDictionary(x => x.Key, y => y.Value);
            }

            var groupedByOrgAndDate = allRecords.GroupBy(x => new { x.Organisation, x.Date });

            var submissions = new StringBuilder();

            foreach (var group in groupedByOrgAndDate)
            {
                var orgRecord = new StringBuilder();
                var groupRecords = group.AsEnumerable();

                foreach (var record in groupRecords)
                {
                    var readCode = readCodes[$"{record.Section}{record.MeasureName}{record.PopulationGroup}"];

                    orgRecord.Append($"<{readCode}>{(int)record.Vaccinated}</{readCode}>");
                }

                // Get total population for different cohorts
                var populationgroups = groupRecords.GroupBy(x => new { x.Section, x.PopulationGroup });

                foreach (var record in populationgroups)
                {
                    var readCode = readCodes[$"{record.Key.Section}population{record.Key.PopulationGroup}"];

                    orgRecord.Append($"<{readCode}>{record.First().Population}</{readCode}>");
                }

                submissions.AppendFormat(_xmlSubmissionTemplate, group.First().Organisation, orgRecord);
            }

            File.WriteAllText("Submission.xml", string.Format(_xmlContainer, submissions));
        }
    }

    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Measure
    {
        public int Population { get; set; }
        public decimal Vaccinated { get; set; }
        public string PopulationGroup { get; set; }
        public string Organisation { get; set; }
        public DateTime Date { get; set; }
        public string Section { get; set; }
        public string MeasureName { get; set; }
    }

    public class MeasureMap : ClassMap<Measure>
    {
        public MeasureMap()
        {
            AutoMap(CultureInfo.InvariantCulture);

            Map(m => m.Organisation).Index(0);
            Map(m => m.PopulationGroup).Index(1);
            Map(m => m.Vaccinated).Index(2);
            Map(m => m.Population).Index(3);
            Map(m => m.Date).Index(5);
            Map(m => m.Section).Ignore();
            Map(m => m.MeasureName).Ignore();
        }
    }
}
