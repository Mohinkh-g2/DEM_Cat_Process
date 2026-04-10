using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.Entity
{
    public class Cat_Batch_Step
    {
        public int ID { get; set; }
        public int Batch_ID { get; set; }
        public int Step_Number { get; set; }
        public int Status { get; set; }
        public string Sub_Status { get; set; }
        public DateTime Create_Time { get; set; }
        public DateTime Modified_Time { get; set; }
        public int Time_Taken { get; set; }

    }
}
