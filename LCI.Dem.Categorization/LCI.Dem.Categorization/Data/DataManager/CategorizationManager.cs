using Dapper;
using LCI.Dem.Categorization.Constants;
using LCI.Dem.Categorization.Contracts;
using LCI.Dem.Categorization.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.DataManager
{
    public class CategorizationManager : DbFactoryBase, ICategorizationManager
    {
        /// <summary>
        /// <see cref="SSNAutoCheckerManager"/>
        /// </summary>
        private readonly ILogger<CategorizationManager> _logger;

        /// <summary>
        /// 
        /// </summary>
        private const string connectionStringKey = DatabaseConnection.SQLDBConnectionString;

        /// <summary>
        /// <see cref="IConfiguration"/>
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public CategorizationManager(IConfiguration config, ILogger<CategorizationManager> logger) : base(config, connectionStringKey)
        {
            _logger = logger;
            _configuration = config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Config>> GetAllConfigValues()
        {
            _logger.LogInformation($"Get All Config Table Value - Start");

            try
            {
                var sqlQuery = $@"SELECT 
                                    [ID]      
                                    ,[Name]      
                                    ,[Value]  
                                FROM [darp].[Config]";

                var response = await DbConnection.QueryAsync<Config>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Get All Config Table Value - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                _logger.LogInformation($"Get All Config Table Value - Completed");
            }
        }

        public async Task ExecuteStoreProcedure(string storeProcedureName)
        {
            _logger.LogInformation($"Execute Store Procedure - Start " + storeProcedureName);
            try
            {
                var p = new DynamicParameters();
                p.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
                await DbConnection.ExecuteAsync(storeProcedureName, p, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                var result = p.Get<int>("@returnValue");
                _logger.LogInformation($"Execute Store Procedure " + storeProcedureName + " - End " + result);
                
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + storeProcedureName + " " + e.InnerException);
                throw;
            }
        }

        public async Task<IEnumerable<Scoring_Status_Master>> GetAllScoringStatusMaster()
        {
            _logger.LogInformation($"Get All Config Table Value - Start");

            try
            {
                var sqlQuery = $@"SELECT [ID]
                                  ,[Name]
                                  ,[Desc]
                              FROM [darp].[Scoring_Status_Master]";

                var response = await DbConnection.QueryAsync<Scoring_Status_Master>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Get All Config Table Value - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                _logger.LogInformation($"Get All Config Table Value - Completed");
            }
        }

        public async Task<int> GetTriggerId(string job_id)
        {
            try
            {
                var sqlQuery = $@"select Max(ID) from [darpRPT].[Cat_Trigger_Stati] Where Job_Id_Num=@job_id_num";
                var param = new DynamicParameters();
                param.Add("job_id_num",Convert.ToString(job_id).Trim());                
                var response = await DbConnection.QueryAsync<int>(sqlQuery,param);
                return response.FirstOrDefault();
            }
            catch(Exception e)
            {
                _logger.LogInformation($"Get All Config Table Value - Exception Occurred " + e.InnerException);
                return 0;
            }
        }

        public async Task<int> GetBatchId(int triggerId)
        {
            var sqlQuery = $@"SELECT ID FROM [darpRPT].[Cat_Batch_Stati] where Trigger_ID=@triggerId";
            var param = new DynamicParameters();
            param.Add("triggerId", triggerId);
            var response = await DbConnection.QueryAsync<int>(sqlQuery, param);
            return response.FirstOrDefault();
        }

        //public async Task<int> Execute_Procedure_Create_Batch_Stat_Entry(int trigger_ID)
        //{
        //    _logger.LogInformation($"Execute Store Procedure [darpRPT].[Create_Batch_Stat_Entry] - Start ");
        //    try
        //    {
        //        var param = new DynamicParameters();
        //        param.Add("trigger_ID", trigger_ID);
        //        param.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
        //        await DbConnection.ExecuteAsync(StoreProcedure.Create_Batch_Stat_Entry, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
        //        var result = param.Get<int>("@returnValue");
        //        _logger.LogInformation($"Execute Store Procedure [darpRPT].[Create_Batch_Stat_Entry] - End " + result);
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogInformation($"Exception occured in method Execute_Procedure_Create_Batch_Stat_Entry" + e.InnerException);
        //        throw;
        //    }
        //}

        //public async Task<int> Execute_Procedure_Batch_Step_Start(int batch_id, string stateName)
        //{
        //    _logger.LogInformation($"Execute Store Procedure [darpRPT].[Batch_Step_Start]  - Start");
        //    try
        //    {
        //        var param = new DynamicParameters();
        //        param.Add("batch_ID", batch_id);
        //        param.Add("sub_status", stateName);
        //        param.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
        //        await DbConnection.ExecuteAsync(StoreProcedure.Batch_Step_Start, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
        //        var result = param.Get<int>("@returnValue");
        //        _logger.LogInformation($"Execute Store Procedure [darpRPT].[Batch_Step_Start] - End " + result);
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogInformation($"Exception occured " + e.InnerException);
        //        throw;
        //    }
        //}

        public async Task<int> Execute_Procedure_Batch_Step_Complete(int step_Id)
        {
            if (step_Id <= 0)
                return 0;

            _logger.LogInformation($"Execute Store Procedure [darpRPT].[Batch_Step_Complete]  - Start");
            try
            {
                var param = new DynamicParameters();
                param.Add("step_ID", step_Id);
                var result = await DbConnection.ExecuteAsync(StoreProcedure.Batch_Step_Complete, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [darpRPT].[Batch_Step_Complete] - End " + result);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured [darpRPT].[Batch_Step_Complete]" + e.InnerException);
                return 0;
            }
        }

        public async Task Execute_Procedure_Update_Batch_Stat_Entry(int batch_ID, int status, string sub_status)
        {
            _logger.LogInformation($"Execute Store Procedure [darpRPT].[Update_Batch_Stat_Entry]  - Start");
            try
            {
                var param = new DynamicParameters();
                param.Add("batch_ID", batch_ID);
                param.Add("status", status);
                param.Add("sub_status", sub_status);
                param.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
                await DbConnection.ExecuteAsync(StoreProcedure.Update_Batch_Stat_Entry, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                var result = param.Get<int>("@returnValue");
                _logger.LogInformation($"Execute Store Procedure [darpRPT].[Update_Batch_Stat_Entry] - End " + result);                
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured [darpRPT].[Update_Batch_Stat_Entry]" + e.InnerException);
            }
        }

        public async Task Execute_Procedure_Update_Trigger_Stat_Entry(int trigger_ID, int status, string sub_status)
        {
            _logger.LogInformation($"Execute Store Procedure [darpRPT].[Update_Trigger_Stat_Entry]  - Start");
            try
            {
                var param = new DynamicParameters();
                param.Add("trigger_ID", trigger_ID);
                param.Add("status", status);
                param.Add("sub_status", sub_status);
                var result = await DbConnection.ExecuteAsync(StoreProcedure.Update_Trigger_Stat_Entry, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [darpRPT].[Update_Trigger_Stat_Entry] - End " + result);
                //return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);
            }
        }

        public async Task<int> Execute_Procedure_Print(string message, int level)
        {
            _logger.LogInformation($"Execute Store Procedure [darp].[Print]  - Start");
            try
            {
                var param = new DynamicParameters();
                param.Add("message", message);
                param.Add("level", level);
                var result = await DbConnection.ExecuteAsync(StoreProcedure.Print, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [darp].[Print]- End " + result);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);
                throw;
            }
        }


        public async Task ExecuteStagingStoreProcedure(string storeProcedureName, string job_id_num)
        {
            _logger.LogInformation($"Execute Store Procedure - Start " + storeProcedureName);
            try
            {
                var param = new DynamicParameters();
                param.Add("Job_Id_Num", job_id_num);
                var result = await DbConnection.ExecuteAsync(storeProcedureName, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure " + storeProcedureName + " - End " + result);                
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + storeProcedureName + " " + e.InnerException);
            }
        }

        public async Task ExecuteDumpMiscStoreProcedure(string storeProcedureName, string job_id_num)
        {
            _logger.LogInformation($"Execute Store Procedure - Start " + storeProcedureName);
            try
            {
                var p = new DynamicParameters();
                p.Add("execution_result", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
                p.Add("Job_Id_Num", job_id_num,direction:ParameterDirection.Input);
                await DbConnection.ExecuteAsync(storeProcedureName, p, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                var result = p.Get<string>("@execution_result");
                _logger.LogInformation($"Execute Store Procedure " + storeProcedureName + " - End " + result);
                //eturn result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + storeProcedureName + " " + e.InnerException);
            }
        }

        public async Task DemExtractCaseDates(string job_id_num)
        {
            _logger.LogInformation($"Extract Dem Case Dates  - Start");

            try
            {
                var param = new DynamicParameters();
                param.Add("job_id_num", job_id_num);

                await DbConnection.ExecuteAsync(StoreProcedure.USP_DEM_EXTRACT_CASE_DATES, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);
            }
        }


        public async Task ExecuteDateMiningStoreProcedure(string storeProcedureName, string job_id_num)
        {
            _logger.LogInformation($"Execute Store Procedure - Start " + storeProcedureName);
            try
            {
                var param = new DynamicParameters();
                param.Add("Job_Id_Num", job_id_num);
                var result = await DbConnection.ExecuteAsync(storeProcedureName, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure " + storeProcedureName + " - End " + result);
               
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + storeProcedureName + " " + e.InnerException);
            }
        }

        public async Task ExecuteEvenetLevelWiseStoreProcedure(string storeProcedureName, string job_id_num)
        {
            _logger.LogInformation($"Execute Store Procedure - Start " + storeProcedureName);
            try
            {
                var param = new DynamicParameters();
                param.Add("Job_Id_Num", job_id_num);
                var result = await DbConnection.ExecuteAsync(storeProcedureName, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure " + storeProcedureName + " - End " + result);
                
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + storeProcedureName + " " + e.InnerException);
            }
        }

        public async Task ExecuteStoreProcedureWithJobId(string storeProcedureName, string job_id_num)
        {
            _logger.LogInformation($"Execute Store Procedure - Start " + storeProcedureName);
            try
            {
                var param = new DynamicParameters();
                param.Add("Job_Id_Num", job_id_num);
                var result = await DbConnection.ExecuteAsync(storeProcedureName, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure " + storeProcedureName + " - End " + result);               
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + storeProcedureName + " " + e.InnerException);                
            }
        }

        public async Task<IEnumerable<string>> ExecuteStatiStoreProcedure(string storeProcedureName, string job_id_num)
        {
            _logger.LogInformation($"Execute Store Procedure - Start " + storeProcedureName);
            try
            {
                var p = new DynamicParameters();
                p.Add("Job_Id_Num", job_id_num);
                p.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
                var result = await DbConnection.QueryAsync<string>(storeProcedureName, p, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure " + storeProcedureName + " - End " + result.Any());
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + storeProcedureName + " " + e.InnerException);
                return null;
            }
        }

        public async Task<IEnumerable<string>> Execute_Procedure_Create_Batch_Stat_Entry(string trigger_ID)
        {
            _logger.LogInformation($"Execute Store Procedure [darpRPT].[Create_Batch_Stat_Entry] - Start ");
            try
            {
                var param = new DynamicParameters();
                param.Add("trigger_ID", trigger_ID);
                var result = await DbConnection.QueryAsync<string>(StoreProcedure.Create_Batch_Stat_Entry, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [darpRPT].[Create_Batch_Stat_Entry] - End " + result.Any());
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured in method Execute_Procedure_Create_Batch_Stat_Entry" + e.InnerException);
                return null;
            }
        }

        public async Task<IEnumerable<string>> Execute_Procedure_Batch_Step_Start(int batch_id, string stateName)
        {
            _logger.LogInformation($"Execute Store Procedure [darpRPT].[Batch_Step_Start]  - Start");
            try
            {
                var param = new DynamicParameters();
                param.Add("batch_ID", batch_id);
                param.Add("sub_status", stateName);
                var result = await DbConnection.QueryAsync<string>(StoreProcedure.Batch_Step_Start, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [darpRPT].[Batch_Step_Start] - End " + result.Any());                
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);
                return null;
            }
        }

        public async Task CleanAndRerun(string jobId,bool type) 
        {
            _logger.LogInformation($"Execute Store Procedure [dbo].[CleanDEMCategorization] - Start ");
            try
            {
                var param = new DynamicParameters();
                param.Add("Job_Id_Num", jobId);
                param.Add("type", type);
                var result = await DbConnection.QueryAsync<string>(StoreProcedure.CleanAndRerun, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [dbo].[CleanDEMCategorization] - End " + result.Any());
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured in method CleanAndRerun" + e.InnerException);
            }
        }
    }
}
