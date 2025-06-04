using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    public interface IQuestionRepository
    {
        /// <summary>
        /// Вывод списка всех вопросов
        /// </summary>
        /// <returns>Список IEnumerable из Question</returns>
        Task<IEnumerable<Question>> GetAllAsync();

        /// <summary>
        /// Получение списка всех вопросов по названию технологии
        /// </summary>
        /// <param name="technologyName">Название технологии</param>
        /// <returns>Список IEnumerable из Question</returns>
        Task<IEnumerable<Question>> GetAllByTechnologyName(string technologyName);

        /// <summary>
        /// Проверка на существование вопроса по его тексту
        /// </summary>
        /// <param name="text">Текст вопроса</param>
        /// <returns>True/False</returns>
        Task<bool> CheckExistsQuestionByText(string text);

        /// <summary>
        /// Проверка на существование вопроса по его короткому названию
        /// </summary>
        /// <param name="shortName">Короткое название</param>
        /// <returns>True - существует/False - нет</returns>
        Task<bool> CheckExistsQuestionByShortName(string shortName);

        /// <summary>
        /// Получение определенного вопроса по его Id
        /// </summary>
        /// <param name="id">Id вопроса</param>
        /// <returns>Вопрос по технологии</returns>
        Task<Question> GetByIdAsync(int id);

        /// <summary>
        /// Получение определенного вопроса по его короткому названию
        /// </summary>
        /// <param name="shortName">Короткое название вопроса</param>
        /// <returns>Вопрос по технологии</returns>
        Task<Question> GetByShortName(string shortName);

        /// <summary>
        /// Добавление вопроса в БД
        /// </summary>
        /// <param name="question">Объект вопроса</param>
        Task AddAsync(Question question);

        /// <summary>
        /// Обновление определенного вопроса
        /// </summary>
        /// <param name="question">Объект вопроса</param>
        /// <returns></returns>
        Task UpdateAsync(Question question);

        /// <summary>
        /// Удаление вопроса из БД по его Id
        /// </summary>
        /// <param name="id">Id вопроса</param>
        Task DeleteAsync(int id);
    }
}
