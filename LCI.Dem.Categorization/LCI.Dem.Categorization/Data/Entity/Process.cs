using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.Entity
{
    public class Process
    {
        public int ProcessId { get; set; }
        public DateTime ProcessStartDateTime { get; set; }
        public DateTime ProcessEndDateTime { get; set; }
        public DateTime ProcessRangeStartDate { get; set; }
        public DateTime ProcessRangeEndDate { get; set; }
        public string ProcessStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifedBy { get; set; }

        public Process(int ProcessId_, DateTime ProcessStartDateTime_, DateTime ProcessEndDateTime_, DateTime ProcessRangeStartDate_, DateTime ProcessRangeEndDate_, string ProcessStatus_, DateTime CreatedDate_, DateTime ModifiedDate_, string CreatedBy_, string ModifedBy_)
        {
            this.ProcessId = ProcessId_;
            this.ProcessStartDateTime = ProcessStartDateTime_;
            this.ProcessEndDateTime = ProcessEndDateTime_;
            this.ProcessRangeStartDate = ProcessRangeStartDate_;
            this.ProcessRangeEndDate = ProcessRangeEndDate_;
            this.ProcessStatus = ProcessStatus_;
            this.CreatedDate = CreatedDate_;
            this.ModifiedDate = ModifiedDate_;
            this.CreatedBy = CreatedBy_;
            this.ModifedBy = ModifedBy_;
        }
    }
}
