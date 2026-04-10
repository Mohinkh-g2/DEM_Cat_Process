using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.Entity
{
    public class Scoring_Status_Master
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        public Scoring_Status_Master()
        { }

        public Scoring_Status_Master(int ID_, string Name_, string Desc_)
        {
            this.ID = ID_;
            this.Name = Name_;
            this.Desc = Desc_;
        }
    }
}
