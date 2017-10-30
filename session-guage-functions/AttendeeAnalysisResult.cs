using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace SessionSuccessGaugeFunctions
{
    /// <summary>
    /// Our Azure Table Data for the analysis results
    /// </summary>
    public class AttendeeAnalysisResult : TableEntity
    {
        public AttendeeAnalysisResult(string lastName, string firstName)
        {
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        public AttendeeAnalysisResult() { }

        public int FaceCount { get; set; }

        public int Anger { get; set; }

        public int Contempt { get; set; }

        public int Disgust { get; set; }

        public int Fear { get; set; }

        public int Happiness { get; set; }

        public int Neutral { get; set; }

        public int Sadness { get; set; }

        public int Suprise { get; set; }
       
    }
}
