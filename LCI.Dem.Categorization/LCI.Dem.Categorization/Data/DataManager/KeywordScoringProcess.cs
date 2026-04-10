using Dapper;
using LCI.Dem.Categorization.Constants;
using LCI.Dem.Categorization.Contracts;
using LCI.Dem.Categorization.Data.Entity.KeywordScoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.DataManager
{
    public class KeywordScoringProcess : DbFactoryBase, IKeywordScoringProcess
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeywordScoringProcess> _logger;
        private const string _connectionStringKey = DatabaseConnection.SQLDBConnectionString;
        public KeywordScoringProcess(IConfiguration configuration, ILogger<KeywordScoringProcess> logger) : base(configuration, _connectionStringKey)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<IEnumerable<DocketEventScore>> GetDocketEventScore()
        {
            //_logger.LogInformation($"GetDocketEventScore - Start");

            try
            {
                var sqlQuery = $@"SELECT [DocketEntry_ID]
                              ,[Event_ID]
                              ,[Score_Auto]
                              ,[Score_Auto_Keyword_ID]
                              ,[Score_Auto_Comment]
                              ,[Score_Auto_Details]
                          FROM [DEM_Categorization].[darp].[Docket_Event_Score] WITH (NOLOCK)";

                var response =await DbConnection.QueryAsync<DocketEventScore>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetDocketEventScore - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                //_logger.LogInformation($"GetDocketEventScore - Completed");
            }
        }
        public async Task<IEnumerable<DocketKeywordScore>> GetDocketKeywordScore()
        {
            //_logger.LogInformation($"GetDocketKeywordScore - Start");

            try
            {
                var sqlQuery = $@"SELECT [DocketEntry_ID]
                                ,[Keyword_ID]
                                ,[Event_ID]
                                ,[Score]
                              FROM [DEM_Categorization].[darp].[Docket_Keyword_Score] WITH (NOLOCK)";

                var response = await DbConnection.QueryAsync<DocketKeywordScore>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetDocketKeywordScore - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                //_logger.LogInformation($"GetDocketKeywordScore - Completed");
            }
        }
        public async Task<IEnumerable<int>> GetEventID()
        {
            //_logger.LogInformation($"GetDocketKeywordScore - Start");

            try
            {
                var sqlQuery = $@"SELECT Distinct [Event_ID]
                              FROM [DEM_Categorization].[darp].[Docket_Keyword_Score_Level3] WITH (NOLOCK)";

                var response = await DbConnection.QueryAsync<int>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetDocketKeywordScore - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                //_logger.LogInformation($"GetDocketKeywordScore - Completed");
            }
        }
        public async Task<IEnumerable<DocketKeywordScoreComplete>> GetDocketKeywordScoreComplete()
        {
            //_logger.LogInformation($"GetDocketKeywordScoreComplete - Start");

            try
            {
                var sqlQuery = $@"SELECT [DocketEntry_ID]
                                ,[Keyword_ID]
                                ,[Event_ID]
                                ,[Score]
                                ,[Match]
                                ,[Time_Taken]
                              FROM [DEM_Categorization].[darp].[Docket_Keyword_Score_Complete] WITH (NOLOCK)";

                var response = await DbConnection.QueryAsync<DocketKeywordScoreComplete>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetDocketKeywordScoreComplete - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                //_logger.LogInformation($"GetDocketKeywordScoreComplete - Completed");
            }
        }
        public async Task<IEnumerable<DocketScoringStatus>> GetDocketScoringStatus()
        {
            //_logger.LogInformation($"GetDocketScoringStatus - Start");

            try
            {
                var sqlQuery = $@"SELECT [DocketEntry_ID]
                              ,[Docket_Created_DT]
                              ,[Created_DT]
                              ,[DocketText_Corrected]
                              ,[Event_Count]
                              ,[DocketText_Correction_Status]
                              ,[DocketText_Correction_DT]
                              ,[Keyword_Scoring_Status]
                              ,[Keyword_Scoring_DT]
                              ,[Event_Scoring_Status]
                              ,[Event_Scoring_DT]
                              ,[Date_Mining_Status]
                              ,[Date_Mining_DT]
                              ,[MISC_Mining_Status]
                              ,[MISC_Mining_DT]
                              ,[POI_Mining_Status]
                              ,[POI_Mining_DT]
                              ,[Categorization_Status]
                              ,[Categorization_DT]
                              ,[Job_Id_Num]
                              ,[BNC_Mining_Status]
                              ,[BNC_Mining_DT]
                          FROM [DEM_Categorization].[darp].[Docket_Scoring_Status] WITH (NOLOCK)";

                var response = await DbConnection.QueryAsync<DocketScoringStatus>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetDocketScoringStatus - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                //_logger.LogInformation($"GetDocketScoringStatus - Completed");
            }
        }
        public async Task<IEnumerable<EventKeywordMaster>> GetEventKeywordMasterLevel3(string sThreadName)
        {
            _logger.LogInformation($"GetEventKeywordMaster - Start " + sThreadName);

            try
            {
                var sqlQuery = $@"SELECT [Keyword_ID]
                                  ,[Event_ID]
                                  ,[keyword_level]
                                  ,[Keyword]
                                  ,[RegexString]
                                  ,[WordCount]
                                  ,[SentenceCount]
                              FROM [DEM_Categorization].[darp].[Event_Keyword_Master] WITH (NOLOCK) 
                              WHERE [Keyword] IS NOT NULL AND [keyword_level]=3";

                var response = await DbConnection.QueryAsync<EventKeywordMaster>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetEventKeywordMaster - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                _logger.LogInformation($"GetEventKeywordMaster - Completed " + sThreadName);
            }
        }
        public async Task<IEnumerable<EventKeywordMaster>> GetEventKeywordMasterLevel012(string sThreadName)
        {
            _logger.LogInformation($"GetEventKeywordMaster - Start " + sThreadName);

            try
            {
                var sqlQuery = $@"SELECT [Keyword_ID]
                                  ,[Event_ID]
                                  ,[keyword_level]
                                  ,[Keyword]
                                  ,[RegexString]
                                  ,[WordCount]
                                  ,[SentenceCount]
                              FROM [DEM_Categorization].[darp].[Event_Keyword_Master] WITH (NOLOCK) 
                              WHERE [keyword_level] IN (0, 1, 2)
			                    AND [RegexString] IS NOT NULL
			                    AND ltrim(rtrim([RegexString])) <> ''";

                var response =await DbConnection.QueryAsync<EventKeywordMaster>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetEventKeywordMaster - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                _logger.LogInformation($"GetEventKeywordMaster - Completed " + sThreadName);
            }
        }
        public async Task<IEnumerable<EventKeywordMaster>> GetEventKeywordMasterLevel101100(string sThreadName)
        {
            _logger.LogInformation($"GetEventKeywordMasterLevel101100 - Start " + sThreadName);

            try
            {
                var sqlQuery = $@"SELECT [Keyword_ID]
                                  ,[Event_ID]
                                  ,[keyword_level]
                                  ,[Keyword]
                                  ,[RegexString]
                                  ,[WordCount]
                                  ,[SentenceCount]
                              FROM [DEM_Categorization].[darp].[Event_Keyword_Master] WITH (NOLOCK) 
                              WHERE [keyword_level] IN (101, 100)
			                    AND [RegexString] IS NOT NULL
			                    AND ltrim(rtrim([RegexString])) <> ''";

                var response = await DbConnection.QueryAsync<EventKeywordMaster>(sqlQuery);
                _logger.LogInformation($"GetEventKeywordMasterLevel101100 - End " + sThreadName);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetEventKeywordMasterLevel101100 - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                _logger.LogInformation($"GetEventKeywordMasterLevel101100 - Completed " + sThreadName);
            }
        }
        public async Task<IEnumerable<EventMaster>> GetEventMaster(int regularEvent, string sThreadName)
        {
            _logger.LogInformation($"GetEventMaster - Start " + sThreadName);

            try
            {
                var sqlQuery = $@"SELECT [Event_ID]
                              ,[Case_Category]
                              ,[Sub_Category]
                              ,[Status]
                              ,[Category]
                              ,[Event_Name]
                              ,[PRA_Priority]
                              ,[Process_Exists]
                              ,[LCI_Comments]
                              ,[Event_date]
                              ,[Cervello_Date]
                              ,[Event_Description]
                              ,[Manual_Review_Keyword_Flag]
                              ,[Client_code]
                              ,[LCI_Category]
                              ,[Category_ID]
                              ,[Primary_Category]
                              ,[Secondary_Category]
                              ,[Tertiary_Category]
                              ,[WordCount]
                              ,[Event_Group]
                          FROM [DEM_Categorization].[darp].[Event_Master] WITH (NOLOCK) WHERE Event_Group=@regularEvent";
                var param = new DynamicParameters();
                param.Add("regularEvent", regularEvent);
                var response =await DbConnection.QueryAsync<EventMaster>(sqlQuery, param);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetEventMaster - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                _logger.LogInformation($"GetEventMaster - Completed" + sThreadName);
            }
        }
        public int ExecuteProcedureCreateDocketKeywordScoreCompleteByLevels(List<DocketKeywordScoreComplete> docketKeywordScoreList, string sThreadName, int batchID)
        {
            _logger.LogInformation($"Execute Store Procedure [darpRPT].[CreateDocketKeywordScoreCompleteByLevels]  - Start" + sThreadName);
            string strConnectionString = DbConnectionString;

            DataTable dt = ToDataTable<DocketKeywordScoreComplete>(docketKeywordScoreList);
            var result = 0;
            try
            {

                using (SqlConnection connection = new SqlConnection(strConnectionString))
                {
                    connection.Open();
                    SqlCommand sqlCommand = new SqlCommand(StoreProcedure.CreateDocketKeywordScoreCompleteByLevels, connection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter tvpParam = new SqlParameter("@docketKeywordScoreList", SqlDbType.Structured)
                    {
                        TypeName = "[dbo].[DocketKeywordScoreCompleteWithThread]",
                        Value = dt
                    };
                    SqlParameter paramBatchID = new SqlParameter();
                    paramBatchID.ParameterName = "@batchID";
                    paramBatchID.SqlDbType = SqlDbType.Int;
                    paramBatchID.Direction = ParameterDirection.Input;
                    paramBatchID.Value = batchID;

                    SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add(tvpParam);
                    sqlCommand.Parameters.Add(paramBatchID);
                    sqlCommand.CommandTimeout = 30;
                    result = sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                }

                _logger.LogInformation($"Execute Store Procedure [darpRPT].[CreateDocketKeywordScoreCompleteByLevels] - End " + result + " " + sThreadName);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured [darpRPT].[CreateDocketKeywordScoreCompleteByLevels]" + e.InnerException);
                throw;
            }
        }

        public async Task<IEnumerable<DocketScoringStatus>> GetDocketEntriesToBeProcessed(string jobIdNum, int inProgressStatus)
        {
            //_logger.LogInformation($"GetDocketEntriesToBeProcessed - Start : " + DateTime.Now);

            try
            {
                var sqlQuery = $@"SELECT 
                                ROW_NUMBER() OVER (ORDER BY [DocketEntry_ID] ASC) AS RowID, 
                                [DocketEntry_ID],[DocketText_Corrected]
                                FROM [darp].[Docket_Scoring_Status] WITH (NOLOCK) 
                                WHERE DocketText_Corrected IS NOT NULL AND [Job_Id_Num] = @JobIdNum AND [Categorization_Status] = @InProgressStatus";
                var param = new DynamicParameters();
                param.Add("JobIdNum", jobIdNum);
                param.Add("InProgressStatus", inProgressStatus);
                var response = await DbConnection.QueryAsync<DocketScoringStatus>(sqlQuery, param);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetDocketEntriesToBeProcessed - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                //_logger.LogInformation($"GetDocketEntriesToBeProcessed - Completed");
            }

        }

        public async Task ExecuteProcedureCreateDocketKeywordScoreByLevels(string jobIdNum,int batchID)
        {
            _logger.LogInformation($"Execute Store Procedure [darpRPT].[CreateDocketKeywordScoreByLevels]  - Start ");
            try
            {
                var param = new DynamicParameters();
                param.Add("jobIdNum", jobIdNum);
                param.Add("batchID", batchID);
                var result = await DbConnection.ExecuteAsync(StoreProcedure.CreateDocketKeywordScoreByLevels,param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [darpRPT].[CreateDocketKeywordScoreByLevels] - End " + result);
                //return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured [darpRPT].[CreateDocketKeywordScoreByLevels]" + e.InnerException);
                throw;
            }
        }
        public int ExecuteProcedureCreateDocketKeywordScoreFromEventMaster(int docketEntryID, string sThreadName)
        {
            _logger.LogInformation($"Execute Store Procedure [darpRPT].[CreateDocketKeywordScore]  - Start " + sThreadName);
            try
            {
                var param = new DynamicParameters();
                param.Add("docketEntryID", docketEntryID);
                var result = DbConnection.Execute(StoreProcedure.CreateDocketKeywordScore, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [darpRPT].[CreateDocketKeywordScore] - End " + result + " " + sThreadName);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured [darpRPT].[CreateDocketKeywordScore]" + e.InnerException);
                throw;
            }
        }

        public int RemoveDocketEvents(int docketEntryID, string sThreadName)
        {
            _logger.LogInformation($"RemoveDocketEvents - [darp].[DocketEvents] - Start " + sThreadName);

            try
            {
                int clientIDAuto = 1;
                var sqlQuery = $@"DELETE FROM [darp].[DocketEvents]
	                            WHERE [Client_ID] = @clientIDAuto
		                            AND [DocketEntry_ID] = @docketEntryID";
                var param = new DynamicParameters();
                param.Add("clientIDAuto", clientIDAuto);
                param.Add("docketEntryID", docketEntryID);
                var response = DbConnection.Execute(sqlQuery, param);
                _logger.LogInformation($"RemoveDocketEvents - [darp].[DocketEvents] - End " + sThreadName);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"RemoveDocketEvents - Exception occured " + e.InnerException);
                throw;
            }
        }
        public int RemoveDocketKeywordScore(int docketEntryID, string sThreadName)
        {
            _logger.LogInformation($"Delete Docket Keyword Score data - Start " + sThreadName);

            try
            {
                var sqlQuery = $@"DELETE 
	                            FROM [darp].[Docket_Keyword_Score]
	                            WHERE [DocketEntry_ID] = @docketEntryID";
                var param = new DynamicParameters();
                param.Add("docketEntryID", docketEntryID);
                var response = DbConnection.Execute(sqlQuery, param);
                _logger.LogInformation($"Delete Docket Keyword Score data - End " + sThreadName);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured in RemoveDocketKeywordScore " + e.InnerException);
                throw;
            }
        }
        public int UpdateKeywordScoringStatus(int docketEntryID, int completedStatus, string sThreadName)
        {
            _logger.LogInformation($"UpdateDocketKeywordScore - Start " + sThreadName);

            try
            {
                var sqlQuery = $@"UPDATE dss
		                            SET dss.[Keyword_Scoring_Status] = @completedStatus
	                            FROM [darp].[Docket_Scoring_Status] dss
	                            WHERE [DocketEntry_ID] = @docketEntryID";
                var param = new DynamicParameters();
                param.Add("docketEntryID", docketEntryID);
                param.Add("completedStatus", completedStatus);
                var response = DbConnection.Execute(sqlQuery, param);
                _logger.LogInformation($"UpdateDocketKeywordScore - End " + sThreadName);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"UpdateDocketKeywordScore - Exception occured " + e.InnerException);
                throw;
            }
        }

        public async Task<IEnumerable<DocketKeywordScoreCompleteEventKeywordMaster>> GetTimeTakenByKeyword()
        {
            //_logger.LogInformation($"GetDocketEventScore - Start");

            try
            {
                var sqlQuery = $@"SELECT 
                                    AVG(Time_Taken_Tick),
                                    MAX(Time_Taken_Tick),
                                    MIN(Time_Taken_Tick),
                                    k.Keyword 
                                FROM [darp].[Docket_Keyword_Score_Complete] c WITH (NOLOCK)
                                INNER JOIN [darp].[Event_Keyword_Master] k WITH (NOLOCK) ON c.Keyword_ID=k.Keyword_ID
                                GROUP BY k.Keyword";

                var response = await DbConnection.QueryAsync<DocketKeywordScoreCompleteEventKeywordMaster>(sqlQuery);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"GetDocketEventScore - Exception Occurred " + e.InnerException);
                return null;
            }
            finally
            {
                //_logger.LogInformation($"GetDocketEventScore - Completed");
            }
        }
        private DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public async Task<int> ExecuteProcedureCreateDocketKeywordScoreFromEventMaster_New(string jobIdNum)
        {
            _logger.LogInformation($"Execute Store Procedure [darpRPT].[CreateDocketEventScore]  - Start " + jobIdNum);
            try
            {
                var param = new DynamicParameters();
                param.Add("JobIdNum", jobIdNum);
                var result = await DbConnection.ExecuteAsync(StoreProcedure.CreateDocketEventScore, param, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                _logger.LogInformation($"Execute Store Procedure [darpRPT].[CreateDocketEventScore] - End " + result + " " + jobIdNum);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception occured [darpRPT].[CreateDocketEventScore]" + e.InnerException);
                throw;
            }
        }

    }
}
