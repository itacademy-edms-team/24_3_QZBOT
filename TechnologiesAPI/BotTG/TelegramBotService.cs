using Azure.Core;
using Data;
using Data.Repository;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Models;
using System.ComponentModel;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace BotTG
{
    public class TelegramBotService : BackgroundService
    {
        private readonly ILogger<TelegramBotService> _logger;
        private readonly TelegramBotClient _bot;
        private readonly CancellationTokenSource _cts;
        private readonly IServiceProvider _serviceProvider;

        private static readonly Dictionary<long, UserLocalState> _userStates = new();
        private static readonly Dictionary<long, SuperUser> _superUsers = new();
        private static Dictionary<long, AdminState> _adminStates = new();
        private static Dictionary<long, CourseInput> tempCourses = new();
        private static readonly object _lock = new();

        public TelegramBotService(IServiceProvider serviceProvider, ILogger<TelegramBotService> logger)
        {
            _logger = logger;
            var envPath = Path.Combine(AppContext.BaseDirectory, "BotTG", ".env");
            Env.Load(envPath);
            string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            _cts = new CancellationTokenSource();
            _bot = new TelegramBotClient(botToken, cancellationToken: _cts.Token);
            _serviceProvider = serviceProvider;

            var recieverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            _bot.StartReceiving(
                    async (botClient, update, token) => await HandleUpdateAsync(botClient, update, token),
                    async (botClient, exception, token) => await HandleErrorAsync(botClient, exception, token),
                    recieverOptions, 
                    _cts.Token
                );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var me = await _bot.GetMe();
                _logger.LogInformation($"@{me.Username} is running...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running the bot");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            await base.StopAsync(cancellationToken);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var techRepo = scope.ServiceProvider.GetRequiredService<ITechnologyRepository>();
            var answerRepo = scope.ServiceProvider.GetRequiredService<IAnswerOptionRepository>();
            var questionRepo = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            try
            {
                if (update.Type == UpdateType.Message)
                {
                    var msg = update.Message;

                    if (msg.Text != null)
                    {
                        if (msg.Text == "/start")
                        {
                            if (await userRepo.ExistsByChatIdAsync(msg.Chat.Id))
                            {
                                var listOfAvailableTechnologies = techRepo.GetAvailableTechnologiesByUserIdAsync(msg.Chat.Id);
                                var buttonsTechnologies = GetTechnologiesButton(await listOfAvailableTechnologies);

                                await botClient.SendMessage(
                                    chatId: msg.Chat.Id,
                                    text: $"Привет, {msg.Chat.FirstName}! Вот курсы, доступные тебе, выбирай",
                                    replyMarkup: buttonsTechnologies
                                );
                            }
                            else
                            {
                                // здесь сделано немного через костыль: метод проверяет курсы,
                                // доступные пользователю, заведомо зная, что его нет в БД

                                var listOfAvailableTechnologies = techRepo.GetAvailableTechnologiesByUserIdAsync(msg.Chat.Id);
                                var buttonsTechnologies = GetTechnologiesButton(await listOfAvailableTechnologies);

                                await botClient.SendMessage(
                                        chatId: msg.Chat.Id,
                                        text: $"Добро пожаловать! Я квиз-бот с вопросами по различным технологиям. Вот курсы начального уровня, выбирай!",
                                        replyMarkup: buttonsTechnologies
                                    );

                                var fullname = $"{msg.Chat.FirstName} {msg.Chat.LastName}";
                                var user = new Models.User
                                {
                                    ChatId = msg.Chat.Id,
                                    FullName = fullname,
                                    FirstName = msg.Chat.FirstName,
                                    LastName = msg.Chat.LastName
                                };

                                await userRepo.AddAsync(user);
                            }
                            
                        }
                        else if (msg.Text == "/checkdb")
                        {
                            if (await userRepo.ExistsByChatIdAsync(msg.Chat.Id))
                            {
                                await botClient.SendMessage(
                                        chatId: msg.Chat.Id,
                                        text: $"Привет, {msg.Chat.FirstName}! Ты уже есть в базе данных"
                                    );
                            }
                            else
                            {
                                await botClient.SendMessage(
                                        chatId: msg.Chat.Id,
                                        text: $"Привет, {msg.Chat.FirstName}! Тебя нет в нашей базе данных"
                                    );

                                // код ниже - добавление пользователя в БД

                                //var fullname = $"{msg.Chat.FirstName} {msg.Chat.LastName}";
                                //var user = new Models.User
                                //{
                                //    ChatId = msg.Chat.Id,
                                //    FullName = fullname,
                                //    FirstName = msg.Chat.FirstName,
                                //    LastName = msg.Chat.LastName
                                //};

                                //await userRepo.AddAsync(user);


                                //await botClient.SendMessage(
                                //        chatId: msg.Chat.Id,
                                //        text: $"Привет, {msg.Chat.FirstName}! Ты внесен в базу данных"
                                //    );
                            }
                        }
                        else if (msg.Text == "/deleteprogress")
                        {
                            await userRepo.DeleteByChatIdAsync(msg.Chat.Id);

                            await botClient.SendMessage(
                                    chatId: msg.Chat.Id,
                                    text: $"Ваш прогресс удален"
                                );
                        }

                        else if (msg.Text == "/checkcources")
                        {
                            var technologies = await userRepo.GetAllCompletedTechnologiesByIdAsync(msg.Chat.Id);

                            var str = "Курсы, пройденные вами:\n";
                            int count = 1;
                            foreach (var te in technologies)
                            {
                                var date = await userRepo.GetDateOfFinishTechnologyByUserId(msg.Chat.Id, te.Id);

                                str += $"{count}. {te.Title}. Дата прохождения: {date.AddHours(7)}\n";
                                count++;
                            }
                            
                            if (technologies.Count() == 0)
                            {
                                await botClient.SendMessage(
                                        chatId: msg.Chat.Id,
                                        text: "Вы еще не прошли ни одного курса",
                                        cancellationToken: cancellationToken
                                    );
                                return;
                            }

                            await botClient.SendMessage(
                                    chatId: msg.Chat.Id,
                                    text: str,
                                    cancellationToken: cancellationToken
                                );
                        }

                        // админская панель
                        else if (msg.Text == "/addcourse")
                        {
                            _adminStates[msg.From.Id] = AdminState.WaitingForCourseName;
                            var newTechnology = new CourseInput();

                            await botClient.SendMessage(
                                        chatId: msg.Chat.Id,
                                        text: "Вы вошли в процедуру добавления курса\n" +
                                        "Важно: <b>не пишите ничего лишнего</b>.\nДля выхода из процедуры отправьте /stop\n" +
                                        "1. Отправьте название технологии",
                                        parseMode: ParseMode.Html
                                    );

                            //var button1 = InlineKeyboardButton.WithCallbackData("Добавить", "add_technology");
                            //var button2 = InlineKeyboardButton.WithCallbackData("Редактировать", "edit_technology");

                            //var butRow = new[] { button1, button2 };
                            //var markup = new InlineKeyboardMarkup(new[] { butRow });

                            //await botClient.SendMessage(
                            //        chatId: msg.Chat.Id,
                            //        text: "Вы открыли админскую панель",
                            //        replyMarkup: markup,
                            //        cancellationToken: cancellationToken
                            //    );

                            //if (_superUsers.TryGetValue(msg.Chat.Id, out var supUser))
                            //{
                            //    supUser.SecretWordCount++;

                            //    var button1 = InlineKeyboardButton.WithCallbackData("Добавить", "callback_1");
                            //    var button2 = InlineKeyboardButton.WithCallbackData("Редактировать", "callback_2");

                            //    var butRow = new[] {button1, button2 };
                            //    var markup = new InlineKeyboardMarkup(new[] { butRow });

                            //    if (supUser.SecretWordCount == 10)
                            //    {
                            //        await botClient.SendMessage(
                            //                chatId: msg.Chat.Id,
                            //                text: "Вы открыли админскую панель",
                            //                replyMarkup: markup
                            //            );
                            //    }
                            //}
                            //else
                            //{
                            //    SuperUser superUser = new SuperUser()
                            //    {
                            //        SecretWordCount = 0,
                            //    };
                            //    _superUsers.Add(msg.Chat.Id, superUser);
                            //}

                            //await botClient.DeleteMessage(msg.Chat.Id, msg.MessageId, cancellationToken);
                        }
                        // админская панель

                        else if (_adminStates.ContainsKey(msg.From.Id))
                        {
                            if (msg.Text == "/stop")
                            {
                                await botClient.SendMessage(
                                        chatId: msg.From.Id,
                                        text: "Добавление курса отменено"
                                    );

                                _adminStates.Remove(msg.From.Id);

                                return;
                            }
                            else if (msg.Text == "/go")
                            {
                                var currentCourse = tempCourses[msg.From.Id];

                                if (string.IsNullOrWhiteSpace(currentCourse.Title))
                                {
                                    await botClient.SendMessage(
                                            chatId: msg.Chat.Id, 
                                            text: "Данные не заполнены! Отправка не удалась, операция прервана"
                                        );
                                    return;
                                }

                                if (currentCourse.Questions.Count == 0)
                                {
                                    await botClient.SendMessage(
                                            chatId: msg.Chat.Id, 
                                            text: "Данные не заполнены! Отправка не удалась, операция прервана"
                                        );
                                    return;
                                }

                                foreach (var question in currentCourse.Questions)
                                {
                                    if (question.AnswerOption.Count == 0)
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.Chat.Id, 
                                                text: "Данные не заполнены! Отправка не удалась, операция прервана"
                                            );
                                        return;
                                    }
                                }



                                var input = tempCourses[msg.From.Id];

                                var parentTechTitle = tempCourses[msg.From.Id].TitleOfParentCourse;

                                var technology = new Technology()
                                {
                                    Title = input.Title                                    
                                };

                                var questions = new List<Question>();

                                foreach (var question in input.Questions)
                                {
                                    questions.Add(question);
                                }


                                await botClient.SendMessage(
                                        chatId: msg.From.Id,
                                        text: $"Данные отправлены"
                                    );

                                await techRepo.AddFromTelegram(parentTechTitle, technology, questions);

                                _adminStates.Remove(msg.From.Id);
                                return;
                            }

                                switch (_adminStates[msg.From.Id])  
                                {
                                    case AdminState.WaitingForCourseName:
                                        {
                                            tempCourses[msg.From.Id] = new CourseInput();
                                            string courseName = msg.Text;

                                            if (await techRepo.CheckExistsTechnologyByTitle(courseName))
                                            {
                                                await botClient.SendMessage(
                                                        chatId: msg.From.Id,
                                                        text: $"Курс '{courseName}' уже существует. Попробуйте еще раз"
                                                    );

                                                //_adminStates.Remove(msg.From.Id);
                                            }
                                            else
                                            {
                                                tempCourses[msg.From.Id].Title = courseName;
                                                _adminStates[msg.From.Id] = AdminState.WaitingForParentCourseName;
                                                await botClient.SendMessage(
                                                        chatId: msg.From.Id,
                                                        text: $"Название курса '{courseName}' сохранено. Введите название предшествующей технологии (если такой нет, введите 'no'):"
                                                    );

                                                tempCourses[msg.From.Id].Questions = new List<Question>();
                                            }

                                            break;
                                        }

                                    case AdminState.WaitingForParentCourseName:
                                        
                                        string parentTech = msg.Text;
                                        if (parentTech == "no")
                                        {
                                            tempCourses[msg.From.Id].TitleOfParentCourse = null;

                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: "Курс будет для начинающих\nВведите текст первого вопроса"
                                                );

                                            _adminStates[msg.From.Id] = AdminState.WaitingForQuestion;
                                        }

                                        else if (await techRepo.CheckExistsTechnologyByTitle(parentTech))
                                        {
                                            tempCourses[msg.From.Id].TitleOfParentCourse = parentTech;
                                            var nameOfCourse = tempCourses[msg.From.Id].Title;

                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: $"Курс '{nameOfCourse}' успешно прикреплен к курсу '{parentTech}'" +
                                                    $"\nВведите текст первого вопроса"
                                                );

                                            _adminStates[msg.From.Id] = AdminState.WaitingForQuestion;
                                        }
                                        else
                                        {
                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: $"Технологии '{parentTech}' не существует. Введите заново"
                                                );

                                            //_adminStates.Remove(msg.From.Id);
                                            //return;
                                        }

                                        break;
                                        

                                    case AdminState.WaitingForQuestion:
                                        {
                                            string question = msg.Text;

                                            if (await questionRepo.CheckExistsQuestionByText(question))
                                            {
                                                await botClient.SendMessage(
                                                        chatId: msg.From.Id,
                                                        text: "Такой вопрос уже существует. Введите заново"
                                                    );

                                                //_adminStates.Remove(msg.From.Id);
                                                break;
                                            }

                                            if (question[question.Length - 1] != '?') // question.Contains("?")
                                            {
                                                question += "?";
                                            }

                                            var quest = new Question();
                                            //tempCourses[msg.From.Id].Questions.Add(quest);

                                            tempCourses[msg.From.Id].Questions.Add(quest);

                                            tempCourses[msg.From.Id].Questions.Last().Text = question;

                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: $"Вопрос '{question}' успешно добавлен. Введите короткое " +
                                                    $"название вопроса (например 'variablesPython')"
                                                );

                                            _adminStates[msg.From.Id] = AdminState.WaitingForShortNameQuestion;
                                            break;
                                        }


                                    case AdminState.WaitingForShortNameQuestion:
                                        {
                                            string shortName = msg.Text;

                                            if (await questionRepo.CheckExistsQuestionByShortName(shortName))
                                            {
                                                await botClient.SendMessage(
                                                        chatId: msg.From.Id,
                                                        text: $"Короткое название '{shortName}' уже существует. Введите заново"
                                                    );

                                                //_adminStates.Remove(msg.From.Id);
                                                break;
                                            }

                                            string questionText = tempCourses[msg.From.Id].Questions.Last().Text;

                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: $"Короткое название '{shortName}' успешно добавлено.\n" +
                                                    $"Введите варианты ответов на вопрос '{questionText}' через запятую без пробелов"
                                                );

                                            tempCourses[msg.From.Id].Questions.Last().ShortName = shortName;

                                            _adminStates[msg.From.Id] = AdminState.WaitingForAnswers;
                                            break;
                                        }

                                    case AdminState.WaitingForAnswers:
                                        {
                                            string textOfAnswers = msg.Text;

                                            if (!textOfAnswers.Contains(","))
                                            {
                                                await botClient.SendMessage(
                                                        chatId: msg.From.Id,
                                                        text: $"Нельзя внести только один вариант ответа. Операция прервана"
                                                    );

                                                //_adminStates.Remove(msg.From.Id);
                                                break;
                                            }

                                            string[] Answers = textOfAnswers.Split(",");

                                            var lastQuestion = tempCourses[msg.From.Id].Questions.Last();

                                            lastQuestion.AnswerOption.Clear();

                                            foreach (var answer in Answers)
                                            {
                                                lastQuestion.AnswerOption.Add(new AnswerOption { Text = answer });
                                            }


                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: $"Варианты ответов '{textOfAnswers}' успешно внесены, " +
                                                    $"теперь отправьте номер правильного варианта ответа среди них"
                                                );

                                            _adminStates[msg.From.Id] = AdminState.WaitingForRightAnswer;
                                            break;
                                        }

                                    case AdminState.WaitingForRightAnswer:
                                        {
                                            //var allAnsw = tempCourses[msg.From.Id].Questions.Last().AnswerOption;
                                            //var strAllAnsw = "";

                                            //foreach (var answer in allAnsw)
                                            //{
                                            //    strAllAnsw += answer;
                                            //}

                                            //var rightAnsw = msg.Text;

                                            //if (!strAllAnsw.Contains(rightAnsw))
                                            //{
                                            //    await botClient.SendMessage(
                                            //            chatId: msg.From.Id,
                                            //            text: $"Правильного варианта ответа нет среди этих: {strAllAnsw}. Операция прервана"
                                            //        );

                                            //    _adminStates.Remove(msg.From.Id);
                                            //    return;
                                            //}

                                            var currentCourse = tempCourses[msg.From.Id];
                                            var lastQuestion = tempCourses[msg.From.Id].Questions.Last();

                                            foreach (var answer in lastQuestion.AnswerOption)
                                            {
                                                answer.IsCorrect = false;
                                            }

                                            if (int.TryParse(msg.Text, out int correctIndex) && correctIndex > 0 && correctIndex <= lastQuestion.AnswerOption.Count)
                                            {
                                                lastQuestion.AnswerOption.ElementAt(correctIndex - 1).IsCorrect = true;

                                                await botClient.SendMessage(
                                                        chatId: msg.From.Id,
                                                        text: $"Ответ '{lastQuestion.AnswerOption.ElementAt(correctIndex - 1).Text}' " +
                                                        $"установлен как правильный. Введите следующий вопрос для этого курса. " +
                                                        $"Если вопросов достаточно, для завершения отправьте '/go'"
                                                    );
                                                _adminStates[msg.From.Id] = AdminState.WaitingForQuestion;
                                            }
                                            else
                                            {
                                                await botClient.SendMessage(
                                                        chatId: msg.From.Id,
                                                        text: $"Некорректный номер ответа. Попробуйте еще раз"
                                                    );

                                                //_adminStates.Remove(msg.From.Id);
                                                //_adminStates[msg.From.Id] = AdminState.WaitingForAnswers;
                                                break;
                                            }

                                            break;
                                        }
                                    default:
                                        break;
                                }
                        }

                        else
                        {
                            await botClient.DeleteMessage(msg.Chat.Id, msg.MessageId, cancellationToken);
                        }
                    }
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    var callbackQuery = update.CallbackQuery;

                    if (callbackQuery == null) return;

                    var fromId = callbackQuery.Message.From.Id;
                    var chatId = callbackQuery.Message.Chat.Id;
                    var messageId = callbackQuery.Message.MessageId;
                    var callbackData = callbackQuery.Data;


                    if (callbackData == "True" || callbackData == "False")
                    {
                        // Теперь получаем состояние ПОСЛЕ создания
                        if (!_userStates.TryGetValue(chatId, out var userState))
                        {
                            await botClient.AnswerCallbackQuery(
                                callbackQueryId: callbackQuery.Id,
                                text: "Начните квиз заново",
                                cancellationToken: cancellationToken);
                            return;
                        }

                        // Проверяем, относится ли Callback к текущему вопросу
                        var msgId = callbackQuery.Message.MessageId;
                        if (msgId != userState.LastQuestionMessageId)
                        {
                            await botClient.AnswerCallbackQuery(
                                callbackQueryId: callbackQuery.Id,
                                text: "Это старый вопрос, нельзя отвечать повторно",
                                showAlert: false,
                                cancellationToken: cancellationToken);
                            return;
                        }


                        if (callbackData == "True")
                        {
                            userState.CurrentScore += 1;
                        }

                        userState.CurrentQuestionIndex++;

                        int techn = int.Parse(userState.Technology);
                        var technology = await techRepo.GetByIdAsync(techn);
                        var questions = await techRepo.GetAllQuestionsByTechnologyId(technology.Id);

                        // Удаляем кнопки у предыдущего сообщения
                        if (userState.LastQuestionMessageId != -1)
                        {
                            try
                            {
                                await botClient.EditMessageReplyMarkup(
                                    chatId: chatId,
                                    messageId: userState.LastQuestionMessageId,

                                    replyMarkup: null,
                                    cancellationToken: cancellationToken);
                            }
                            catch { /* игнорируем ошибки, если сообщение не найдено */ }
                        }

                        // Переходим к следующему вопросу или завершаем квиз
                        if (userState.CurrentQuestionIndex < questions.Count())
                        {
                            var nextQuestion = questions.ElementAt(userState.CurrentQuestionIndex);

                            var sentMessage = await botClient.SendMessage(
                                chatId: chatId,
                                text: $"Вопрос {userState.CurrentQuestionIndex + 1}. {nextQuestion.Text}",
                                replyMarkup: await GetAnswerButtons(answerRepo, nextQuestion.ShortName),
                                cancellationToken: cancellationToken);

                            lock (_lock)
                            {
                                userState.LastQuestionMessageId = sentMessage.MessageId;
                            }
                        }
                        else
                        {
                            if (userState.CurrentScore / questions.Count() >= 0.8)
                            {
                                var techId = int.Parse(userState.Technology);

                                var technologies = await userRepo.GetNewTechnologiesByParentTechnologyId(techId);

                                if (technologies.Count() == 1)
                                {
                                    var newTech = technologies.ElementAt(0);

                                    await botClient.SendMessage(
                                        chatId: chatId,
                                        text: $"Поздравляем! Тест успешно пройден.\nВаш результат: {userState.CurrentScore} / {questions.Count()}" +
                                        $"\nТеперь вам доступен курс {newTech.Title}",
                                        cancellationToken: cancellationToken
                                    );
                                }
                                else if (technologies.Count() > 1)
                                {
                                    var strNewTech = "";

                                    foreach (var tech in technologies)
                                    {
                                        strNewTech += $" {tech.Title},";
                                    }

                                    strNewTech = strNewTech.Substring(0, strNewTech.Length - 1);

                                    await botClient.SendMessage(
                                        chatId: chatId,
                                        text: $"Поздравляем! Тест успешно пройден.\nВаш результат: {userState.CurrentScore} / {questions.Count()}" +
                                        $"\nТеперь вам доступны курсы{strNewTech}",
                                        cancellationToken: cancellationToken
                                    );
                                }



                                    var passing = new UsersTechnologies()
                                    {
                                        UserId = chatId,
                                        TechnologyId = techId,
                                        IsCompleted = true
                                    };
                                await userRepo.AddFinishedTechnologyAsync(passing);
                                

                                lock (_lock)
                                {
                                    _userStates.Remove(chatId);
                                }
                            }
                            else
                            {
                                await botClient.SendMessage(
                                    chatId: chatId,
                                    text: $"К сожалению, тест не пройден, попробуйте еще раз\nВаш результат: {userState.CurrentScore} / {questions.Count()}",
                                    cancellationToken: cancellationToken
                                );

                                lock (_lock)
                                {
                                    _userStates.Remove(chatId);
                                }
                            }
                        }
                    }

                    else if (int.TryParse(update.CallbackQuery.Data, out int techId))
                    {
                        int technology = int.Parse(callbackData);
                        if (await userRepo.CheckFinishedTechnology(chatId, technology))
                        {
                            var date = await userRepo.GetDateOfFinishTechnologyByUserId(chatId, technology);

                            await botClient.SendMessage(
                                    chatId: chatId,
                                    text: $"Данный курс уже пройден вами!\nДата прохождения {date.AddHours(7)}",
                                    cancellationToken: cancellationToken
                                );

                            await botClient.AnswerCallbackQuery(
                                    callbackQueryId: update.CallbackQuery.Id,
                                    cancellationToken: _cts.Token
                                );

                            return;
                        }

                        var newUserState = new UserLocalState
                        {
                            Technology = callbackData,
                            CurrentQuestionIndex = 0,
                            CurrentScore = 0,
                            LastQuestionMessageId = -1
                        };

                        lock (_lock)
                        {
                            _userStates[chatId] = newUserState;
                        }


                        var tech = await techRepo.GetByIdAsync(techId);
                        var questions = await techRepo.GetAllQuestionsByTechnologyId(techId);
                        var question = questions.First();


                        await botClient.SendMessage(
                                chatId: update.CallbackQuery.From.Id,
                                text: $"Вы выбрали {tech.Title}, начинаем тестирование"
                            );

                        var sentMessage = await botClient.SendMessage(
                            chatId: chatId,
                            text: $"Вопрос 1. {question.Text}",
                            replyMarkup: await GetAnswerButtons(answerRepo, question.ShortName),
                            cancellationToken: cancellationToken);

                        lock (_lock)
                        {
                            _userStates[chatId].LastQuestionMessageId = sentMessage.MessageId;
                        }

                        //await botClient.AnswerCallbackQuery(
                        //        callbackQueryId: callbackQuery.Id,
                        //        cancellationToken: cancellationToken
                        //    );

                        await botClient.AnswerCallbackQuery(
                                callbackQueryId: update.CallbackQuery.Id,
                                cancellationToken: _cts.Token
                            );
                    }

                    else if (callbackData == "add_technology")
                    {
                        await botClient.SendMessage(
                                chatId: chatId,
                                text: "Вы хотите добавить курс",
                                cancellationToken: cancellationToken
                            );

                        await botClient.AnswerCallbackQuery(
                                callbackQueryId: update.CallbackQuery.Id,
                                cancellationToken: _cts.Token
                            );
                    }

                    else if (callbackData == "edit_technology")
                    {
                        await botClient.SendMessage(
                                chatId: chatId,
                                text: "Вы хотите изменить курс",
                                cancellationToken: cancellationToken
                            );

                        await botClient.AnswerCallbackQuery(
                                callbackQueryId: update.CallbackQuery.Id,
                                cancellationToken: _cts.Token
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Sequence contains no elements")
                {
                    await botClient.SendMessage(
                            chatId: 987896563,
                            text: $"В данном курсе отсутствуют вопросы"
                        );

                    await botClient.AnswerCallbackQuery(
                            callbackQueryId: update.CallbackQuery.Id,
                            cancellationToken: _cts.Token
                        );
                }
                else if (ex.Message == "message to delete not found")
                {
                    
                }
                else
                {
                    await botClient.SendMessage(
                            chatId: 987896563,
                            text: $"Ошибка: {ex},\nmessage: {ex.Message}"
                        );
                }
                throw;
            }
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var techRepo = scope.ServiceProvider.GetRequiredService<ITechnologyRepository>();
            var answerRepo = scope.ServiceProvider.GetRequiredService<IAnswerOptionRepository>();
            var questionRepo = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();


        }

        /// <summary>
        /// Метод для построения кнопок с вариантами ответов
        /// </summary>
        /// <param name="answerRepo">Репозиторий с вопросами</param>
        /// <param name="questionShortName">Короткое имя вопроса</param>
        /// <returns></returns>
        private static async Task<InlineKeyboardMarkup> GetAnswerButtons(IAnswerOptionRepository answerRepo, string questionShortName)
        {
            var buttonRows = new List<InlineKeyboardButton[]>();

            var options = await answerRepo.GetAllByQuestionShortName(questionShortName);

            foreach (var answer in options)
            {
                var isCorrect = answer.IsCorrect;
                buttonRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(answer.Text, isCorrect.ToString())
                });
            }

            var markup = new InlineKeyboardMarkup(buttonRows.ToArray());
            return markup;
        }

        /// <summary>
        /// Метод для построение кнопок с технологиями
        /// </summary>
        /// <param name="listOfTech">Список технологий</param>
        /// <returns>Кнопки с технологиями</returns>
        private static InlineKeyboardMarkup GetTechnologiesButton(IEnumerable<Technology> listOfTech)
        {
            var buttonRows = new List<InlineKeyboardButton[]>();

            foreach (var technology in listOfTech)
            {
                var technologyId = Convert.ToString(technology.Id);

                buttonRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(technology.Title, technologyId)
                });
            }

            var markup = new InlineKeyboardMarkup(buttonRows.ToArray());
            return markup;
        }
    }
}


