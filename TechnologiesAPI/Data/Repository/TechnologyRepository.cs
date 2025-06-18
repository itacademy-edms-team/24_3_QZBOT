using Data.Repository.Helpers;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
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

        public async Task<int> GetIdByTitleAsync(string title)
        {
            return await _context.Technologies
                .Where(t => t.Title == title)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CheckExistsTechnologyByTitle(string title)
        {
            var tech = await _context.Technologies
            .Where(t => t.Title == title)
            .FirstOrDefaultAsync();

            return tech != null;
        }

        public async Task<string> CheckValidTechnology(Technology tech)
        {
            // проверка названия технологии
            if (await CheckExistsTechnologyByTitle(tech.Title))
            {
                return "Данная технология уже существует";
            }
            

            // проверка существования родительской технологии
            var parentId = await _context.Technologies
                .Where(t => t.Id == tech.ParentTechnologyId)
                .Select(t => t.Id)
                .FirstAsync();

            var parentTitle = await _context.Technologies
                .Where(t => t.Id == parentId)
                .Select(t => t.Title)
                .FirstAsync();

            if (!await CheckExistsTechnologyByTitle(parentTitle))
            {
                return "Такой родительской технологии не существует";
            }


            // проверка количества вопросов
            if (tech.Questions.Count < 5)
            {
                return "Недостаточно вопросов";
            }


            // проверка существования вопросов
            var questionsShortName = new List<string>();
            var questionsText = new List<string>();
            foreach (var quest in tech.Questions)
            {
                var qu = await _context.Questions
                    .Where(q => q.ShortName == quest.ShortName)
                    .Select(q => q.ShortName)
                    .FirstAsync();

                questionsShortName.Add(qu);


                var que = await _context.Questions
                    .Where(q => q.Text == quest.Text)
                    .Select(q => q.Text)
                    .FirstAsync();

                questionsText.Add(que);
            }
            if (questionsShortName.Count > 0)
            {
                return "Такое короткое название вопроса уже существует";
            }
            if (questionsText.Count > 0)
            {
                return "Такой вопрос уже существует";
            }


            // проверка вариантов ответов
            foreach (var quest in tech.Questions)
            {
                var haveTrue = false;
                foreach (var answer in quest.AnswerOption)
                {
                    if (haveTrue && answer.IsCorrect)
                    {
                        return $"Для вопроса '{quest.Text}' выбрано несколько правильных ответов, ошибка";
                    }

                    if (answer.IsCorrect)
                    {
                        haveTrue = true;
                    }
                }
                if (!haveTrue)
                {
                    return $"Для вопроса '{quest.Text}' не введен правильный вариант ответа, ошибка";
                }
            }



            return "true";
        }

        public async Task<IEnumerable<Technology>> GetFinishedTechnologiesByUserIdAsync(long userId)
        {
            var completedTechs = await _context.UsersTechnologies
                .Where(ut => ut.UserId == userId && ut.IsCompleted)
                .Select(ut => ut.Technology)
                .ToListAsync();

            if (!completedTechs.Any())
                return Enumerable.Empty<Technology>();

            var availableTechs = new HashSet<Technology>(new TechnologyComparer());

            foreach (var tech in completedTechs)
            {
                await AddAllChildTechnologiesRecursiveAsync(tech, availableTechs);
            }

            return availableTechs;
        }

        public async Task<IEnumerable<Technology>> GetAvailableTechnologiesByUserIdAsync(long userId)
        {
            var usId = _context.Users
                .Where(u => u.ChatId == userId)
                .Select(u => u.ChatId)
                .FirstOrDefault();

            var completedTechs = await _context.UsersTechnologies
                .Where(ut => ut.UserId == usId)
                .Select(ut => ut.TechnologyId)
                .ToListAsync();

            var technologies = await _context.Technologies
                .Where(t => ((t.ParentTechnologyId == null || completedTechs.Contains((int)t.ParentTechnologyId)) && !completedTechs.Contains(t.Id)))
                .ToListAsync();

            return technologies;
        }

        public async Task AddAllChildTechnologiesRecursiveAsync(Technology technology, HashSet<Technology> result)
        {
            if (result.Contains(technology, new TechnologyComparer()))
                return;

            result.Add(technology);

            var directChildren = await _context.Technologies
                .Where(t => t.ParentTechnologyId == technology.Id)
                .ToListAsync();

            foreach (var child in directChildren)
            {
                await AddAllChildTechnologiesRecursiveAsync(child, result);
            }
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
                .Include(q => q.AnswerOption)
                .Where(q => q.Technology.Title.ToLower() == name)
                .ToListAsync();
        }

        public async Task<bool> CheckTechnologyAccessByTitle(string title, long chatId)
        {
            var technology = await _context.Technologies
                .Where(t => t.Title == title)
                .Select(t => new { t.Id, t.ParentTechnologyId })
                .FirstOrDefaultAsync();

            if (technology == null)
                return false;

            return await CheckTechnologyAccessById(technology.Id, chatId);
        }

        public async Task<List<int>> GetAllParentTechnologyIdsAsync(int technologyId)
        {
            var parentIds = new List<int>();

            int? currentId = technologyId;

            while (currentId.HasValue)
            {
                var technology = await _context.Technologies
                    .Where(t => t.Id == currentId.Value)
                    .Select(t => new { t.ParentTechnologyId })
                    .FirstOrDefaultAsync();

                if (technology?.ParentTechnologyId != null)
                {
                    parentIds.Add(technology.ParentTechnologyId.Value);
                    currentId = technology.ParentTechnologyId;
                }
                else
                {
                    currentId = null;
                }
            }

            return parentIds;
        }

        public async Task<bool> CheckTechnologyAccessById(int id, long chatId)
        {
            var technology = await _context.Technologies
                .Where(t => t.Id == id)
                .Select(t => new { t.ParentTechnologyId })
                .FirstOrDefaultAsync();

            if (technology == null)
                return false;

            var parentTechIds = await GetAllParentTechnologyIdsAsync(id);

            bool allParentsCompleted = true;

            foreach (var parentId in parentTechIds)
            {
                bool isCompleted = await _context.UsersTechnologies
                    .Where(ut => ut.UserId == chatId && ut.TechnologyId == parentId)
                    .AnyAsync(ut => ut.IsCompleted);

                if (!isCompleted)
                {
                    allParentsCompleted = false;
                    break;
                }
            }

            return allParentsCompleted;
        }

        public async Task AddAsync(Technology technology)
        {
            await _context.Technologies.AddAsync(technology);
            await _context.SaveChangesAsync();
        }

        public async Task AddFromTelegram(string parentTechnologyTitle, Technology technology, List<Question> questions)
        {
            var tech = new Technology();
            tech = technology;

            if (parentTechnologyTitle == null)
            {
                tech.ParentTechnologyId = null;
            }
            else
            {
                var parentTechId = await _context.Technologies
                .Where(t => t.Title == parentTechnologyTitle)
                .Select(t => t.Id)
                .FirstAsync();

                tech.ParentTechnologyId = parentTechId;
            }

            await _context.AddAsync(tech);

            foreach (var question in questions)
            {
                tech.Questions.Add(question);
            }
            await _context.SaveChangesAsync();
        }

        public async Task AddFromTelegram(int parentTechnologyId, Technology technology, List<Question> questions)
        {
            var tech = new Technology();
            tech = technology;

            tech.ParentTechnologyId = parentTechnologyId;

            await _context.Technologies.AddAsync(tech);

            foreach (var question in questions)
            {
                tech.Questions.Add(question);
            }
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
