using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetByChatIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(t => t.ChatId == id);
        }

        public async Task<IEnumerable<Technology>> GetAllCompletedTechnologiesByIdAsync(long id)
        {
            return await _context.UsersTechnologies
                    .Where(ut => ut.UserId == id && ut.IsCompleted)
                    .Select(ut => ut.Technology)
                    .ToListAsync();
        }

        public async Task<bool> ExistsByChatIdAsync(long chatId)
        {
            return await _context.Users
                .AnyAsync(u => u.ChatId == chatId);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task AddFinishedTechnologyAsync(UsersTechnologies technologies)
        {
            await _context.UsersTechnologies.AddAsync(technologies);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckFinishedTechnology(long userId, int technologyId)
        {
            var technologies = await _context.UsersTechnologies
                .Where(ut => ut.UserId == userId)
                .Where(ut => ut.TechnologyId == technologyId)
                .FirstOrDefaultAsync();

            if (technologies != null)
            {
                return true;
            }
            else return false;
        }

        public async Task<bool> CheckUserAdmin(long userId)
        {
            var admin = await _context.Admins
                .Where(a => a.UserId == userId)
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                return false;
            }
            else return true;
        }

        public async Task<IEnumerable<Technology>> GetNewTechnologiesByParentTechnologyId(int technologyId)
        {
            return await _context.Technologies
                .Where(t => t.ParentTechnologyId == technologyId)
                .ToListAsync();
        }
        public async Task<DateTime> GetDateOfFinishTechnologyByUserId(long userId, int technologyId)
        {
            if (await CheckFinishedTechnology(userId, technologyId))
            {
                var dates = await _context.UsersTechnologies
                .Where(ut => ut.UserId == userId && ut.TechnologyId == technologyId)
                .Select(ut => ut.CompletedAt)
                .FirstAsync();
                
                return dates;
            }
            else
            {
                return new DateTime();
            }
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteByChatIdAsync(long chatId)
        {
            var users = await _context.Users
                .Where(u => u.ChatId == chatId)
                .ToListAsync(); // FirstAsync

            // тут очень жесткий костыль из-за
            // невозможности достать из контекста один объект

            var user = users[0];

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var progress = await _context.UsersTechnologies
                .Where(ut => ut.UserId == chatId)
                .ToListAsync();

            foreach (var pr in progress)
            {
                _context.UsersTechnologies.Remove(pr);
                await _context.SaveChangesAsync();
            }
        }
    }
}