//namespace BotTG
//{
//    public class TelegramBotService : BackgroundService
//    {
//        private readonly ILogger<TelegramBotService> _logger;
//        private readonly TelegramBotClient _bot;
//        private readonly CancellationTokenSource _cts;

//        private static readonly Dictionary<long, UserState> _userStates = new();
//        private static readonly object _lock = new();

//        public TelegramBotService(ILogger<TelegramBotService> logger)
//        {
//            _logger = logger;
//            var envPath = Path.Combine(AppContext.BaseDirectory, "BotTG", ".env");
//            Env.Load(envPath);
//            string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
//            _cts = new CancellationTokenSource();
//            _bot = new TelegramBotClient(botToken, cancellationToken: _cts.Token);

//            var recieverOptions = new ReceiverOptions
//            {
//                AllowedUpdates = { }
//            };

//            _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, recieverOptions, _cts.Token);
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            try
//            {
//                var me = await _bot.GetMe();
//                _logger.LogInformation($"@{me.Username} is running...");

//                while (!stoppingToken.IsCancellationRequested)
//                {
//                    await Task.Delay(1000, stoppingToken);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while running the bot");
//                throw;
//            }
//        }

//        public override async Task StopAsync(CancellationToken cancellationToken)
//        {
//            _cts.Cancel();
//            await base.StopAsync(cancellationToken);
//        }

