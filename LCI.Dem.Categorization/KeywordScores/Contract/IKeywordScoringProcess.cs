using KeywordScoring.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeywordScoring.Contract
{
    public interface IKeywordScoringProcess
    {
        IEnumerable<EventMaster> GetEventMaster(int regularEvent,string sThreadName);
        IEnumerable<int> GetEventID();
        IEnumerable<DocketKeywordScore> GetDocketKeywordScore();
        IEnumerable<DocketKeywordScoreComplete> GetDocketKeywordScoreComplete();
        int ExecuteProcedureCreateDocketKeywordScoreFromEventMaster(int docketEntryID,string sThreadName);
        int ExecuteProcedureCreateDocketKeywordScoreByLevels(string jobIdNum,int batchID);
        int ExecuteProcedureCreateDocketKeywordScoreCompleteByLevels(List<DocketKeywordScoreComplete> docketKeywordScoreList, string sThreadName,int batchID);
        int RemoveDocketKeywordScore(int docketEntryID, string sThreadName);
        int RemoveDocketEvents(int docketEntryID, string sThreadName);
        IEnumerable<DocketScoringStatus> GetDocketScoringStatus();
        IEnumerable<EventKeywordMaster> GetEventKeywordMasterLevel3(string sThreadName);
        IEnumerable<EventKeywordMaster> GetEventKeywordMasterLevel012(string sThreadName);
        IEnumerable<EventKeywordMaster> GetEventKeywordMasterLevel101100(string sThreadName);
        IEnumerable<DocketScoringStatus> GetDocketEntriesToBeProcessed(string jobIdNum, int inProgressStatus);
        IEnumerable<DocketKeywordScoreCompleteEventKeywordMaster> GetTimeTakenByKeyword();

        Task<int> ExecuteProcedureCreateDocketKeywordScoreFromEventMaster_New(string jobIdNum);
    }
}
