using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using KeywordScoring.Constants;
using KeywordScoring.Contract;
using KeywordScoring.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

namespace KeywordScoring.Data.DataManager
{
    public class KeywordScoringManager : DbFactoryBase, IKeywordScoringManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeywordScoringManager> _logger;
        private readonly ILogger<Utility> _utillogger;
        private const string _connectionStringKey = DatabaseConnection.SQLDBConnectionString;
        private readonly IKeywordScoringProcess _keywordScoringProcess;
        private readonly IUtility _utility;

        public KeywordScoringManager(IConfiguration configuration, ILogger<KeywordScoringManager> logger, IKeywordScoringProcess keywordScoringProcess, IUtility utility, ILogger<Utility> utillogger) : base(configuration, _connectionStringKey)
        {
            _logger = logger;
            _utillogger = utillogger;
            _configuration = configuration;
            _keywordScoringProcess = keywordScoringProcess;
            _utility = utility;
        }
        public async Task KeywordScoring(int batchId, string jobIdNum, int inProgressStatus, int maxThreads, int timebetweenThreadCreation, bool debugEnabled, bool unMatchEnabled)
        {
            var startDateTime = DateTime.Now;
            IEnumerable<DocketScoringStatus> dssList = null;
            List<Thread> ThreadList = new List<Thread>();
            List<string> ErrorList = new List<string>();
            List<string> LogList = new List<string>();
            StringCollection stoppedThreads = new StringCollection();

            _logger.LogInformation($"------------------------------------------------------------------");
            _logger.LogInformation($"");
            _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + " : START Multi - Threaded Keyword Scoring Batch");
            _logger.LogInformation($"Requested Job ID : " + jobIdNum.ToString());


            dssList = _keywordScoringProcess.GetDocketEntriesToBeProcessed(jobIdNum, inProgressStatus);

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = maxThreads
                //MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            if (dssList != null)
            {
                var docketKeywordScoreFromEventMaster = await _keywordScoringProcess.ExecuteProcedureCreateDocketKeywordScoreFromEventMaster_New(jobIdNum);

                Parallel.ForEach(dssList, options, dss =>
                {
                    _logger.LogInformation($"Start thread = {Thread.CurrentThread.ManagedThreadId} - " + dss.DocketEntry_ID);
                    CreateThread(ref ThreadList, ref ErrorList, ref LogList, batchId, dss.DocketEntry_ID, debugEnabled, dss, unMatchEnabled);
                    _logger.LogInformation($"Completed thread = {Thread.CurrentThread.ManagedThreadId} - " + dss.DocketEntry_ID);
                });


                _keywordScoringProcess.ExecuteProcedureCreateDocketKeywordScoreByLevels(jobIdNum,batchId);
                
                _logger.LogInformation($"Complete : Keyword scoring for jobIdNum : " + jobIdNum + " - " + batchId + DateTime.Now);
            }

            while (RunningThreads(ref ThreadList, ref stoppedThreads) > 0)
            {
                _logger.LogInformation($"Waiting for all Threads to finish");
                Thread.Sleep(1000);
            }

            _logger.LogInformation($"All Threads have Completed.");
            _logger.LogInformation($"Log List Count: " + LogList.Count.ToString());
            _logger.LogInformation($"Error List Count: " + ErrorList.Count.ToString());


            if (debugEnabled)
            {
                EnumerateLogs(ref LogList);
            }

            EnumerateExceptions(ref ErrorList);


            _logger.LogInformation($"------------------------------------------------------------------");
            _logger.LogInformation($"");

            DateTime endDateTime = DateTime.Now;
            TimeSpan diff = endDateTime - startDateTime;

            _logger.LogInformation($"Batch Start Time: " + startDateTime + " " + batchId);
            _logger.LogInformation($"Batch End Time: " + endDateTime + " " + batchId);
            _logger.LogInformation($"Total Elapsed Time (seconds) for the batch: " + diff.TotalSeconds + " " + batchId);

            _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + " : END Multi-Threaded Keyword Scoring Batch " + batchId);
            _logger.LogInformation($"------------------------------------------------------------------");
            _logger.LogInformation($"");
        }

        private void EnumerateExceptions(ref List<string> Errors)
        {
            try
            {
                if (Errors.Count > 0)
                {
                    foreach (string err in Errors)
                    {
                        _logger.LogInformation(err.ToString());
                    }

                    throw new Exception("Error Occurred in DEM Categorization in one or more threads.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: Error enumerating all Thread Exceptions: " + ex.ToString());
            }
        }

        private void EnumerateLogs(ref List<string> Logs)
        {
            try
            {
                if (Logs.Count > 0)
                {
                    foreach (string log in Logs)
                    {
                        _logger.LogInformation(log.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: Error enumerating all logs: " + ex.ToString());
            }

        }

        private int RunningThreads(ref List<Thread> allThreads, ref StringCollection stoppedThreads)
        {
            int iRunningCount = 0;

            foreach (var t in allThreads)
            {
                if (t.IsAlive)
                {
                    iRunningCount += 1;
                }
                
            }

            _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + ": Number of Active Threads = " + iRunningCount.ToString());

            return iRunningCount;
        }
        
        private void CreateThread(ref List<Thread> ThreadList, ref List<string> ErrorList, ref List<string> LogList, int batchId, int docketEntryID, bool debugEnabled, DocketScoringStatus docketScoringStatus,bool unMatchEnabled)
        {
            var threadName = "";
            try
            {
                if (debugEnabled)
                {
                    _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + ": Creating Thread for DocketEntry_ID: " + docketEntryID.ToString());
                }

                IUtility _utility = new Utility(_configuration, _utillogger, _keywordScoringProcess)
                {
                    Batch_ID = batchId,
                    DocketEntry_ID = docketEntryID,
                    Debug_Enabled = debugEnabled,
                    UnMatch_Enabled = unMatchEnabled,
                    Docket_Scoring_Status = docketScoringStatus,
                    Error_List = ErrorList,
                    Log_List = LogList
                };
                _utility.Execute();
                //var demThread = new Thread(_utility.Execute)
                //{
                //    Name = "DEMCAT_KEYWORD_SCORE_" + docketEntryID.ToString().Trim()
                //};
                //threadName = demThread.Name;
                //demThread.Start();
                //ThreadList.Add(demThread);

                if (debugEnabled)
                {
                    _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + ": Thread Started: " + threadName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(System.DateTime.Now.ToLongTimeString() + ": Exception: Thread could not be created or started: " + threadName + ": Error: " + ex.ToString());
            }
        }


    }
}

