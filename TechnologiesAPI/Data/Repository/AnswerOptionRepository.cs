using Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    public class AnswerOptionRepository : IAnswerOptionRepository
    {
        private readonly AppDbContext _context;

        public AnswerOptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnswerOption>> GetAllAsync()
        {
            return await _context.AnswerOptions.ToListAsync();
        }

        public async Task<IEnumerable<AnswerOption>> GetAllByQuestionId(int id)
        {
            return await _context.AnswerOptions
                .Where(x => x.QuestionId == id)
                .ToListAsync();
        }

        public async Task<AnswerOption> GetByIdAsync(int id)
        {
            return await _context.AnswerOptions
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(AnswerOption answerOption)
        {
            await _context.AnswerOptions.AddAsync(answerOption);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AnswerOption answerOption)
        {
            _context.AnswerOptions.Update(answerOption);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var answerOption = await _context.AnswerOptions.FindAsync(id);
            if (answerOption != null)
            {
                _context.AnswerOptions.Remove(answerOption);
                await _context.SaveChangesAsync();
            }
        }
    }
}
