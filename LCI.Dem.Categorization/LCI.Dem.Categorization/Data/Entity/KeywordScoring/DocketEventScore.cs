using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.Entity.KeywordScoring
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

    public class DocketKeywordScore
    {
        public int DocketEntry_ID { get; set; }
        public int Keyword_ID { get; set; }
        public int Event_ID { get; set; }
        public int Score { get; set; }
    }

    public class DocketKeywordScoreComplete
    {
        public int Batch_ID { get; set; }
        public int DocketEntry_ID { get; set; }
        public int Keyword_ID { get; set; }
        public int Event_ID { get; set; }
        public int Score { get; set; }
        public bool Match { get; set; }
        public int Time_Taken { get; set; }
        public string Thread_Name { get; set; }
    }

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


    public class DocketScoringStatus
    {
        public int DocketEntry_ID { get; set; }
        public DateTime Docket_Created_DT { get; set; }
        public DateTime Created_DT { get; set; }
        public string DocketText_Corrected { get; set; }
        public int Event_Count { get; set; }
        public int DocketText_Correction_Status { get; set; }
        public DateTime DocketText_Correction_DT { get; set; }
        public int Keyword_Scoring_Status { get; set; }
        public DateTime Keyword_Scoring_DT { get; set; }
        public int Event_Scoring_Status { get; set; }
        public DateTime Event_Scoring_DT { get; set; }
        public int Date_Mining_Status { get; set; }
        public DateTime Date_Mining_DT { get; set; }
        public int MISC_Mining_Status { get; set; }
        public DateTime MISC_Mining_DT { get; set; }
        public int POI_Mining_Status { get; set; }
        public DateTime POI_Mining_DT { get; set; }
        public int Categorization_Status { get; set; }
        public DateTime Categorization_DT { get; set; }
        public string Job_Id_Num { get; set; }
        public int BNC_Mining_Status { get; set; }
        public DateTime BNC_Mining_DT { get; set; }
    }

    public class EventKeywordMaster
    {
        public int Keyword_ID { get; set; }
        public int Event_ID { get; set; }
        public int keyword_level { get; set; }
        public string Keyword { get; set; }
        public string RegexString { get; set; }
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
    }


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


    public static class RegexMatch
    {
        public static string RegexCapture(string searchtext, string pattern)
        {
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return r.Match(searchtext).Value.ToString();
        }
    }


}
