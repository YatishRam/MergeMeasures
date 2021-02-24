using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace XmlGenerator
{
    public class XmlGenerator
    {
        static string _xmlContainer = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Surveys xsi:noNamespaceSchemaLocation=\"{1}\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">{0}</Surveys>";
        static string _xmlSubmissionTemplate = "<Svy SurveyName=\"{2}\" OrgCode=\"{0}\" DataSource=\"OpenSafely\">{1}</Svy>";

        public void Generate(IEnumerable<Measure> allRecords, string readCodesFile, string schemaName, string surveyName)
        {
            IDictionary<string, string> readCodes = default;

            using (var reader = new StreamReader(readCodesFile))
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

                submissions.AppendFormat(_xmlSubmissionTemplate, group.First().Organisation, orgRecord, surveyName);
            }

            File.WriteAllText("Submission.xml", string.Format(_xmlContainer, submissions, schemaName));
        }
    }
}
