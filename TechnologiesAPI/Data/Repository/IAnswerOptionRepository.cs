using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    public interface IAnswerOptionRepository
    {
        Task<IEnumerable<AnswerOption>> GetAllAsync();
        Task<IEnumerable<AnswerOption>> GetAllByQuestionId(int id);
        Task<AnswerOption> GetByIdAsync(int id);
        Task AddAsync(AnswerOption answerOption);
        Task UpdateAsync(AnswerOption answerOption);
        Task DeleteAsync(int id);
    }
}
