using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.Entity
{
    public class ProcessCourt
    {
        public int ProcessCourtId { get; set; }
        public int ProcessId { get; set; }
        public string LCI_Court_Number { get; set; }
        public string Job_Id_Num { get; set; }
        public string PullerStatus { get; set; }
        public string Parser_ID_Num { get; set; }
        public string ParserStatus { get; set; }
        public string CategorizationUniqueId { get; set; }
        public string CategorizationStatus { get; set; }
        public string ExportUniqueId { get; set; }
        public string ExportStageOneStatus { get; set; }
        public string ExportStatus { get; set; }
        public bool ValidationStatus { get; set; }
        public string OverallStatus { get; set; }
        public string Error { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

    }
}
