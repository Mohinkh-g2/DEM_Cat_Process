using LCI.Dem.Categorization.Data;
using LCI.Dem.Categorization.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCI.Dem.Categorization.Contracts
{
    public interface IPersonManager : IRepository<Person>
    {
        Task<(IEnumerable<Person> Persons, Pagination Pagination)> GetPersonsAsync(UrlQueryParameters urlQueryParameters);

        //Add more class specific methods here when neccessary
    }
}
