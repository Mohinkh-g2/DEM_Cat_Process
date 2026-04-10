using LCI.Dem.Categorization.Constants;
using LCI.Dem.Categorization.Contracts;
using LCI.Dem.Categorization.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Workers
{
    /// <summary>
    /// Class <see cref="CategorizationService"/>
    /// </summary>
    public class CategorizationService : BackgroundService
    {
        /// <summary>
        /// logger for <see cref="CategorizationService"/>
        /// </summary>
        private readonly ILogger<CategorizationService> _logger;

        /// <summary>
        /// service for <see cref="ICategorizationManager"/>
        /// </summary>
        private readonly ICategorizationManager _categorizationManagerNoScope;

        /// <summary>
        /// service for <see cref="ICategorizationManager"/>
        /// </summary>
        private readonly IProcessCourtManager _processCourtManager;        

        /// <summary>
        /// <see cref="IConfiguration"/>
        /// </summary>
        private readonly IConfiguration _configuration;

        private readonly IServiceProvider _services;

        //public readonly object _object = new object();

        //private readonly object __lockObj = new();

        /// <summary>
        /// Constructor for CategorizationService
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="configuration">configuration</param>
        public CategorizationService(ILogger<CategorizationService> logger, ICategorizationManager categorizationManagerNoScope, IProcessCourtManager processCourtManager, IConfiguration configuration, IServiceProvider services)
        {
            _logger = logger;
            _configuration = configuration;
            _processCourtManager = processCourtManager;
            _categorizationManagerNoScope = categorizationManagerNoScope;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"CategorizationService is starting.");

            stoppingToken.Register(() => _logger.LogInformation($" CategorizationService background task is stopping."));

            var getScoringStatusMaster = await _categorizationManagerNoScope.GetAllScoringStatusMaster();

            var notStarted = getScoringStatusMaster.Where(x => x.Name.StartsWith("NotStarted")).FirstOrDefault().ID;

            var inProgress = getScoringStatusMaster.Where(x => x.Name.StartsWith("InProgress")).FirstOrDefault().ID;

            var Completed = getScoringStatusMaster.Where(x => x.Name.StartsWith("Complete")).FirstOrDefault().ID;

            var jobCheckInterval = TimeSpan.FromSeconds(5);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"CategorizationService task doing background work.");

                var getConfigData = await _categorizationManagerNoScope.GetAllConfigValues();


                var getProcessCourt = await _processCourtManager.GetProcessCourt();

                var lst_job_Id = getProcessCourt
                    .Where(x => x.CategorizationStatus == "WT" && x.Job_Id_Num != null && x.ParserStatus == "DN" && x.PullerStatus == "DN")
                    .OrderBy(c => c.ProcessCourtId)
                    .ToList();



                var lst_job_Id_PG = getProcessCourt.Where(x => x.CategorizationStatus == "PG" && x.Job_Id_Num != null);

                await _categorizationManagerNoScope.ExecuteStoreProcedure(StoreProcedure.FillRegExForKeywords);

                #region Variable Section to hold value

                var metrics_collection_on = 0;

                var batch_size = 10000;

                var MultiThreadedKeywordScoring = 0;

                var MaxThreads = 20;

                var TimeBetweenThreadCreationInMiliSeconds = 50;

                var CLRDebugEnabled = 0;

                var UnMatchEnabled = 0;

                var parallelJobCount = 3;

               

                #endregion

                if (getConfigData.Any() && lst_job_Id.Any())
                {
                    var setOperationalData = new string[] { "METRICS_STATE", "BATCH_SIZE", "MULTI_THREADED_KEYWORD_SCORING", "MAX_THREADS", "TIME_BETWEEN_THREAD_CREATION", "CLR_DEBUG_ENABLED", "UNMATCH_ENABLED", "PARALLEL_JOB_COUNT" };

                    var configActions = new Dictionary<string, Action<int>>
                    {
                        { "METRICS_STATE", val => metrics_collection_on = val },
                        { "BATCH_SIZE", val => batch_size = val },
                        { "MULTI_THREADED_KEYWORD_SCORING", val => MultiThreadedKeywordScoring = val },
                        { "MAX_THREADS", val => MaxThreads = val },
                        { "TIME_BETWEEN_THREAD_CREATION", val => TimeBetweenThreadCreationInMiliSeconds = val },
                        { "CLR_DEBUG_ENABLED", val => CLRDebugEnabled = val },
                        { "UNMATCH_ENABLED", val => UnMatchEnabled = val },
                        { "PARALLEL_JOB_COUNT", val => parallelJobCount = val }
                    };

                    foreach (var key in configActions.Keys)
                    {
                        var value = GetConfigValue(getConfigData, key);
                        configActions[key](value);
                    }



                    try
                    {
                        try
                        {
                            if (metrics_collection_on == 1)
                            {

                                var selectedJobs = lst_job_Id.Take(parallelJobCount).ToList();

                                // Ensure that this method is marked as async
                                await Task.WhenAll(selectedJobs.Select(job =>
                                    _processCourtManager.UpdateProcessCourtStatus(job.Job_Id_Num, "PG")
                                ));



                                var lstJobId = selectedJobs.Select(x => x.Job_Id_Num).Distinct();
                                var jobIdToProcess = GetCommaSeperatedValue(lstJobId, false);

                                // Trigger and Batch creation
                                var lst_trigger_id = await _categorizationManagerNoScope.ExecuteStatiStoreProcedure(StoreProcedure.Create_Trigger_Stat_Entry, jobIdToProcess);
                                var trigger_id = GetCommaSeperatedValue(lst_trigger_id, false);

                                var lst_batch_id = await _categorizationManagerNoScope.Execute_Procedure_Create_Batch_Stat_Entry(trigger_id);
                                var details = GetCommaSeperatedValueDatatable(lst_batch_id, false);

                                var jobDetails = (from emp in details.AsEnumerable()
                                                  select new {
                                                      BatchId = emp.Field<string>("batchid"),
                                                      TriggerId = emp.Field<string>("triggerid"),
                                                      JobId = emp.Field<string>("jobid")
                                                  }).ToList();

                                var semaphore = new SemaphoreSlim(MaxThreads);
                                var jobTasks = new List<Task>();

                                foreach (var currentElement in jobDetails)
                                {

                                        await semaphore.WaitAsync(stoppingToken);
                                        var task = Task.Run(async () =>
                                    {
                                        
                                        try
                                        {
                                            stoppingToken.ThrowIfCancellationRequested();
                                            using var scope = _services.CreateScope();

                                            var _categorizationManager = scope.ServiceProvider.GetRequiredService<ICategorizationManager>();
                                            var _keywordScoringManager = scope.ServiceProvider.GetRequiredService<IKeywordScoringManager>();
                                            var steplist = new List<string>();

                                            _logger.LogInformation($"Starting job: {currentElement.JobId} | Batch: {currentElement.BatchId}");

                                            //await _processCourtManager.UpdateProcessCourtStatus(currentElement.JobId, "PG");
                                            #region Staging State

                                            await _processCourtManager.UpdateProcessCourtBatchId(currentElement.JobId, Convert.ToInt32(currentElement.BatchId));

                                            //stepid batchid substatus
                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Staging"));

                                            await _categorizationManager.ExecuteStagingStoreProcedure(StoreProcedure.StagingMethods, currentElement.JobId);

                                            //Task.WaitAll(task); //Complete Task Here then Proceed

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Staging"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Staging complete");

                                            #endregion

                                            #region Keyword SCoring

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Keyword Scoring"));

                                            var debugEnabled = CLRDebugEnabled == 1;

                                            var unMatchEnabled = UnMatchEnabled == 1;


                                            await _keywordScoringManager.KeywordScoring(Convert.ToInt32(currentElement.BatchId), currentElement.JobId, inProgress, MaxThreads, TimeBetweenThreadCreationInMiliSeconds, debugEnabled, unMatchEnabled);

                                            //Task.WaitAll(tasktwo);  //Complete KeywordSoring Task Here then Proceed

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Keyword Scoring"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Keyword Scoring complete");

                                            #endregion

                                            #region Event Scoring

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Event Scoring"));

                                            await _categorizationManager.ExecuteEvenetLevelWiseStoreProcedure(StoreProcedure.PerformEventScoring_LevelWise, currentElement.JobId);

                                            //Task.WaitAll(taskThree);                                            

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Event Scoring"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Event Scoring complete");

                                            #endregion

                                            #region BNC Parent Mining

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "BNC Parent Mining"));

                                            await _categorizationManager.ExecuteStoreProcedureWithJobId(StoreProcedure.Perform_BNCParentEventMining, currentElement.JobId);

                                            //Task.WaitAll(taskFour);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "BNC Parent Mining"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "BNC Parent Event Mining complete");

                                            #endregion

                                            #region Archive Scoring

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Archive Scoring Tables"));

                                            await _categorizationManager.ExecuteStoreProcedureWithJobId(StoreProcedure.ArchiveScoringTables, currentElement.JobId);

                                            //Task.WaitAll(taskFive);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Archive Scoring Tables"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Archive Scoring Tables complete");

                                            #endregion

                                            #region Date Mining Start

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Date Mining"));

                                            await _categorizationManager.ExecuteDateMiningStoreProcedure(StoreProcedure.PerformDateMining, currentElement.JobId);

                                            //Task.WaitAll(taskSix);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Date Mining"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Date Mining complete");

                                            #endregion

                                            #region AP Collection Start

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "AP Collection"));

                                            await _categorizationManager.ExecuteStoreProcedureWithJobId(StoreProcedure.CollectAndInsertAdversaryProceedings, currentElement.JobId);

                                            //Task.WaitAll(taskSeven);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "AP Collection"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "AP(adversary) Collection complete");

                                            #endregion

                                            #region Danger MISC De-Duping Start

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Danger MISC De-Duping"));

                                            await _categorizationManager.ExecuteStoreProcedureWithJobId(StoreProcedure.Perform_DangerMISC_DeDupe, currentElement.JobId);

                                            //Task.WaitAll(taskEight);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Danger MISC De-Duping"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Danger MISC De-Duping complete");

                                            #endregion

                                            #region POI Scoring Start

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "POI Scoring"));

                                            await _categorizationManager.ExecuteStoreProcedureWithJobId(StoreProcedure.POIScoringOnAllDocketEntries, currentElement.JobId);

                                            //Task.WaitAll(taskNine);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "POI Scoring"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "POI Scoring complete");

                                            #endregion

                                            #region Archive POI Scores Start

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Archive POI Scores"));

                                            await _categorizationManager.ExecuteStoreProcedure(StoreProcedure.ArchiveScoringTables_ForPOI);

                                            //Task.WaitAll(taskTen);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "ArchivePOI Scores"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Archive POI Scores complete");

                                            #endregion

                                            #region Off-Staging Start

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Off-Staging"));

                                            await _categorizationManager.ExecuteStoreProcedureWithJobId(StoreProcedure.OffStageTheBatchOfDocketTexts, currentElement.JobId);

                                            //Task.WaitAll(taskEvelen);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Off-Staging"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Off-Staging complete");

                                            #endregion

                                            #region Dumb MISC Start

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Dump MISC"));

                                            await _categorizationManager.ExecuteDumpMiscStoreProcedure(StoreProcedure.Populate_Dumb_MISC, currentElement.JobId);

                                            //Task.WaitAll(taskTwelve);

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Dump MISC"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "DUMP Misc complete");

                                            #endregion

                                            #region Dem Extract Dates 
                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Extract Dates"));

                                            await _categorizationManager.DemExtractCaseDates(currentElement.JobId);
                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Extract Dates"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "Extract Dates complete");

                                            #endregion



                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Updating Batch Stat"));

                                            await _categorizationManager.Execute_Procedure_Update_Batch_Stat_Entry(Convert.ToInt32(currentElement.BatchId), Completed, "Complete");

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "Updating Batch Stat"));

                                            steplist.AddRange(await _categorizationManager.Execute_Procedure_Batch_Step_Start(Convert.ToInt32(currentElement.BatchId), "Updating Trigger Stat"));

                                            await _categorizationManager.Execute_Procedure_Update_Trigger_Stat_Entry(Convert.ToInt32(currentElement.BatchId), inProgress, "");

                                            await _categorizationManager.Execute_Procedure_Batch_Step_Complete(GetStepDetails(steplist, Convert.ToInt32(currentElement.BatchId), "UpdatingTrigger Stat"));

                                            await _categorizationManager.Execute_Procedure_Update_Trigger_Stat_Entry(Convert.ToInt32(currentElement.TriggerId), Completed, "");


                                            await _processCourtManager.UpdateProcessCourtStatus(currentElement.JobId, "DN");
                                        }
                                        catch (Exception e)
                                        {
                                            _logger.LogError(e
                                                , Convert.ToString(string.Format("ERROR: {0} ", "CategorizationService")) + "Job_Id : {Job_Id_Num}"
                                                , currentElement.JobId);

                                            await _processCourtManager.UpdateProcessFailedCourtStatus(currentElement.JobId, Convert.ToString(string.Format("Exeception Occur : {0} for Job_Id {1} trigger_Id {2}", Convert.ToString(e.InnerException), currentElement.JobId, currentElement.TriggerId)));
                                        }
                                        finally
                                        {
                                            semaphore.Release();
                                        }
                                    }, stoppingToken);



                                    jobTasks.Add(task);
                                }
                                    await Task.WhenAll(jobTasks);
                                    await Task.Delay(3000);

                                }



                        }
                        catch (Exception e)
                        {
                            var lst = lst_job_Id.Take(parallelJobCount);
                            _logger.LogError(e,
                            Convert.ToString(string.Format("ERROR: {0} ", "CategorizationService")) + "Job_Id : {Job_Id_Num}"
                            , lst.Select(x => x.Job_Id_Num).Distinct());

                        }


                    }

                    catch (Exception e)
                    {
                        _logger.LogError(e, Convert.ToString(string.Format("ERROR: {0} ", "CategorizationService")));
                    }


                }
                    await Task.Delay(jobCheckInterval, stoppingToken);
                }
        }
           
            catch (Exception ex)
{
                _logger.LogError(ex, "Unexpected error in CategorizationService main loop.");
            }
        }
            
        
        private static int GetStepDetails(List<string> steplist,int batchid,string substatus)
        {
            var getStepDetail = GetCommaSeperatedValueDatatableStep(steplist);

            var getstepId = (from emp in getStepDetail.AsEnumerable()
                                              select new {
                                                  stepid = emp.Field<string>("stepid"),  //variable
                                                  batchid = emp.Field<string>("batchid"), //variable
                                                  substatus = emp.Field<string>("substatus") //variable
                                              }).ToList();

            return Convert.ToInt32(getstepId.Where(c => Convert.ToInt32(c.batchid) == batchid &&  c.substatus == Convert.ToString(substatus.Split(null)[0]).Trim()).Select(c => c.stepid).FirstOrDefault());
        }

        private int GetConfigValue(IEnumerable<Config> getConfigData, string keyData)
        {
            _logger.LogInformation($"Get All Config Value - Start");

            var metrics_collection_on_value = getConfigData.Where(x => x.Name == keyData).Select(m => m.Value).FirstOrDefault();

            _logger.LogInformation($"get metrics collection " + metrics_collection_on_value);

            if (!string.IsNullOrEmpty(metrics_collection_on_value) && metrics_collection_on_value.Length > 0)
            {
                return Convert.ToInt32(metrics_collection_on_value);
            }

            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="passValue"></param>
        /// <param name="clientCode"></param>
        /// <returns></returns>
        private static string GetCommaSeperatedValue(IEnumerable<string> passValue, bool clientCode)
        {
            var lst = new List<string>();
            foreach (var item in passValue.ToList())
            {
                var ssize = item.Split(null);
                var codeExist = lst.Contains(clientCode ? ssize[1] : ssize[0]);
                if (!codeExist)
                {
                    lst.Add(clientCode ? ssize[1] : ssize[0]);
                }
            }
            var response = string.Join(",", lst.Select(n => n.ToString()).ToArray());
            return response;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="passValue"></param>
        /// <param name="clientCode"></param>
        /// <returns></returns>
        private static DataTable GetCommaSeperatedValueDatatable(IEnumerable<string> passValue, bool clientCode)
        {
            var table = new DataTable();
            table.Columns.Add("batchid", typeof(string));
            table.Columns.Add("triggerid", typeof(string));
            table.Columns.Add("jobid", typeof(string));
            foreach (var item in passValue.ToList())
            {
                var ssize = item.Split(null);
                table.Rows.Add(ssize[0], ssize[1], ssize[2]);
            }
            return table;
        }


        private static DataTable GetCommaSeperatedValueDatatableStep(IEnumerable<string> passValue)
        {
            var table = new DataTable();
            table.Columns.Add("stepid", typeof(string));
            table.Columns.Add("batchid", typeof(string));
            table.Columns.Add("substatus", typeof(string));
            foreach (var item in passValue.ToList())
            {
                var ssize = item.Split(null);
                table.Rows.Add(ssize[0], ssize[1], ssize[2]);
            }
            return table;
        }



    }
}
