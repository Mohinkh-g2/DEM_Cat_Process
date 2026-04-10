using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.Entity
{
    public class Config
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public Config(int ID, string Name, string Value)
        {
            this.ID = ID;
            this.Name = Name ?? throw new ArgumentNullException(nameof(Name));
            this.Value = Value ?? throw new ArgumentNullException(nameof(Value));
        }

    }
}