//        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
//        {
//            DataRepository repo = new DataRepository();

//            var ListOfTechnologies = new List<string> { "python", "java" };

//            try
//            {
//                if (update.Type == UpdateType.CallbackQuery)
//                {
//                    var callbackQuery = update.CallbackQuery;

//                    if (callbackQuery == null) return;


//                    var fromId = callbackQuery.Message.From.Id;
//                    var chatId = callbackQuery.Message.Chat.Id;
//                    var messageId = callbackQuery.Message.MessageId;
//                    var callbackData = callbackQuery.Data;


//                    // --- Выбор технологии ---
//                    if (ListOfTechnologies.Contains(callbackData))
//                    {
//                        // Создаём состояние пользователя
//                        var newUserState = new UserState
//                        {
//                            Technology = callbackData,
//                            CurrentQuestionIndex = 0,
//                            CurrentScore = 0,
//                            LastQuestionMessageId = -1
//                        };

//                        lock (_lock)
//                        {
//                            _userStates[chatId] = newUserState;
//                        }

//                        await botClient.SendMessage(
//                            chatId: chatId,
//                            text: $"Хорошо, вы выбрали {callbackData}, начинаем тестирование.",
//                            cancellationToken: cancellationToken);

//                        var question = repo.GetQuestions(callbackData).Values.First();

