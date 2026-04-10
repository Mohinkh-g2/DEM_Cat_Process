using KeywordScoring.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeywordScoring.Contract
{
    public interface IUtility
    {
        //void CreateThread(int batchId, int docketEntryId, bool debugFlag, DocketScoringStatus docketScoringStatus);
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
