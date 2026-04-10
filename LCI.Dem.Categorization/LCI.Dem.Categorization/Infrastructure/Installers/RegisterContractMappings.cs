using LCI.Dem.Categorization.Contracts;
using LCI.Dem.Categorization.Data.DataManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LCI.Dem.Categorization.Infrastructure.Installers
{
    internal class RegisterContractMappings : IServiceRegistration
    {
        public void RegisterAppServices(IServiceCollection services, IConfiguration config)
        {
            //Register Interface Mappings for Repositories
            //services.AddTransient<ICategorizationManager, CategorizationManager>();
            //services.AddTransient<IProcessCourtManager, ProcessCourtManager>();
            //services.AddTransient<IKeywordScoringManager,KeywordScoringManager>();
            //services.AddTransient<IKeywordScoringProcess,KeywordScoringProcess>();
            //services.AddTransient<IUtility, Utility>();

            services.AddTransient<ICategorizationManager, CategorizationManager>();
            services.AddTransient<IProcessCourtManager, ProcessCourtManager>();
            services.AddTransient<IKeywordScoringManager, KeywordScoringManager>();
            services.AddTransient<IKeywordScoringProcess, KeywordScoringProcess>();
            services.AddTransient<IUtility, Utility>();
        }
    }
}
