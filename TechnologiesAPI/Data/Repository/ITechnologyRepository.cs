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
        /// <summary>
        /// Получение списка всех технологий
        /// </summary>
        /// <returns>Список IEnumerable всех технологий</returns>
        Task<IEnumerable<Technology>> GetAllAsync();

        /// <summary>
        /// Получение объекта технологии по его Id
        /// </summary>
        /// <param name="id">Id технологии</param>
        /// <returns>Объект технологии</returns>
        Task<Technology> GetByIdAsync(int id);

        /// <summary>
        /// Проверка на существование технологии по названию
        /// </summary>
        /// <param name="title">Название технологии</param>
        /// <returns>True - существует/False - нет</returns>
        Task<bool> CheckExistsTechnologyByTitle(string title);

        /// <summary>
        /// Получение списка технологий, пройденных пользователем по его Id
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Список IEnumerable технологий</returns>
        Task<IEnumerable<Technology>> GetFinishedTechnologiesByUserIdAsync(long userId);

        /// <summary>
        /// Получение списка технологий, доступных пользователю по его Id
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Список IEnumerable технологий</returns>
        Task<IEnumerable<Technology>> GetAvailableTechnologiesByUserIdAsync(long userId);

        /// <summary>
        /// Получение списка вопросов определенной технологии по ее Id
        /// </summary>
        /// <param name="id">Id технологии</param>
        /// <returns>Список IEnumerable вопросов</returns>
        Task<IEnumerable<Question>> GetAllQuestionsByTechnologyId(int id);

        /// <summary>
        /// Получение списка вопросов определенной технологии по ее названию
        /// </summary>
        /// <param name="name">Название технологии</param>
        /// <returns>Список вопросов</returns>
        Task<IEnumerable<Question>> GetAllQuestionsByTechnologyName(string name);

        /// <summary>
        /// Проверка доступа пользователя к определенной технологии по ее Id и UserId
        /// </summary>
        /// <param name="id">Id технологии</param>
        /// <param name="chatId">Id пользователя</param>
        /// <returns>True/False</returns>
        Task<bool> CheckTechnologyAccessById(int id, long chatId);

        /// <summary>
        /// Проверка доступа пользователя к определенной технологии по ее названию и UserId
        /// </summary>
        /// <param name="title">Название технологии</param>
        /// <param name="chatId">Id пользователя</param>
        /// <returns></returns>
        Task<bool> CheckTechnologyAccessByTitle(string title, long chatId);
        
        /// <summary>
        /// Получение Id всех предшествующих технологий по Id текущей технологии
        /// </summary>
        /// <param name="technologyId">Id технологии</param>
        /// <returns>Список List Id технологий</returns>
        Task<List<int>> GetAllParentTechnologyIdsAsync(int technologyId);

        /// <summary>
        /// Добавление технологии
        /// </summary>
        /// <param name="technology">Объект технологии</param>
        Task AddAsync(Technology technology);

        /// <summary>
        /// Добавление технологии из телеграма
        /// </summary>
        /// <param name="ParentTechnologyTitle">Название предшествующей технологии</param>
        /// <param name="technology">Объект технологии</param>
        /// <param name="questions">Список объектов вопросов</param>
        /// <returns></returns>
        Task AddFromTelegram(string ParentTechnologyTitle, Technology technology, List<Question> questions);

        /// <summary>
        /// Добавление технологии из телеграма
        /// </summary>
        /// <param name="ParentTechnologyId">Id предшествующей технологии</param>
        /// <param name="technology">Объект технологии</param>
        /// <param name="questions">Список объектов вопросов</param>
        /// <returns></returns>
        Task AddFromTelegram(int ParentTechnologyId, Technology technology, List<Question> questions);

        /// <summary>
        /// Обновление технологии
        /// </summary>
        /// <param name="technology">Объект технологии</param>
        Task UpdateAsync(Technology technology);

        /// <summary>
        /// Удаление технологии
        /// </summary>
        /// <param name="id">Id технологии</param>
        Task DeleteAsync(int id);
    }
}
