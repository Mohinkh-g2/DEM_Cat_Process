using System;
using System.Collections.Generic;
using System.Text;

namespace KeywordScoring.Data.Entity
{
    public class EventMaster
    {
        public int Event_ID { get; set; }
        public string Case_Category { get; set; }
        public string Sub_Category { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string Event_Name { get; set; }
        public string PRA_Priority { get; set; }
        public string Process_Exists { get; set; }
        public string LCI_Comments { get; set; }
        public int Event_date { get; set; }
        public int Cervello_Date { get; set; }
        public string Event_Description { get; set; }
        public int Manual_Review_Keyword_Flag { get; set; }
        public string Client_code { get; set; }
        public string LCI_Category { get; set; }
        public int Category_ID { get; set; }
        public int Primary_Category { get; set; }
        public int Secondary_Category { get; set; }
        public int Tertiary_Category { get; set; }
        public int WordCount { get; set; }
        public int Event_Group { get; set; }

    }
}
