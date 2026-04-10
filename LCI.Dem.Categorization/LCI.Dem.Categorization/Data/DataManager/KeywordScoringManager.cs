using LCI.Dem.Categorization.Constants;
using LCI.Dem.Categorization.Contracts;
using LCI.Dem.Categorization.Data.Entity.KeywordScoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.DataManager
{
    public class KeywordScoringManager : DbFactoryBase, IKeywordScoringManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeywordScoringManager> _logger;
        private const string _connectionStringKey = DatabaseConnection.SQLDBConnectionString;
        private readonly IServiceProvider _services;

        public KeywordScoringManager(IConfiguration configuration, ILogger<KeywordScoringManager> logger, IServiceProvider services) : base(configuration, _connectionStringKey)
        {
            _configuration = configuration;
            _logger = logger;
            _services = services;
        }

        public async Task KeywordScoring(int batchId, string jobIdNum, int inProgressStatus, int maxThreads, int timebetweenThreadCreation, bool debugEnabled, bool unMatchEnabled)
        {
            using (var scope = _services.CreateScope())
            {
                var _keywordScoringProcess = scope.ServiceProvider.GetRequiredService<IKeywordScoringProcess>();

                var dssList = await _keywordScoringProcess.GetDocketEntriesToBeProcessed(jobIdNum, inProgressStatus);

                if (dssList.Any())
                {
                    var listDocketKeywordScore = _keywordScoringProcess.ExecuteProcedureCreateDocketKeywordScoreFromEventMaster_New(jobIdNum);
                    var parallel = ExecuteParallel(dssList, maxThreads, batchId, debugEnabled, unMatchEnabled, jobIdNum);

                    await Task.WhenAll(listDocketKeywordScore, parallel);
                    
                }
                //if (dssList.Any())
                //{
                //    // Step 1: Run SP first (initial setup / cleanup)
                //    await _keywordScoringProcess.ExecuteProcedureCreateDocketKeywordScoreFromEventMaster_New(jobIdNum);

                //    // Step 2: Then run parallel keyword processing
                //    await ExecuteParallel(dssList, maxThreads, batchId, debugEnabled, unMatchEnabled, jobIdNum);
                //}

            }
        }
        private async Task<int> ExecuteParallel(IEnumerable<DocketScoringStatus> dssList, int maxThreads, int batchId, bool debugEnabled, bool unMatchEnabled, string jobIdNum)
        {
            var parallelOption = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxThreads
            };

            Parallel.ForEach(dssList, parallelOption, dss =>
           {
               CreateThread(batchId, dss.DocketEntry_ID, debugEnabled, dss, unMatchEnabled);
           });

            using (var scope = _services.CreateScope())
            {
                var _keywordScoringProcess = scope.ServiceProvider.GetRequiredService<IKeywordScoringProcess>();
                await _keywordScoringProcess.ExecuteProcedureCreateDocketKeywordScoreByLevels(jobIdNum, batchId);
            }
            return 0;

        }



        //private void CreateThread(ref List<Thread> ThreadList, ref List<string> ErrorList, ref List<string> LogList, int batchId, int docketEntryID, bool debugEnabled, DocketScoringStatus docketScoringStatus, bool unMatchEnabled)
        //{
        //    var threadName = "";

        //    try
        //    {
        //        using (var scope = _services.CreateScope())
        //        {
        //            var _utility = scope.ServiceProvider.GetRequiredService<IUtility>();
        //            _utility.Batch_ID = batchId;
        //            _utility.DocketEntry_ID = docketEntryID;
        //            _utility.Debug_Enabled = debugEnabled;
        //            _utility.UnMatch_Enabled = unMatchEnabled;
        //            _utility.Docket_Scoring_Status = docketScoringStatus;
        //            _utility.Error_List = ErrorList;
        //            _utility.Log_List = LogList;
        //            _utility.Execute();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + ": Exception: Thread could not be created or started: " + threadName + ": Error: " + ex.ToString());
        //    }
        //}


        //private async Task CreateThread(int batchId, int docketEntryID, bool debugEnabled, DocketScoringStatus docketScoringStatus, bool unMatchEnabled)
        //{
        //    var threadName = "";

        //    try
        //    {
        //        using (var scope = _services.CreateScope())
        //        {
        //            var _utility = scope.ServiceProvider.GetRequiredService<IUtility>();
        //            _utility.Batch_ID = batchId;
        //            _utility.DocketEntry_ID = docketEntryID;
        //            _utility.Debug_Enabled = debugEnabled;
        //            _utility.UnMatch_Enabled = unMatchEnabled;
        //            _utility.Docket_Scoring_Status = docketScoringStatus;
        //            //_utility.Error_List = ErrorList;
        //            //_utility.Log_List = LogList;
        //            await _utility.Execute();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + ": Exception: Thread could not be created or started: " + threadName + ": Error: " + ex.ToString());
        //    }
        //}

        private void CreateThread(int batchId, int docketEntryID, bool debugEnabled, DocketScoringStatus docketScoringStatus, bool unMatchEnabled)
        {
            var threadName = "";

            try
            {
                using (var scope = _services.CreateScope())
                {
                    var _utility = scope.ServiceProvider.GetRequiredService<IUtility>();
                    _utility.Batch_ID = batchId;
                    _utility.DocketEntry_ID = docketEntryID;
                    _utility.Debug_Enabled = debugEnabled;
                    _utility.UnMatch_Enabled = unMatchEnabled;
                    _utility.Docket_Scoring_Status = docketScoringStatus;
                    //_utility.Error_List = ErrorList;
                    //_utility.Log_List = LogList;
                    _utility.Execute();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + ": Exception: Thread could not be created or started: " + threadName + ": Error: " + ex.ToString());
            }
        }



    }
}
