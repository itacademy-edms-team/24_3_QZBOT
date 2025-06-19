using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Data.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// Получение списка всех пользователей
        /// </summary>
        /// <returns>Список IEnumerable пользователей</returns>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Получение объекта пользователя по его Id
        /// </summary>
        /// <param name="id">Id пользователя</param>
        /// <returns>Объект пользователя</returns>
        Task<User> GetByChatIdAsync(int id);

        /// <summary>
        /// Получение всех технологий, завершенных пользователем по его Id
        /// </summary>
        /// <param name="id">Id пользователя</param>
        /// <returns>Список IEnumerable технологий</returns>
        Task<IEnumerable<Technology>> GetAllCompletedTechnologiesByIdAsync(long id);

        /// <summary>
        /// Проверка существует ли пользователь в БД
        /// </summary>
        /// <param name="chatId">Id пользователя</param>
        /// <returns>True/False</returns>
        Task<bool> ExistsByChatIdAsync(long chatId);

        /// <summary>
        /// Добавление пользователя в БД
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <returns></returns>
        Task AddAsync(User user);

        /// <summary>
        /// Добавление записи о прохождении курса пользователем
        /// </summary>
        /// <param name="usersTechnologies">Объект </param>
        /// <returns></returns>
        Task AddFinishedTechnologyAsync(UsersTechnologies usersTechnologies);

        /// <summary>
        /// Проверка, прошел ли пользователь технологию
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="technologyId">Id технологии</param>
        /// <returns>True/False</returns>
        Task<bool> CheckFinishedTechnology(long userId, int technologyId);

        /// <summary>
        /// Проверка, является ли пользователь админом
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>True - админ/False - нет</returns>
        Task<bool> CheckUserAdmin(long userId);

        /// <summary>
        /// Получение списка технологий, доступных после прохождения определенной технологии по ее Id
        /// </summary>
        /// <param name="technologyId">Id технологии</param>
        /// <returns>Список IEnumerable технологий</returns>
        Task<IEnumerable<Technology>> GetNewTechnologiesByParentTechnologyId(int technologyId);

        /// <summary>
        /// Получение даты прохождения курса пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="technologyId">Id технологии</param>
        /// <returns>Дата прохождения</returns>
        Task<DateTime> GetDateOfFinishTechnologyByUserId(long userId, int technologyId);

        /// <summary>
        /// Обновление объекта пользователя
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <returns></returns>
        Task UpdateAsync(User user);

        /// <summary>
        /// Удаление пользователя и его прогресса из БД по его Id
        /// </summary>
        /// <param name="chatId">ChatId пользователя</param>
        /// <returns></returns>
        Task DeleteByChatIdAsync(long chatId);

        /// <summary>
        /// Удаление прогресса пользователя из БД
        /// </summary>
        /// <param name="chatId">Id пользователя</param>
        /// <returns></returns>
        Task DeleteProgressByChatIdAsync(long chatId);
    }
}
