using AspNetCoreRateLimit;
using LCI.Dem.Categorization.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LCI.Dem.Categorization.Infrastructure.Installers
{
    internal class RegisterRequestRateLimiter : IServiceRegistration
    {
        public void RegisterAppServices(IServiceCollection services, IConfiguration config)
        {
            // needed to load configuration from appsettings.json
            //services.AddOptions();
            //// needed to store rate limit counters and ip rules
            //services.AddMemoryCache();

            ////load general configuration from appsettings.json
            //services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
            //services.Configure<IpRateLimitPolicies>(config.GetSection("IpRateLimitPolicies"));

            //// inject counter and rules stores
            //services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            //services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            //services.AddInMemoryRateLimiting();


            ////If you load - balance your app, you'll need to use IDistributedCache with Redis or SQLServer
            ////so that all kestrel instances will have the same rate limit store. Instead of the in-memory
            ////stores you should inject the distributed stores like this:
            //// inject counter and rules distributed cache stores
            ////services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
            ////services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();

            //// https://github.com/aspnet/Hosting/issues/793
            //// the IHttpContextAccessor service is not registered by default.
            //// the clientId/clientIp resolvers use it.
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //// configuration (resolvers, counter key builders)
            //services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }
    }
}
