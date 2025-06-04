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
        /// <summary>
        /// Получение списка IEnumerable из всех вариантов ответа
        /// </summary>
        /// <returns>Список вариантов ответа IEnumerable</returns>
        Task<IEnumerable<AnswerOption>> GetAllAsync();

        /// <summary>
        /// Получение списка IEnumerable всех вариантов ответа для определенного вопроса по Id этого вопроса
        /// </summary>
        /// <param name="id">Id вопроса</param>
        /// <returns>Список IEnumerable вариантов ответа</returns>
        Task<IEnumerable<AnswerOption>> GetAllByQuestionId(int id);

        /// <summary>
        /// Получение списка IEnumerable всех вариантов ответа для определенного вопроса по краткому названию этого вопроса
        /// </summary>
        /// <param name="shortName">Короткое название вопроса</param>
        /// <returns>Список IEnumerable вариантов ответа</returns>
        Task<IEnumerable<AnswerOption>> GetAllByQuestionShortName(string shortName);

        /// <summary>
        /// Получение объекта варианта ответа по его Id
        /// </summary>
        /// <param name="id">Id варианта ответа</param>
        /// <returns>Объект варианта ответа</returns>
        Task<AnswerOption> GetByIdAsync(int id);

        /// <summary>
        /// Добавление объекта варианта ответа в БД
        /// </summary>
        /// <param name="answerOption">Объект варианта ответа</param>
        Task AddAsync(AnswerOption answerOption);

        /// <summary>
        /// Обновление объекта варианта ответа
        /// </summary>
        /// <param name="answerOption"></param>
        Task UpdateAsync(AnswerOption answerOption);

        /// <summary>
        /// Удаление объекта варианта ответа по его Id
        /// </summary>
        /// <param name="id">Id варианта ответа</param>
        Task DeleteAsync(int id);
    }
}
