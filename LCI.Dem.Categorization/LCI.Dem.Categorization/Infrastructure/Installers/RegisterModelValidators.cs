using FluentValidation;
using LCI.Dem.Categorization.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LCI.Dem.Categorization.Infrastructure.Installers
{
    internal class RegisterModelValidators : IServiceRegistration
    {
        public void RegisterAppServices(IServiceCollection services, IConfiguration configuration)
        {
            //Register DTO Validators
            //services.AddTransient<IValidator<CreatePersonRequest>, CreatePersonRequestValidator>();
            //services.AddTransient<IValidator<UpdatePersonRequest>, UpdatePersonRequestValidator>();

            ////Disable Automatic Model State Validation built-in to ASP.NET Core
            //services.Configure<ApiBehaviorOptions>(opt => { opt.SuppressModelStateInvalidFilter = true; });
        }
    }
}
