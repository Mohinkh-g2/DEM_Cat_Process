using System;
using System.Collections.Generic;
using System.Text;

namespace KeywordScoring.Data.Entity
{
    public class DocketEvents
    {
        public int ClientID { get; set; }
        public int DocketEntryID { get; set; }
        public int EventID { get; set; }
        public int ScoreAuto { get; set; }
        public int ScoreAutoKeywordID { get; set; }
        public string ScoreAutoComment { get; set; }
        public string ScoreAutoDetails { get; set; }
        public int WordCount { get; set; }
        public DateTime DateAuto { get; set; }
        public int CountBNCParents { get; set; }
    }
}
