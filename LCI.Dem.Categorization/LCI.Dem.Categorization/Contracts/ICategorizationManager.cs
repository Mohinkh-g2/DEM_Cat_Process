using LCI.Dem.Categorization.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICategorizationManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Config>> GetAllConfigValues();

        Task<IEnumerable<Scoring_Status_Master>> GetAllScoringStatusMaster();
        
        Task ExecuteStoreProcedure(string storeprocedureName);
                       
        //Task<int> Execute_Procedure_Create_Batch_Stat_Entry(int trigger_ID);

        //Task<int> Execute_Procedure_Batch_Step_Start(int batch_id,string stateName);

        Task<int> Execute_Procedure_Batch_Step_Complete(int step_Id);

        Task Execute_Procedure_Update_Batch_Stat_Entry(int batch_ID,int status,string sub_status);

        Task Execute_Procedure_Update_Trigger_Stat_Entry(int trigger_ID,int status,string sub_status);

        Task<int> Execute_Procedure_Print(string message, int level);

        Task ExecuteStagingStoreProcedure(string storeprocedureName,string job_id);

        Task ExecuteDumpMiscStoreProcedure(string storeprocedureName, string job_id);

        Task DemExtractCaseDates(string job_id);

        Task ExecuteDateMiningStoreProcedure(string storeprocedureName, string job_id);


        Task ExecuteEvenetLevelWiseStoreProcedure(string storeprocedureName, string job_id);

        Task ExecuteStoreProcedureWithJobId(string storeprocedureName, string job_id);

        //Task<int> ExecuteStatiStoreProcedure(string storeprocedureName, string job_id);

        Task<int> GetTriggerId(string job_id);

        Task<int> GetBatchId(int triggerId);

        Task<IEnumerable<string>> ExecuteStatiStoreProcedure(string storeprocedureName, string job_id);

        Task<IEnumerable<string>> Execute_Procedure_Create_Batch_Stat_Entry(string trigger_ID);

        Task<IEnumerable<string>> Execute_Procedure_Batch_Step_Start(int batch_id, string stateName);

        Task CleanAndRerun(string jobId, bool type);

    }
}