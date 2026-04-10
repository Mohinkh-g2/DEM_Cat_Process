using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LCI.Dem.Categorization.Contracts
{
    public interface IServiceRegistration
    {
        void RegisterAppServices(IServiceCollection services, IConfiguration configuration);
    }
}
