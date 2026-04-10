using LCI.Dem.Categorization.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Contracts
{
    public interface IProcessCourtManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProcessCourt>> GetProcessCourt();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task UpdateProcessCourtStatus(string job_id_num,string status);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task UpdateProcessCourtBatchId(string job_id_num, int batchId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<int> UpdateProcessFailedCourtStatus(string job_id_num, string msg);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<IEnumerable<Cat_Batch_Step>> getStepDetails(IEnumerable<int> batchId);

        Task<IEnumerable<ProcessCourt>> GetProcessCourtWithFL();
    }
}
