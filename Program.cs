using System.CommandLine;
using System.CommandLine.Invocation;

namespace XmlGenerator
{
    class Program
    {
        //static string _path = "C:/Users/yram/source/repos/YatishRam/OpenSafely-covid-vaccine-uptake-riskgroup/output/measures";
        //static string searchPattern = "*_stp.csv";
        //static string readcodes = "riskgroup_readcodes.csv";
        //static string schemaName = "COVID_AT-RISK_2021-22_W.xsd";
        //static string surveyName = "COVID_AT-RISK_2021-22_W_21-01";

        //static string _path = "C:/Users/yram/source/repos/YatishRam/OpenSafely-covid-vaccine-uptake-ethnicity/output/measures";
        //static string searchPattern = "*_stp.csv";
        //static string readcodes = "ethnicity_readcodes.csv";
        //static string schemaName = "COVID_ETHNICITY_GENDER_2021-22_W.xsd";
        //static string surveyName = "COVID_ETHNICITY_GENDER_2021-22_W_21-01";

        static int Main(string[] args)
        {

            var cmd = new RootCommand
            {
                new Argument<string>("measuresDirectory", "Directory containing all measures in sub directories. Each sub directory corresponds to a section in dataset."),
                new Argument<string>("searchPattern", "Search pattern to filter out other files."),
                new Argument<string>("readCodesCsv", "Csv file containing mapping of measures with readcodess."),
                new Argument<string>("schemaName", "Schema name to be inserted in the submission xml file."),
                new Argument<string>("surveyName", "Survey name to be inserted in the submission xml file.")
            };

            cmd.Handler = CommandHandler.Create<string, string, string, string, string, IConsole>(HandleCommand);

            return cmd.Invoke(args);
        }

        static void HandleCommand(string measuresDirectory, string searchPattern, string readCodesCsv, string schemaName, string surveyName, IConsole console)
        {
            var allRecords = new MeasureLoader().Load(measuresDirectory, searchPattern);
            new XmlGenerator().Generate(allRecords, readCodesCsv, schemaName, surveyName);

            System.Console.WriteLine("Submission xml generated..");
            System.Console.ReadKey();
        }
    }
}
