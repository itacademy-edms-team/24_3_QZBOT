using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Data.Repository
{
    public interface ITechnologyRepository
    {
        Task<IEnumerable<Technology>> GetAllAsync();
        Task<Technology> GetByIdAsync(int id);
        Task<IEnumerable<Question>> GetAllQuestionsByTechnologyId(int id);
        Task<IEnumerable<Question>> GetAllQuestionsByTechnologyName(string name);
        Task AddAsync(Technology technology);
        Task UpdateAsync(Technology technology);
        Task DeleteAsync(int id);
    }
}
