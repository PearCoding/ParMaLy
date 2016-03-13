using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace PML.Output
{
    using Statistics;
    public static class CSV
    {
        public static void PrintProceedings(TextWriter writer, IEnumerable<BTStatistics.ProcessEntry> proceedings, Style.CSVStyle style)
        {
            foreach(var proc in proceedings)
            {
                writer.WriteLine(proc.Job.ID + style.Seperator
                    + proc.TotalStages + style.Seperator
                    + proc.QueueJobs + style.Seperator
                    + proc.TimeElapsed);
            }

            writer.Flush();
        }
    }
}
