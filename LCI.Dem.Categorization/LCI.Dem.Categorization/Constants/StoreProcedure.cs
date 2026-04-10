using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Constants
{
    public class StoreProcedure
    {
        public const string Create_Trigger_Stat_Entry = "[darpRPT].[Create_Trigger_Stat_Entry]";

        public const string Create_Batch_Stat_Entry = "[darpRPT].[Create_Batch_Stat_Entry]";

        public const string Batch_Step_Start = "[darpRPT].[Batch_Step_Start]";

        public const string StagingMethods = "[darp].[StagingMethods]";
        
        public const string Batch_Step_Complete = "[darpRPT].[Batch_Step_Complete]";

        public const string Update_Batch_Stat_Entry = "[darpRPT].[Update_Batch_Stat_Entry]";

        public const string KeywordScoring="[darp].[KeywordScoring]";

        public const string KeywordScoringOnAllDocketEntries = "[darp].[KeywordScoringOnAllDocketEntries]";

        public const string PerformEventScoring_LevelWise = "[darp].[PerformEventScoring_LevelWise]";

        public const string Perform_BNCParentEventMining = "[darp].[Perform_BNCParentEventMining]";

        public const string ArchiveScoringTables = "[darp].[ArchiveScoringTables]";

        public const string PerformDateMining = "[darp].[PerformDateMining]";

        public const string CollectAndInsertAdversaryProceedings = "[darp].[CollectAndInsertAdversaryProceedings]";

        public const string Perform_DangerMISC_DeDupe = "[darp].[Perform_DangerMISC_DeDupe]";

        public const string POIScoringOnAllDocketEntries = "[darpPOI].[POIScoringOnAllDocketEntries]";

        public const string ArchiveScoringTables_ForPOI ="[darpPOI].[ArchiveScoringTables_ForPOI]";

        public const string OffStageTheBatchOfDocketTexts ="[darp].[OffStageTheBatchOfDocketTexts]";

        public const string Update_Trigger_Stat_Entry = "[darpRPT].[Update_Trigger_Stat_Entry]";

        public const string Print = "[darp].[Print]";

        public const string Populate_Dumb_MISC = "[darp].[Populate_Dumb_MISC]";

        public const string FillRegExForKeywords = "[darp].[FillRegExForKeywords]";

        //Keyword Scoring Store Procedures
        public const string CreateDocketKeywordScore = "[darpRPT].[CreateDocketKeywordScore]";

        public const string CreateDocketKeywordScoreByLevels = "[darpRPT].[CreateDocketKeywordScoreByLevels]";

        public const string CreateDocketKeywordScoreCompleteByLevels = "[darpRPT].[CreateDocketKeywordScoreCompleteByLevels]";

        public const string CreateDocketEventScore = "[darpRPT].[CreateDocketEventScore]";

        public const string CleanAndRerun = "[dbo].[CleanDEMCategorization]";

        public const string USP_DEM_Monitoring_ProcessCourt_Status = "[USP_DEM_Monitoring_ProcessCourt_Status]";

        public const string USP_DEM_EXTRACT_CASE_DATES = "[Usp_Extract_Case_Dates]";

        public const string USP_Get_Process_Court = "[USP_Get_Process_Court]"
;


    }
}
