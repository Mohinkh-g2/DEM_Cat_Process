using System;
using System.Collections.Generic;
using System.Text;

namespace KeywordScoring.Data.Entity
{
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
}
