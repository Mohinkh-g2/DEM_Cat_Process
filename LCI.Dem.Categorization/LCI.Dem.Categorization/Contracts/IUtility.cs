using LCI.Dem.Categorization.Data.Entity.KeywordScoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Contracts
{
    public interface IUtility
    {
        void Execute();
        int Batch_ID { get; set; }
        int DocketEntry_ID { get; set; }
        bool Debug_Enabled { get; set; }
        bool UnMatch_Enabled { get; set; }
        DocketScoringStatus Docket_Scoring_Status { get; set; }
        List<string> Error_List { get; set; }
        List<string> Log_List { get; set; }
    }
}