//                        var sentMessage = await botClient.SendMessage(
//                            chatId: chatId,
//                            text: $"Вопрос 1. {question.Text}",
//                            replyMarkup: GetAnswerButtons(question),
//                            cancellationToken: cancellationToken);

//                        lock (_lock)
//                        {
//                            _userStates[chatId].LastQuestionMessageId = sentMessage.MessageId;
//                        }

//                        await botClient.AnswerCallbackQuery(
//                            callbackQueryId: callbackQuery.Id,
//                            cancellationToken: cancellationToken);
//                    }

//                    // --- Ответ на вопрос ---
//                    else if (callbackData == "True" || callbackData == "False")
//                    {
//                        // Теперь получаем состояние ПОСЛЕ создания
//                        if (!_userStates.TryGetValue(chatId, out var userState))
//                        {
//                            await botClient.AnswerCallbackQuery(
//                                callbackQueryId: callbackQuery.Id,
//                                text: "Начните квиз заново",
//                                cancellationToken: cancellationToken);
//                            return;
//                        }

//                        // Проверяем, относится ли Callback к текущему вопросу
//                        var msgId = callbackQuery.Message.MessageId;
//                        if (msgId != userState.LastQuestionMessageId)
//                        {
//                            await botClient.AnswerCallbackQuery(
//                                callbackQueryId: callbackQuery.Id,
//                                text: "Это старый вопрос, нельзя отвечать повторно",
//                                showAlert: false,
//                                cancellationToken: cancellationToken);
//                            return;
//                        }


