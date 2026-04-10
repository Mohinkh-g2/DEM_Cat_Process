using LCI.Dem.Categorization.Constants;
using LCI.Dem.Categorization.Contracts;
using LCI.Dem.Categorization.Data.Entity.KeywordScoring;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Data.DataManager
{
    public class Utility : DbFactoryBase, IUtility
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<Utility> _logger;

        private const string _connectionStringKey = DatabaseConnection.SQLDBConnectionString;

        private readonly IKeywordScoringProcess _keywordScoringProcess;

        private readonly IMemoryCache _memoryCache;
        private const string strRegEx0 = "[^a-z]";
        private const string strRegEx = "%[^a-z]clos%";
        private const string strRegEx1 = "%receiv%grant[^a-z]%";
        private const string strRegEx2 = "%grant[^a-z]%receiv%";
        private const string strRegEx3 = "%HAF[^a-z]%fund%";
        private const string strRegEx4 = "%fund%HAF[^a-z]%";
        private const string strRegEx5 = "%HAF[^a-z]%assist%";

        private string sThreadName;

        public int Batch_ID { get; set; }

        public int DocketEntry_ID { get; set; }

        public bool Debug_Enabled { get; set; }

        public bool UnMatch_Enabled { get; set; }

        public DocketScoringStatus Docket_Scoring_Status { get; set; }

        public List<string> Error_List { get; set; }

        public List<string> Log_List { get; set; }

        public Utility(IConfiguration configuration, ILogger<Utility> logger, IKeywordScoringProcess keywordScoringProcess, IMemoryCache memoryCache) : base(configuration, _connectionStringKey)
        {
            _logger = logger;
            _configuration = configuration;
            _keywordScoringProcess = keywordScoringProcess;
            _memoryCache = memoryCache;
        }

        public void Execute()
        {
            var regularEvent = 1;

            IEnumerable<int> distinctEventIDList = null;

            try
            {
                #region start Keyword scoring

                sThreadName = DocketEntry_ID.ToString();

                _logger.LogInformation($"Start : Keyword scoring for :" + DocketEntry_ID + " : " + DateTime.Now);

                var eventKeywordMasterListByLevel3 = GetEventKeywordMastersByLevel3(sThreadName);

                var eventKeywordMasterListByLevel012 = GetEventKeywordMastersByLevel012(sThreadName);

                var eventKeywordMasterListByLevel101100 = GetEventKeywordMastersByLevel101100(sThreadName);

                var docketKeywordScoreList = new List<DocketKeywordScoreComplete>();

                if (eventKeywordMasterListByLevel3.Any())
                {
                    var docketKeywordScoreLevel3
                        = GetDocketKeywordScoreLevel3(UnMatch_Enabled, Batch_ID, Docket_Scoring_Status,
                        eventKeywordMasterListByLevel3, sThreadName, out distinctEventIDList);

                    if (docketKeywordScoreLevel3 != null && docketKeywordScoreLevel3.Any())
                    {
                        docketKeywordScoreList.AddRange(docketKeywordScoreLevel3);

                        FirstMethod(distinctEventIDList, eventKeywordMasterListByLevel012, docketKeywordScoreList);

                        SecondMethod(regularEvent, distinctEventIDList, eventKeywordMasterListByLevel101100, docketKeywordScoreList);
                    }
                }

                if (docketKeywordScoreList != null && docketKeywordScoreList.Any())
                {
                    _keywordScoringProcess.ExecuteProcedureCreateDocketKeywordScoreCompleteByLevels(docketKeywordScoreList, sThreadName, Batch_ID);
                }

                _logger.LogInformation($"Complete : Inserting keyword scores : " + DateTime.Now);

                #endregion

            }
            catch (Exception ex)
            {
                //Error_List.Add("Exception: " + sThreadName + ex.ToString());
            }
            //Log_List.Add(System.DateTime.Now.ToLongTimeString() + ": Ending Thread: " + sThreadName);
        }

        private void SecondMethod(int regularEvent, IEnumerable<int> distinctEventIDList, IEnumerable<EventKeywordMaster> eventKeywordMasterListByLevel101100, List<DocketKeywordScoreComplete> docketKeywordScoreList)
        {
            if (distinctEventIDList != null && distinctEventIDList.Any()
                                        && eventKeywordMasterListByLevel101100 != null && eventKeywordMasterListByLevel101100.Any())
            {
                var eventKeywordMasterDKSListLevel101100
                    = GetEventKeywordMasterDKSListLevel101100(eventKeywordMasterListByLevel101100, distinctEventIDList);

                if (eventKeywordMasterDKSListLevel101100 != null && eventKeywordMasterDKSListLevel101100.Any())
                {
                    var filteredEventMasterList = GetEventMasterByRegularEvent(regularEvent, sThreadName);

                    if (filteredEventMasterList != null && filteredEventMasterList.Any())
                    {
                        var eventKeywordMasterEMListLevel101100 = GetEventKeywordMasterEMListLevel101100(eventKeywordMasterDKSListLevel101100, filteredEventMasterList);

                        if (eventKeywordMasterEMListLevel101100 != null && eventKeywordMasterEMListLevel101100.Any())
                        {
                            var docketKeywordScoreLevel101100
                                = GetdocketKeywordScoreLevel101100(UnMatch_Enabled, Batch_ID, Docket_Scoring_Status, eventKeywordMasterEMListLevel101100, sThreadName);

                            if (docketKeywordScoreLevel101100 != null && docketKeywordScoreLevel101100.Any())
                            {
                                docketKeywordScoreList.AddRange(docketKeywordScoreLevel101100);
                            }
                        }

                    }
                }
            }
        }

        private void FirstMethod(IEnumerable<int> distinctEventIDList, IEnumerable<EventKeywordMaster> eventKeywordMasterListByLevel012, List<DocketKeywordScoreComplete> docketKeywordScoreList)
        {
            if (distinctEventIDList != null && distinctEventIDList.Any() && eventKeywordMasterListByLevel012 != null
                                        && eventKeywordMasterListByLevel012.Any())
            {
                var eventKeywordMasterListLevel012 = GetEventKeywordMasterListLevel012(eventKeywordMasterListByLevel012, distinctEventIDList);

                if (eventKeywordMasterListLevel012 != null && eventKeywordMasterListLevel012.Any())
                {
                    var docketKeywordScoreLevel012
                        = GetdocketKeywordScoreLevel012(UnMatch_Enabled, Batch_ID, Docket_Scoring_Status, eventKeywordMasterListLevel012, sThreadName);

                    if (docketKeywordScoreLevel012 != null && docketKeywordScoreLevel012.Any())
                    {
                        docketKeywordScoreList.AddRange(docketKeywordScoreLevel012);
                    }
                }
            }
        }

        private IEnumerable<EventKeywordMaster> GetEventKeywordMastersByLevel3(string sThreadName)
        {
            var cacheKey = "GetEventKeywordMastersByLevel3";

            if (!_memoryCache.TryGetValue(cacheKey, out List<EventKeywordMaster> eventKeywordMasterListone))
            {
                eventKeywordMasterListone = (List<EventKeywordMaster>)_keywordScoringProcess.GetEventKeywordMasterLevel3(sThreadName).Result;

                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddDays(1),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                };
                //setting cache entries
                _memoryCache.Set(cacheKey, eventKeywordMasterListone, cacheExpiryOptions);
            }

            return eventKeywordMasterListone;
        }

        private IEnumerable<EventKeywordMaster> GetEventKeywordMastersByLevel012(string sThreadName)
        {
            var cacheKey = "GetEventKeywordMastersByLevel012";

            if (!_memoryCache.TryGetValue(cacheKey, out List<EventKeywordMaster> eventKeywordMasterListTwo))
            {
                eventKeywordMasterListTwo = (List<EventKeywordMaster>)_keywordScoringProcess.GetEventKeywordMasterLevel012(sThreadName).Result;

                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddDays(1),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                };
                //setting cache entries
                _memoryCache.Set(cacheKey, eventKeywordMasterListTwo, cacheExpiryOptions);
            }

            return eventKeywordMasterListTwo;
        }

        private IEnumerable<EventKeywordMaster> GetEventKeywordMastersByLevel101100(string sThreadName)
        {
            var cacheKey = "GetEventKeywordMastersByLevel101100";

            if (!_memoryCache.TryGetValue(cacheKey, out List<EventKeywordMaster> eventKeywordMasterListThree))
            {
                eventKeywordMasterListThree = (List<EventKeywordMaster>)_keywordScoringProcess.GetEventKeywordMasterLevel101100(sThreadName).Result;

                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddDays(1),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                };
                //setting cache entries
                _memoryCache.Set(cacheKey, eventKeywordMasterListThree, cacheExpiryOptions);
            }

            return eventKeywordMasterListThree;
        }

        private IEnumerable<EventMaster> GetEventMasterByRegularEvent(int regularEvent, string sThreadName)
        {
            var cacheKey = "GetEventMasterByRegularEvent";

            if (!_memoryCache.TryGetValue(cacheKey, out List<EventMaster> eventMasterListOne))
            {
                eventMasterListOne = (List<EventMaster>)_keywordScoringProcess.GetEventMaster(regularEvent, sThreadName).Result;

                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddDays(1),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                };
                //setting cache entries
                _memoryCache.Set(cacheKey, eventMasterListOne, cacheExpiryOptions);
            }

            return eventMasterListOne;
        }

        private static IEnumerable<DocketKeywordScoreComplete> GetDocketKeywordScoreLevel3(bool unMatchEnabled, int batchId, DocketScoringStatus docketScoringStatus, IEnumerable<EventKeywordMaster> eventKeywordMasterList, string threadName, out IEnumerable<int> distinctEventIDList)
        {
            var docketKeywordScoreLevel3 = new List<DocketKeywordScoreComplete>();
            try
            {
                var docketText_Corrected = docketScoringStatus.DocketText_Corrected.ToLower();

                if (unMatchEnabled)
                {

                }
                else
                {
                    for (var i = 0; i < eventKeywordMasterList.Count(); i++)
                    {
                        var startTick = DateTime.Now.Ticks;
                        var e = eventKeywordMasterList.ToArray();
                        var freq = e[i].Keyword.Split('%').Length - 1;
                        if (freq == 1)
                        {

                            if (e[i].Keyword.StartsWith('%'))
                            {
                                var key = e[i].Keyword.Replace("%", "");
                                if (docketText_Corrected.EndsWith(key.ToLower()))
                                {
                                    var endTick = DateTime.Now.Ticks;
                                    var interval = endTick - startTick;
                                    docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                    {
                                        Batch_ID = batchId,
                                        DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                        Keyword_ID = e[i].Keyword_ID,
                                        Event_ID = e[i].Event_ID,
                                        Score = e[i].keyword_level,
                                        Match = true,
                                        Time_Taken = Convert.ToInt32(interval),
                                        Thread_Name = threadName
                                    });
                                }
                            }
                            else if (e[i].Keyword.EndsWith('%'))
                            {
                                var key = e[i].Keyword.Replace("%", "");
                                if (docketText_Corrected.StartsWith(key.ToLower()))
                                {
                                    var endTick = DateTime.Now.Ticks;
                                    var interval = endTick - startTick;
                                    docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                    {
                                        Batch_ID = batchId,
                                        DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                        Keyword_ID = e[i].Keyword_ID,
                                        Event_ID = e[i].Event_ID,
                                        Score = e[i].keyword_level,
                                        Match = true,
                                        Time_Taken = Convert.ToInt32(interval),
                                        Thread_Name = threadName
                                    });
                                }
                            }

                        }
                        else if (freq == 2)
                        {
                            if (e[i].Keyword.StartsWith("% "))
                            {
                                var key = e[i].Keyword.ToLower().Replace("%", "");
                                if (docketText_Corrected.Contains(key))
                                {
                                    var endTick = DateTime.Now.Ticks;
                                    var interval = endTick - startTick;
                                    docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                    {
                                        Batch_ID = batchId,
                                        DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                        Keyword_ID = e[i].Keyword_ID,
                                        Event_ID = e[i].Event_ID,
                                        Score = e[i].keyword_level,
                                        Match = true,
                                        Time_Taken = Convert.ToInt32(interval),
                                        Thread_Name = threadName
                                    });
                                }
                            }
                            else if (e[i].Keyword.Equals(strRegEx))
                            {
                                var key = e[i].Keyword.ToLower().Replace("%", "");
                                try
                                {
                                    if (Regex.IsMatch(docketText_Corrected, key))
                                    {
                                        var endTick = DateTime.Now.Ticks;
                                        var interval = endTick - startTick;
                                        docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                        {
                                            Batch_ID = batchId,
                                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                            Keyword_ID = e[i].Keyword_ID,
                                            Event_ID = e[i].Event_ID,
                                            Score = e[i].keyword_level,
                                            Match = true,
                                            Time_Taken = Convert.ToInt32(interval),
                                            Thread_Name = threadName
                                        });
                                    }

                                }
                                catch (Exception ex)
                                {
                                    if (docketText_Corrected.Contains(key))
                                    {
                                        var endTick = DateTime.Now.Ticks;
                                        var interval = endTick - startTick;
                                        docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                        {
                                            Batch_ID = batchId,
                                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                            Keyword_ID = e[i].Keyword_ID,
                                            Event_ID = e[i].Event_ID,
                                            Score = e[i].keyword_level,
                                            Match = true,
                                            Time_Taken = Convert.ToInt32(interval),
                                            Thread_Name = threadName
                                        });
                                    }


                                }

                            }
                            else
                            {
                                var key = e[i].Keyword.ToLower().Replace("%", "");

                                if (docketText_Corrected.Contains(key))
                                {
                                    var endTick = DateTime.Now.Ticks;
                                    var interval = endTick - startTick;
                                    docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                    {
                                        Batch_ID = batchId,
                                        DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                        Keyword_ID = e[i].Keyword_ID,
                                        Event_ID = e[i].Event_ID,
                                        Score = e[i].keyword_level,
                                        Match = true,
                                        Time_Taken = Convert.ToInt32(interval),
                                        Thread_Name = threadName
                                    });
                                }
                                else if (key.Contains('_'))
                                {
                                    var k = key?.ToLower().Split('_');
                                    var isExist = false;

                                    if (k.Length == 2)
                                    {
                                        isExist = IsOneCharDiff(docketText_Corrected, k[0], k[1]);
                                    }
                                    else
                                    {
                                        isExist = IsOneCharDiff1(docketText_Corrected, k[0], k[1], k[2]);
                                    }


                                    if (isExist)
                                    {
                                        var endTick = DateTime.Now.Ticks;
                                        var interval = endTick - startTick;
                                        docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                        {
                                            Batch_ID = batchId,
                                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                            Keyword_ID = e[i].Keyword_ID,
                                            Event_ID = e[i].Event_ID,
                                            Score = e[i].keyword_level,
                                            Match = true,
                                            Time_Taken = Convert.ToInt32(interval),
                                            Thread_Name = threadName
                                        });
                                    }
                                }
                                else if (!e[i].Keyword.StartsWith('%'))
                                {
                                    var k = e[i].Keyword.ToLower().Split('%');
                                    if (docketText_Corrected.StartsWith(k[0]) && docketText_Corrected.Contains(k[1]))
                                    {
                                        var endTick = DateTime.Now.Ticks;
                                        var interval = endTick - startTick;
                                        docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                        {
                                            Batch_ID = batchId,
                                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                            Keyword_ID = e[i].Keyword_ID,
                                            Event_ID = e[i].Event_ID,
                                            Score = e[i].keyword_level,
                                            Match = true,
                                            Time_Taken = Convert.ToInt32(interval),
                                            Thread_Name = threadName
                                        });
                                    }

                                }

                            }

                        }
                        else if (freq == 3)
                        {
                            if (e[i].Keyword.Equals(strRegEx1) ||
                                e[i].Keyword.Equals(strRegEx2) ||
                                e[i].Keyword.Equals(strRegEx3) ||
                                e[i].Keyword.Equals(strRegEx4) ||
                                e[i].Keyword.Equals(strRegEx5)
                                )
                            {
                                var k1 = e[i].Keyword.ToLower().Split('%');
                                var temp = new List<string>();
                                foreach (var k11 in k1.ToList())
                                {
                                    if (!string.IsNullOrEmpty(k11))
                                        temp.Add(k11);
                                }
                                var temp1 = temp?.ToArray();
                                if (temp1[0].Contains(strRegEx0))
                                {
                                    var strK1 = temp1[0].Replace("%", "");

                                    if (Regex.IsMatch(docketText_Corrected, strK1))
                                    {
                                        if (docketText_Corrected.Contains(temp1[1]))
                                        {
                                            var endTick = DateTime.Now.Ticks;
                                            var interval = endTick - startTick;
                                            docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                            {
                                                Batch_ID = batchId,
                                                DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                                Keyword_ID = e[i].Keyword_ID,
                                                Event_ID = e[i].Event_ID,
                                                Score = e[i].keyword_level,
                                                Match = true,
                                                Time_Taken = Convert.ToInt32(interval),
                                                Thread_Name = threadName
                                            });
                                        }

                                    }


                                }
                                else if (temp1[1].Contains(strRegEx0))
                                {
                                    var strK1 = temp1[1].Replace("%", "");

                                    if (Regex.IsMatch(docketText_Corrected, strK1))
                                    {
                                        if (docketText_Corrected.Contains(temp1[0]))
                                        {
                                            var endTick = DateTime.Now.Ticks;
                                            var interval = endTick - startTick;
                                            docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                            {
                                                Batch_ID = batchId,
                                                DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                                Keyword_ID = e[i].Keyword_ID,
                                                Event_ID = e[i].Event_ID,
                                                Score = e[i].keyword_level,
                                                Match = true,
                                                Time_Taken = Convert.ToInt32(interval),
                                                Thread_Name = threadName
                                            });
                                        }

                                    }

                                }

                            }
                            else
                            {
                                var temp = new List<string>();
                                var k1 = e[i].Keyword.ToLower().Split('%');
                                foreach (var k11 in k1.ToList())
                                {
                                    if (!string.IsNullOrEmpty(k11))
                                        temp.Add(k11);
                                }
                                var temp1 = temp?.ToArray();
                                if (temp1[0].Contains('_'))
                                {
                                    var k = temp1[0]?.Split('_');
                                    var isExist = IsOneCharDiff(docketText_Corrected, k[0], k[1]);

                                    if (isExist && docketText_Corrected.Contains(temp1[1]))
                                    {
                                        var endTick = DateTime.Now.Ticks;
                                        var interval = endTick - startTick;
                                        docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                        {
                                            Batch_ID = batchId,
                                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                            Keyword_ID = e[i].Keyword_ID,
                                            Event_ID = e[i].Event_ID,
                                            Score = e[i].keyword_level,
                                            Match = true,
                                            Time_Taken = Convert.ToInt32(interval),
                                            Thread_Name = threadName
                                        });
                                    }
                                }
                                else if (temp1[1].Contains('_'))
                                {
                                    var k = temp1[1]?.Split('_');
                                    var isExist = IsOneCharDiff(docketText_Corrected, k[0], k[1]);

                                    if (isExist && docketText_Corrected.Contains(temp1[0]))
                                    {
                                        var endTick = DateTime.Now.Ticks;
                                        var interval = endTick - startTick;
                                        docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                        {
                                            Batch_ID = batchId,
                                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                            Keyword_ID = e[i].Keyword_ID,
                                            Event_ID = e[i].Event_ID,
                                            Score = e[i].keyword_level,
                                            Match = true,
                                            Time_Taken = Convert.ToInt32(interval),
                                            Thread_Name = threadName
                                        });
                                    }
                                }
                                else
                                {
                                    var isMatched = IsWordExist(docketText_Corrected, temp1[0], temp1[1]);
                                    if (isMatched)
                                    {
                                        var endTick = DateTime.Now.Ticks;
                                        var interval = endTick - startTick;
                                        docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                        {
                                            Batch_ID = batchId,
                                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                            Keyword_ID = e[i].Keyword_ID,
                                            Event_ID = e[i].Event_ID,
                                            Score = e[i].keyword_level,
                                            Match = true,
                                            Time_Taken = Convert.ToInt32(interval),
                                            Thread_Name = threadName
                                        });
                                    }
                                }
                            }

                        }
                        else if (freq == 4)
                        {
                            var temp = new List<string>();
                            var k1 = e[i].Keyword.ToLower().Split('%');
                            foreach (var k11 in k1.ToList())
                            {
                                if (!string.IsNullOrEmpty(k11))
                                    temp.Add(k11);
                            }
                            var temp1 = temp?.ToArray();
                            var isMatched = false;
                            if (temp1.Length == 2)
                            {
                                isMatched = IsWordExist(docketText_Corrected, temp1[0], temp1[1]);
                            }
                            else if (temp1.Length == 3)
                            {
                                isMatched = IsWordExist1(docketText_Corrected, temp1[0], temp1[1], temp1[2]);
                            }

                            if (isMatched)
                            {
                                var endTick = DateTime.Now.Ticks;
                                var interval = endTick - startTick;
                                docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                {
                                    Batch_ID = batchId,
                                    DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                    Keyword_ID = e[i].Keyword_ID,
                                    Event_ID = e[i].Event_ID,
                                    Score = e[i].keyword_level,
                                    Match = true,
                                    Time_Taken = Convert.ToInt32(interval),
                                    Thread_Name = threadName
                                });
                            }
                        }
                        else if (freq == 5)
                        {
                            var temp = new List<string>();
                            var k1 = e[i].Keyword.ToLower().Split('%');
                            foreach (var k11 in k1.ToList())
                            {
                                if (!string.IsNullOrEmpty(k11))
                                    temp.Add(k11);
                            }
                            var temp1 = temp?.ToArray();
                            var isMatched = false;
                            if (temp1.Length == 3)
                            {
                                isMatched = IsWordExist1(docketText_Corrected, temp1[0], temp1[1], temp1[2]);
                            }
                            else
                            {
                                isMatched = IsWordExist2(docketText_Corrected, temp1[0], temp1[1], temp1[2], temp1[3]);
                            }


                            if (isMatched)
                            {
                                var endTick = DateTime.Now.Ticks;
                                var interval = endTick - startTick;
                                docketKeywordScoreLevel3.Add(new DocketKeywordScoreComplete
                                {
                                    Batch_ID = batchId,
                                    DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                    Keyword_ID = e[i].Keyword_ID,
                                    Event_ID = e[i].Event_ID,
                                    Score = e[i].keyword_level,
                                    Match = true,
                                    Time_Taken = Convert.ToInt32(interval),
                                    Thread_Name = threadName
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            distinctEventIDList = docketKeywordScoreLevel3.Select(e => e.Event_ID).Distinct();

            return docketKeywordScoreLevel3;
        }

        private static IEnumerable<EventKeywordMaster> GetEventKeywordMasterListLevel012(IEnumerable<EventKeywordMaster> eventKeywordMasterList, IEnumerable<int> docketKeywordScoreLevel3)
        {
            var eventKeywordMasterListLevel012 = new List<EventKeywordMaster>();

            foreach (var evt in docketKeywordScoreLevel3)
            {
                foreach (var e in eventKeywordMasterList)
                {
                    if (evt == e.Event_ID)
                    {
                        eventKeywordMasterListLevel012.Add(new EventKeywordMaster
                        {
                            Keyword = e.Keyword,
                            keyword_level = e.keyword_level,
                            Event_ID = e.Event_ID,
                            RegexString = e.RegexString,
                            Keyword_ID = e.Keyword_ID
                        });
                    }
                }
            }

            return eventKeywordMasterListLevel012;
        }

        private static IEnumerable<DocketKeywordScoreComplete> GetdocketKeywordScoreLevel012(bool unMatchEnabled, int batchId, DocketScoringStatus docketScoringStatus, IEnumerable<EventKeywordMaster> eventKeywordMasterListLevel012, string threadName)
        {
            var docketKeywordScoreLevel012 = new List<DocketKeywordScoreComplete>();

            if (unMatchEnabled)
            {
                foreach (var e in eventKeywordMasterListLevel012)
                {
                    var startTick = DateTime.Now.Ticks;
                    var data = RegexMatch.RegexCapture(docketScoringStatus.DocketText_Corrected, e.RegexString);
                    if (!string.IsNullOrEmpty(data))
                    {
                        var endTick = DateTime.Now.Ticks;
                        var interval = endTick - startTick;
                        docketKeywordScoreLevel012.Add(new DocketKeywordScoreComplete
                        {
                            Batch_ID = batchId,
                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                            Keyword_ID = e.Keyword_ID,
                            Event_ID = e.Event_ID,
                            Score = e.keyword_level,
                            Match = true,
                            Time_Taken = Convert.ToInt32(interval),
                            Thread_Name = threadName
                        });
                    }
                    else
                    {
                        var endTick = DateTime.Now.Ticks;
                        var interval = endTick - startTick;
                        docketKeywordScoreLevel012.Add(new DocketKeywordScoreComplete
                        {
                            Batch_ID = batchId,
                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                            Keyword_ID = e.Keyword_ID,
                            Event_ID = e.Event_ID,
                            Score = e.keyword_level,
                            Match = false,
                            Time_Taken = Convert.ToInt32(interval),
                            Thread_Name = threadName
                        });
                    }
                }
            }
            else
            {
                foreach (var e in eventKeywordMasterListLevel012)
                {
                    var startTick = DateTime.Now.Ticks;
                    var data = RegexMatch.RegexCapture(docketScoringStatus.DocketText_Corrected, e.RegexString);
                    if (!string.IsNullOrEmpty(data))
                    {
                        var endTick = DateTime.Now.Ticks;
                        var interval = endTick - startTick;
                        docketKeywordScoreLevel012.Add(new DocketKeywordScoreComplete
                        {
                            Batch_ID = batchId,
                            DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                            Keyword_ID = e.Keyword_ID,
                            Event_ID = e.Event_ID,
                            Score = e.keyword_level,
                            Match = true,
                            Time_Taken = Convert.ToInt32(interval),
                            Thread_Name = threadName
                        });
                    }

                }
            }

            return docketKeywordScoreLevel012;
        }

        private static IEnumerable<EventKeywordMaster> GetEventKeywordMasterDKSListLevel101100(IEnumerable<EventKeywordMaster> eventKeywordMasterList, IEnumerable<int> docketKeywordScoreLevel3)
        {
            var eventKeywordMasterDKSListLevel101100 = new List<EventKeywordMaster>();
            foreach (var evt in docketKeywordScoreLevel3)
            {
                foreach (var e in eventKeywordMasterList)
                {
                    if (evt == e.Event_ID)
                    {
                        eventKeywordMasterDKSListLevel101100.Add(new EventKeywordMaster
                        {
                            Keyword = e.Keyword,
                            keyword_level = e.keyword_level,
                            Event_ID = e.Event_ID,
                            RegexString = e.RegexString,
                            Keyword_ID = e.Keyword_ID
                        });
                    }
                }
            }
            return eventKeywordMasterDKSListLevel101100;
        }

        private static IEnumerable<EventKeywordMaster> GetEventKeywordMasterEMListLevel101100(IEnumerable<EventKeywordMaster> eventKeywordMasterDKSListLevel101100, IEnumerable<EventMaster> eventMasterList)
        {
            var eventKeywordMasterEMListLevel101100 = new List<EventKeywordMaster>();
            foreach (var e in eventKeywordMasterDKSListLevel101100)
            {
                foreach (var eventId in eventMasterList)
                {
                    if (e.Event_ID == eventId.Event_ID)
                    {
                        eventKeywordMasterEMListLevel101100.Add(new EventKeywordMaster
                        {
                            Keyword = e.Keyword,
                            keyword_level = e.keyword_level,
                            Event_ID = e.Event_ID,
                            RegexString = e.RegexString,
                            Keyword_ID = e.Keyword_ID
                        });
                    }
                }

            }
            return eventKeywordMasterEMListLevel101100;
        }

        private static IEnumerable<DocketKeywordScoreComplete> GetdocketKeywordScoreLevel101100(bool unMatchEnabled, int batchId, DocketScoringStatus docketScoringStatus, IEnumerable<EventKeywordMaster> eventKeywordMasterEMListLevel101100, string threadName)
        {
            var docketKeywordScoreLevel101100 = new List<DocketKeywordScoreComplete>();

            if (unMatchEnabled)
            {
                foreach (var e in eventKeywordMasterEMListLevel101100)
                {
                    var startTick = DateTime.Now.Ticks;
                    if (e.RegexString != null)
                    {
                        string data = RegexMatch.RegexCapture(docketScoringStatus.DocketText_Corrected, e.RegexString);

                        if (!string.IsNullOrEmpty(data))
                        {
                            var endTick = DateTime.Now.Ticks;
                            var interval = endTick - startTick;
                            docketKeywordScoreLevel101100.Add(new DocketKeywordScoreComplete
                            {
                                Batch_ID = batchId,
                                DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                Keyword_ID = e.Keyword_ID,
                                Event_ID = e.Event_ID,
                                Score = e.keyword_level,
                                Match = true,
                                Time_Taken = Convert.ToInt32(interval),
                                Thread_Name = threadName
                            });
                        }
                        else
                        {
                            var endTick = DateTime.Now.Ticks;
                            var interval = endTick - startTick;
                            docketKeywordScoreLevel101100.Add(new DocketKeywordScoreComplete
                            {
                                Batch_ID = batchId,
                                DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                Keyword_ID = e.Keyword_ID,
                                Event_ID = e.Event_ID,
                                Score = e.keyword_level,
                                Match = false,
                                Time_Taken = Convert.ToInt32(interval),
                                Thread_Name = threadName
                            });
                        }
                    }

                }

            }
            else
            {
                foreach (var e in eventKeywordMasterEMListLevel101100)
                {
                    var startTick = DateTime.Now.Ticks;
                    if (e.RegexString != null)
                    {
                        string data = RegexMatch.RegexCapture(docketScoringStatus.DocketText_Corrected, e.RegexString);

                        if (!string.IsNullOrEmpty(data))
                        {
                            var endTick = DateTime.Now.Ticks;
                            var interval = endTick - startTick;
                            docketKeywordScoreLevel101100.Add(new DocketKeywordScoreComplete
                            {
                                Batch_ID = batchId,
                                DocketEntry_ID = docketScoringStatus.DocketEntry_ID,
                                Keyword_ID = e.Keyword_ID,
                                Event_ID = e.Event_ID,
                                Score = e.keyword_level,
                                Match = true,
                                Time_Taken = Convert.ToInt32(interval),
                                Thread_Name = threadName
                            });
                        }

                    }

                }

            }
            return docketKeywordScoreLevel101100;
        }

        private static bool IsWordExist(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int start, end;
                start = strSource.IndexOf(strStart, 0) + strStart.Length;
                end = strSource.IndexOf(strEnd, start) + strEnd.Length;
                if (start < end)
                    return true;

            }
            return false;
        }

        private static bool IsWordExist1(string strSource, string strStart, string strMed, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strMed) && strSource.Contains(strEnd))
            {
                int start, med, end;
                start = strSource.IndexOf(strStart, 0) + strStart.Length;
                med = strSource.IndexOf(strMed, start) + strMed.Length;
                end = strSource.IndexOf(strEnd, med);
                if (start < med && med < end)
                    return true;

            }
            return false;
        }

        private static bool IsWordExist2(string strSource, string strFirst, string strSecond, string strThird, string strFourth)
        {
            if (strSource.Contains(strFirst) && strSource.Contains(strSecond) && strSource.Contains(strThird) && strSource.Contains(strFourth))
            {
                int first, second, third, fourth;
                first = strSource.IndexOf(strFirst, 0) + strFirst.Length;
                second = strSource.IndexOf(strSecond, first) + strSecond.Length;
                third = strSource.IndexOf(strThird, second) + strThird.Length;
                fourth = strSource.IndexOf(strFourth, third);
                if (first < second && second < third && third < fourth)
                    return true;

            }
            return false;
        }

        private static bool IsOneCharDiff(string strSource, string strStart, string strEnd)
        {
            if (strSource.ToLower().Contains(strStart) && strSource.ToLower().Contains(strEnd))
            {
                int start, end;
                start = strSource.IndexOf(" " + strStart, 0) + strStart.Length;
                end = strSource.IndexOf(strEnd, start);
                if (end - start == 2)
                    return true;
                
            }
            return false;
        }

        private static bool IsOneCharDiff1(string strSource, string strStart, string strMed, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strMed) && strSource.Contains(strEnd))
            {
                int start, med, end;
                start = strSource.IndexOf(strStart, 0) + strStart.Length;
                med = strSource.IndexOf(strMed, start) + strMed.Length;
                end = strSource.IndexOf(strEnd, med) + strEnd.Length;
                if (end - med == 2 && med - start == 2)
                    return true;

            }
            return false;
        }
        private static List<int> AllIndexesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(" " + value, index) + value.Length;
                if (index == -1)
                    return indexes;
                if (indexes != null && indexes.Count() > 0 && indexes.Last() > index)
                {
                    return indexes;
                }
                indexes.Add(index);

            }
        }
    }
}
