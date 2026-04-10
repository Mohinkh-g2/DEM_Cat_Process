using System;
using System.Collections.Generic;
using System.Text;

namespace KeywordScoring.Data.Entity
{
    public class DocketKeywordScore
    {
        public int DocketEntry_ID { get; set; }
        public int Keyword_ID { get; set; }
        public int Event_ID { get; set; }
        public int Score { get; set; }
    }
}
