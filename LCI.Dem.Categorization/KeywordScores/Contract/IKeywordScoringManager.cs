using System.Threading.Tasks;

namespace KeywordScoring.Contract
{
    public interface IKeywordScoringManager
    {
         Task KeywordScoring(int batchId, string jobIdNum, int inProgressStatus,int maxThreads, int timebetweenThreadCreation, bool debugEnabled, bool unMatchEnabled);
    }
}
