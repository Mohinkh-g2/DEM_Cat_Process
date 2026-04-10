using Dapper;
using LCI.Dem.Categorization.Constants;
using LCI.Dem.Categorization.Contracts;
using LCI.Dem.Categorization.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.DataManager
{
    public class ProcessCourtManager : DbFactoryBase,IProcessCourtManager
    {
        /// <summary>
        /// <see cref="SSNAutoCheckerManager"/>
        /// </summary>
        private readonly ILogger<ProcessCourtManager> _logger;

        /// <summary>
        /// 
        /// </summary>
        private const string connectionStringKey = DatabaseConnection.SQLDBDEM_MonitoringConnectionString;

        /// <summary>
        /// <see cref="IConfiguration"/>
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public ProcessCourtManager(IConfiguration config, ILogger<ProcessCourtManager> logger) : base(config, connectionStringKey)
        {
            _logger = logger;
            _configuration = config;
        }

        public async Task<IEnumerable<ProcessCourt>> GetProcessCourt()
        {
            _logger.LogInformation($"Get Process Court  - Start");

            try
            {

                //var sqlQuery = $@"SELECT [ProcessCourtId]
                //                  ,[ProcessId]
                //                  ,[LCI_Court_Number]
                //                  ,[Job_Id_Num]
                //                  ,[PullerStatus]
                //                  ,[Parser_ID_Num]
                //                  ,[ParserStatus]
                //                  ,[CategorizationUniqueId]
                //                  ,[CategorizationStatus]
                //                  ,[ExportStageOneStatus]
                //                  ,[ExportUniqueId]
                //                  ,[ExportStatus]
                //                  ,[ValidationStatus]
                //                  ,[OverallStatus]
                //                  ,[Error]
                //                  ,[CreatedDate]
                //                  ,[ModifiedDate]
                //                  ,[CreatedBy]
                //                  ,[ModifiedBy]
                //          FROM [DEM_Monitoring].[dbo].[ProcessCourt]";

                //var response = await DbConnection.QueryAsync<ProcessCourt>(sqlQuery);

                //return response;
               

                var result = await DbConnection.QueryAsync<ProcessCourt>(
                    StoreProcedure.USP_Get_Process_Court,
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: 0
                );
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);
                throw;
            }
        }

        public async Task UpdateProcessCourtStatus(string job_id_num, string status)
        {
            _logger.LogInformation($"Update Process Court Status - Start");

            try
            {
                var param = new DynamicParameters();
                param.Add("job_id_num", job_id_num);
                param.Add("CategorizationStatus", status);
                await DbConnection.ExecuteAsync(StoreProcedure.USP_DEM_Monitoring_ProcessCourt_Status, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);               
            }
        }

        public async Task UpdateProcessCourtBatchId(string job_id_num, int batchId) 
        {
            _logger.LogInformation($"Update Process Court Status - Start");

            try
            {
                var sqlQuery = $@"UPDATE [dbo].[ProcessCourt]
                                SET [CategorizationUniqueId] = @CategorizationUniqueId
                                WHERE [Job_Id_Num] = @job_id_num";
                var param = new DynamicParameters();
                param.Add("job_id_num", job_id_num);
                param.Add("CategorizationUniqueId", batchId);
                await DbConnection.ExecuteAsync(sqlQuery, param);                
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);
            }
        }

        public async Task<int> UpdateProcessFailedCourtStatus(string job_id_num, string msg)
        {
            _logger.LogInformation($"Update Process Court Status - Start");

            try
            {
                var sqlQuery = $@"UPDATE [dbo].[ProcessCourt]
                                SET [CategorizationStatus] = @CategorizationStatus,
                                Error=@Error
                                WHERE [Job_Id_Num] = @job_id_num";
                var param = new DynamicParameters();
                param.Add("job_id_num", job_id_num);
                param.Add("CategorizationStatus", "FL");
                param.Add("Error", msg);
                var response = await DbConnection.ExecuteAsync(sqlQuery, param);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);
                throw;
            }
        }

        public async Task<IEnumerable<Cat_Batch_Step>> getStepDetails(IEnumerable<int> batchId)
        {
            _logger.LogInformation($"Get Step Details - Start");

            try
            {
                var sqlQuery = $@"SELECT [ID]
                              ,[Batch_ID]
                              ,[Step_Number]
                              ,[Status]
                              ,[Sub_Status]
                              ,[Create_Time]
                              ,[Modified_Time]
                              ,[Time_Taken]
                          FROM [Dem_Categorization].[darpRPT].[Cat_Batch_Step]
                          WHERE [Batch_ID] IN @batchId";

               

                var response = await DbConnection.QueryAsync<Cat_Batch_Step>(sqlQuery,new { batchId = batchId});

                return response;

            }
            catch (Exception e)
            {
                _logger.LogInformation($"Get Step Details - Exception Occurred " + e.InnerException);
                throw;
            }

        }

        public async Task<IEnumerable<ProcessCourt>> GetProcessCourtWithFL()
        {
            _logger.LogInformation($"Get Process Court  - Start");

            try
            {

                var sqlQuery = $@"SELECT [ProcessCourtId]
                          ,[ProcessId]
                          ,[LCI_Court_Number]
                          ,[Job_Id_Num]
                          ,[PullerStatus]
                          ,[Parser_ID_Num]
                          ,[ParserStatus]
                          ,[CategorizationUniqueId]
                          ,[CategorizationStatus]
                          ,[ExportStageOneStatus]
                          ,[ExportUniqueId]
                          ,[ExportStatus]
                          ,[ValidationStatus]
                          ,[OverallStatus]
                          ,[Error]
                          ,[CreatedDate]
                          ,[ModifiedDate]
                          ,[CreatedBy]
                          ,[ModifiedBy]
                          FROM [DEM_Monitoring].[dbo].[ProcessCourt]
                            WHERE [CategorizationStatus]='FL'";

                var response = await DbConnection.QueryAsync<ProcessCourt>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured " + e.InnerException);
                throw;
            }
        }


        //Task<Cat_Batch_Step> IProcessCourtManager.getStepDetails() => throw new NotImplementedException();
    }
}
