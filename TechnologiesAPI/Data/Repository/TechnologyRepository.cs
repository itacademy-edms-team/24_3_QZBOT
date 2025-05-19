using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    public class TechnologyRepository : ITechnologyRepository
    {
        private readonly AppDbContext _context;

        public TechnologyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Technology>> GetAllAsync()
        {
            return await _context.Technologies.ToListAsync();
        }

        public async Task<Technology> GetByIdAsync(int id)
        {
            return await _context.Technologies
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsByTechnologyId(int id)
        {
            return await _context.Questions
                .Where(x => x.TechnologyId == id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsByTechnologyName(string name)
        {
            return await _context.Questions
                .Include(q => q.Technology)
                .Where(q => q.Technology.Title.ToLower() == name)
                .ToListAsync();
        }

        public async Task AddAsync(Technology technology)
        {
            await _context.Technologies.AddAsync(technology);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Technology technology)
        {
            _context.Technologies.Update(technology);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var technology = await _context.Technologies.FindAsync(id);
            if (technology != null)
            {
                _context.Technologies.Remove(technology);
                await _context.SaveChangesAsync();
            }
        }
    }
}
