using System;
using System.Collections.Generic;
using System.Text;

namespace KeywordScoring.Data.Entity
{
    public class DocketKeywordScoreCompleteEventKeywordMaster
    {
        public int Batch_ID { get; set; }
        public int DocketEntry_ID { get; set; }
        public int Keyword_ID { get; set; }
        public int Event_ID { get; set; }
        public int Score { get; set; }
        public bool Match { get; set; }
        public int Time_Taken { get; set; }
        public string Thread_Name { get; set; }
        public int keyword_level { get; set; }
        public string Keyword { get; set; }
        public string RegexString { get; set; }
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
    }
}
