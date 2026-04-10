using LCI.Dem.Categorization.Data.Entity.KeywordScoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Contracts
{
    public interface IKeywordScoringProcess
    {
        Task<IEnumerable<EventMaster>> GetEventMaster(int regularEvent, string sThreadName);
        Task<IEnumerable<int>> GetEventID();
        Task<IEnumerable<DocketKeywordScore>> GetDocketKeywordScore();
        Task<IEnumerable<DocketKeywordScoreComplete>> GetDocketKeywordScoreComplete();
        int ExecuteProcedureCreateDocketKeywordScoreFromEventMaster(int docketEntryID, string sThreadName);
        Task ExecuteProcedureCreateDocketKeywordScoreByLevels(string jobIdNum, int batchID);
        int ExecuteProcedureCreateDocketKeywordScoreCompleteByLevels(List<DocketKeywordScoreComplete> docketKeywordScoreList, string sThreadName, int batchID);
        int RemoveDocketKeywordScore(int docketEntryID, string sThreadName);
        int RemoveDocketEvents(int docketEntryID, string sThreadName);
        Task<IEnumerable<DocketScoringStatus>>  GetDocketScoringStatus();
        Task<IEnumerable<EventKeywordMaster>>  GetEventKeywordMasterLevel3(string sThreadName);
        Task<IEnumerable<EventKeywordMaster>>  GetEventKeywordMasterLevel012(string sThreadName);
        Task<IEnumerable<EventKeywordMaster>>  GetEventKeywordMasterLevel101100(string sThreadName);
        Task<IEnumerable<DocketScoringStatus>> GetDocketEntriesToBeProcessed(string jobIdNum, int inProgressStatus);
        Task<IEnumerable<DocketKeywordScoreCompleteEventKeywordMaster>> GetTimeTakenByKeyword();
        Task<int> ExecuteProcedureCreateDocketKeywordScoreFromEventMaster_New(string jobIdNum);
    }
}
