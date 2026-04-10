using LCI.Dem.Categorization.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Workers
{
    public class CategorizationServiceRerun : BackgroundService
    {
        /// <summary>
        /// logger for <see cref="CategorizationService"/>
        /// </summary>
        private readonly ILogger<CategorizationServiceRerun> _logger;

        private readonly IServiceProvider _services;

        private readonly IProcessCourtManager _processCourtManager;

        public CategorizationServiceRerun(ILogger<CategorizationServiceRerun> logger, IServiceProvider services, IProcessCourtManager processCourtManager)
        {
            _logger = logger;
            _services = services;
            _processCourtManager = processCourtManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            _logger.LogInformation($"CategorizationServiceRerun is starting.");

            stoppingToken.Register(() => _logger.LogInformation($" CategorizationServiceRerun background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"CategorizationServiceRerun task doing background work.");

                //Gt All Process Court
                var getProcessCourt = await _processCourtManager.GetProcessCourtWithFL();

                //Pick with Status FL
                var lst_job_Id = getProcessCourt.Where(x => x.CategorizationStatus == "FL" && x.Job_Id_Num != null);

                //Take one at Time
                var job_p = lst_job_Id.Take(1);

                // Select Distinct this is not required but in multi concurrency may cause break
                var lstJobId = job_p.Select(x => x.Job_Id_Num).Distinct();

                if (lstJobId.Any()) 
                {
                    using (var scope = _services.CreateScope())
                    {
                        //Create scoped called
                        var _categorizationManager = scope.ServiceProvider.GetRequiredService<ICategorizationManager>();

                        await _categorizationManager.CleanAndRerun(lstJobId.FirstOrDefault(), true);

                        lstJobId.ToList().ForEach(async x => await _processCourtManager.UpdateProcessCourtStatus(x, "WT"));
                    }
                }                
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation($"CategorizationServiceRerun background task is stopping.");
        }
    }
}
