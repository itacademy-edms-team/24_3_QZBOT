using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Data.Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Question>> GetAllAsync()
        {
            return await _context.Questions.ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetAllByTechnologyName(string technologyName)
        {
            return await _context.Questions
                .Include(x => x.Technology)
                .Where(x => x.Technology.Title.ToLower() == technologyName.ToLower())
                .ToListAsync();
        }

        public async Task<bool> CheckExistsQuestionByText(string text)
        {
            var quest = await _context.Questions
                .Where(t => t.Text == text)
                .FirstOrDefaultAsync();

            return quest != null;
        }

        public async Task<bool> CheckExistsQuestionByShortName(string shortName)
        {
            var quest = await _context.Questions
                .Where(t => t.ShortName == shortName)
                .FirstOrDefaultAsync();

            return quest != null;
        }

        public async Task<Question> GetByIdAsync(int id)
        {
            return await _context.Questions
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Question> GetByShortName(string shortName)
        {
            return await _context.Questions
                .FirstOrDefaultAsync(t => t.ShortName == shortName);
        }

        public async Task AddAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }
    }
}
