using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Contracts
{
    public interface IAuthServerConnect
    {
        Task<string> RequestClientCredentialsTokenAsync();
    }
}
