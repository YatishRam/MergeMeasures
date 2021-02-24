using CsvHelper.Configuration;
using System;
using System.Globalization;

namespace XmlGenerator
{
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