//                        if (callbackData == "True")
//                        {
//                            userState.CurrentScore += 1;
//                        }

//                        userState.CurrentQuestionIndex++;

//                        var questions = repo.GetQuestions(userState.Technology).Values.ToList();

//                        // Удаляем кнопки у предыдущего сообщения
//                        if (userState.LastQuestionMessageId != -1)
//                        {
//                            try
//                            {
//                                await botClient.EditMessageReplyMarkup(
//                                    chatId: chatId,
//                                    messageId: userState.LastQuestionMessageId,

//                                    replyMarkup: null,
//                                    cancellationToken: cancellationToken);
//                            }
//                            catch { /* игнорируем ошибки, если сообщение не найдено */ }
//                        }

//                        // Переходим к следующему вопросу или завершаем квиз
//                        if (userState.CurrentQuestionIndex < questions.Count)
//                        {
//                            var nextQuestion = questions[userState.CurrentQuestionIndex];

//                            var sentMessage = await botClient.SendMessage(
//                                chatId: chatId,
//                                text: $"Вопрос {userState.CurrentQuestionIndex + 1}. {nextQuestion.Text}",
//                                replyMarkup: GetAnswerButtons(nextQuestion),
//                                cancellationToken: cancellationToken);

//                            lock (_lock)
//                            {
//                                userState.LastQuestionMessageId = sentMessage.MessageId;
//                            }
//                        }
//                        else
//                        {
//                            await botClient.SendMessage(
//                                chatId: chatId,
//                                text: $"🎉 Поздравляем! Тест завершён.\nВаш результат: {userState.CurrentScore} / {questions.Count}",
//                                cancellationToken: cancellationToken);

