using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Contracts
{
    public interface IKeywordScoringManager
    {
        Task KeywordScoring(int batchId, string jobIdNum, int inProgressStatus, int maxThreads, int timebetweenThreadCreation, bool debugEnabled, bool unMatchEnabled);
    }
}
