using System;
using System.Collections.Generic;
using System.Text;

namespace KeywordScoring.Data.Entity
{
    public class DocketEventScore
    {
        public int DocketEntryID { get; set; }
        public int EventID { get; set; }
        public int ScoreAuto { get; set; }
        public int ScoreAutoKeywordID { get; set; }
        public string ScoreAutoComment { get; set; }
        public string ScoreAutoDetails { get; set; }
    }
}