//                            lock (_lock)
//                            {
//                                _userStates.Remove(chatId);
//                            }
//                        }
//                    }
//                }

//                // --- Работа с текстовыми сообщениями ---
//                else if (update.Type == UpdateType.Message)
//                {
//                    var msg = update.Message;

//                    if (msg.Text != null)
//                    {
//                        if (msg.Text == "/start")
//                        {
//                            await botClient.SendMessage(
//                                chatId: msg.Chat.Id,
//                                text: $"Привет, {msg.Chat.FirstName}! Выбери технологию для прохождения теста",
//                                replyMarkup: new InlineKeyboardMarkup(new[]
//                                {
//                            new[] { InlineKeyboardButton.WithCallbackData("Python", "python") },
//                            new[] { InlineKeyboardButton.WithCallbackData("Java", "java") }
//                                }));

//                            var db = new AppDbContext();
//                            var user = new Models.User
//                            {
//                                ChatId = msg.Chat.Id,
//                                FullName = $"{msg.Chat.FirstName} {msg.Chat.LastName}",
//                                FirstName = msg.Chat.FirstName,
//                                LastName = msg.Chat.LastName
//                            };
//                            db.Add(user);
//                            db.SaveChanges();
//                        }
//                        else
//                        {
//                            await botClient.DeleteMessage(msg.Chat.Id, msg.MessageId, cancellationToken);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка при обработке обновления: {ex.Message}");
//            }
//        }

//        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
//        {
//            var errorMessage = exception switch
//            {
//                ApiRequestException apiException => $"Telegram API Error:\n[{apiException.ErrorCode}]\n{apiException.Message}",
//                _ => exception.ToString()
//            };

//            Console.WriteLine(errorMessage);
//            return Task.CompletedTask;
//        }

//        private static InlineKeyboardMarkup GetAnswerButtons(Question question)
//        {
//            var buttonRows = new List<InlineKeyboardButton[]>();

//            foreach (var answer in question.Answers)
//            {
//                var isCorrect = answer == question.CorrectAnswer;
//                buttonRows.Add(new[]
//                {
//                    InlineKeyboardButton.WithCallbackData(answer, isCorrect.ToString())
//                });
//            }

//            return new InlineKeyboardMarkup(buttonRows.ToArray());
//        }
//    }
//}