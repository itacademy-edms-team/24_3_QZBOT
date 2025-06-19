using Azure.Core;
using BotTG.DTO;
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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


namespace BotTG
{
    public class TelegramBotService : BackgroundService
    {
        private readonly ILogger<TelegramBotService> _logger;
        private readonly TelegramBotClient _bot;
        private readonly CancellationTokenSource _cts;
        private readonly IServiceProvider _serviceProvider;

        private static readonly Dictionary<long, UserLocalState> _userStates = new();
        private static Dictionary<long, ConfirmationAdd> _confirm = new(); 
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
                    if (msg.Document != null)
                    {
                        if (msg.Document.MimeType == "application/yaml")
                        {
                            if (_confirm.TryGetValue(msg.From.Id, out var conf))
                            {
                                _confirm.Remove(msg.From.Id);
                            }

                            if (msg.Caption != null)
                            {
                                if (await userRepo.CheckUserAdmin(msg.From.Id))
                                {
                                    if (msg.Caption == "/readfile")
                                    {
                                        string fileContent = await ReadFile(msg, botClient);

                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Содержимое файла:\n```\n{fileContent}\n```",
                                                parseMode: ParseMode.MarkdownV2
                                            );
                                    }
                                    else if (msg.Caption == "/addcourse")
                                    {
                                        string fileContent = await ReadFile(msg, botClient);

                                        var deserializer = new DeserializerBuilder()
                                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                            .WithNamingConvention(UnderscoredNamingConvention.Instance)
                                            .Build();

                                        try
                                        {
                                            var tech = deserializer.Deserialize<TechnologyDto>(fileContent);
                                        }
                                        catch (Exception)
                                        {
                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: "Удостоверьтесь в правильности данных в файле"
                                                );
                                            return;
                                        }

                                        var t = deserializer.Deserialize<TechnologyDto>(fileContent);


                                        if (await techRepo.CheckExistsTechnologyByTitle(t.Title))
                                        {
                                            await botClient.SendMessage(
                                                    msg.From.Id,
                                                    text: "Данная технология уже существует!"
                                                );
                                            return;
                                        }

                                        foreach (var quest in t.Questions)
                                        {
                                            var haveTrue = false;
                                            foreach (var answer in quest.AnswerOption)
                                            {
                                                if (haveTrue && answer.IsCorrect)
                                                {
                                                    await botClient.SendMessage(
                                                            chatId: msg.From.Id,
                                                            text: $"Для вопроса '{quest.Text}' выбрано несколько правильных ответов, ошибка"
                                                        );
                                                    return;
                                                }

                                                if (answer.IsCorrect)
                                                {
                                                    haveTrue = true;
                                                }
                                            }
                                            if (!haveTrue)
                                            {
                                                await botClient.SendMessage(
                                                        chatId: msg.From.Id,
                                                        text: $"Для вопроса '{quest.Text}' не введен правильный вариант ответа, ошибка"
                                                    );
                                                return;
                                            }
                                        }

                                        string stringOfData = "";
                                        foreach (var quest in t.Questions)
                                        {
                                            stringOfData += $"\n{quest.Text}:\n";

                                            foreach (var answer in quest.AnswerOption)
                                            {
                                                stringOfData += $"- {answer.Text} - {answer.IsCorrect}\n";
                                            }
                                        }

                                        try
                                        {
                                            var tec = MakeDtoToTechnology(techRepo, t);
                                        }
                                        catch (Exception)
                                        {
                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: "Такой родительской технологии не существует"
                                                );
                                            return;
                                        }

                                        var te = await MakeDtoToTechnology(techRepo, t);

                                        if (await techRepo.CheckValidTechnology(te) == "true")
                                        {
                                            await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Будет передано:\n" +
                                                $"{t.Title}:\nПредшествующий курс: {t.ParentTechnologyTitle} {stringOfData}\n\n" +
                                                $"Подтвердить? (+/-)"
                                            );

                                            if (_confirm.TryGetValue(msg.From.Id, out var confi))
                                            {
                                                _confirm.Remove(msg.From.Id);
                                            }

                                            _confirm.Add(msg.From.Id, new ConfirmationAdd());

                                            _confirm[msg.From.Id].Technology = t;
                                            _confirm[msg.From.Id].ConfirmToAdd = ConfirmToAdd.Confirm;
                                        }
                                        else
                                        {
                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: $"{await techRepo.CheckValidTechnology(te)}"
                                                );
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var fileType = "";

                            foreach (var item in msg.Document.FileName)
                            {
                                if (item == '.')
                                {
                                    fileType = "";
                                }
                                else
                                {
                                    fileType += item;
                                }
                            }

                            await botClient.SendMessage(
                                    chatId: msg.From.Id,
                                    text: $"Ты отправил документ {msg.Document.FileName}\n" +
                                    $"Thambnail: {msg.Document.Thumbnail}\n" +
                                    $"MimeType: {msg.Document.MimeType}\n" +
                                    $"FileSize: {msg.Document.FileSize}\n" +
                                    $"тип файла: {fileType}"
                                );
                        }
                    }

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
                        //else if (msg.Text == "/checkdb")
                        //{
                        //    if (await userRepo.ExistsByChatIdAsync(msg.Chat.Id))
                        //    {
                        //        await botClient.SendMessage(
                        //                chatId: msg.Chat.Id,
                        //                text: $"Привет, {msg.Chat.FirstName}! Ты уже есть в базе данных"
                        //            );
                        //    }
                        //    else
                        //    {
                        //        await botClient.SendMessage(
                        //                chatId: msg.Chat.Id,
                        //                text: $"Привет, {msg.Chat.FirstName}! Тебя нет в нашей базе данных"
                        //            );
                        //    }
                        //}
                        else if (msg.Text == "/deleteprogress")
                        {
                            if (!await userRepo.ExistsByChatIdAsync(msg.Chat.Id))
                            {
                                await botClient.SendMessage(
                                        chatId: msg.Chat.Id,
                                        text: "Вас нет в базе данных"
                                    );
                            }
                            else
                            {
                                await userRepo.DeleteProgressByChatIdAsync(msg.Chat.Id);

                                await botClient.SendMessage(
                                        chatId: msg.Chat.Id,
                                        text: $"Ваш прогресс удален"
                                    );

                                _userStates.Remove(msg.Chat.Id);
                            }
                        }
                        
                        else if (msg.Text == "/checkcources")
                        {
                            var technologies = await userRepo.GetAllCompletedTechnologiesByIdAsync(msg.Chat.Id);

                            var str = "Курсы, пройденные вами:\n";
                            int count = 1;
                            foreach (var te in technologies)
                            {
                                var date = await userRepo.GetDateOfFinishTechnologyByUserId(msg.Chat.Id, te.Id);

                                str += $"{count}. {te.Title}. Дата прохождения: {date} (UTC)\n";
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
                                        "Введите <b>название технологии</b>",
                                        parseMode: ParseMode.Html
                                    );

                        }

                        else if (_confirm.TryGetValue(msg.From.Id, out var conf))
                        {
                            if (_confirm[msg.From.Id].ConfirmToAdd == ConfirmToAdd.Confirm)
                            {
                                if (msg.Text == "+")
                                {
                                    try
                                    {
                                        await MakeDtoToTechnology(techRepo, _confirm[msg.From.Id].Technology);
                                    }
                                    catch (Exception)
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: "Такой родительской технологии не существует"
                                            );
                                    }

                                    var te = await MakeDtoToTechnology(techRepo, _confirm[msg.From.Id].Technology);

                                    if (await techRepo.CheckValidTechnology(te) == "true")
                                    {
                                        await techRepo.AddAsync(te);
                                    }

                                    await botClient.SendMessage(
                                            chatId: msg.From.Id,
                                            text: "Данные отправлены"
                                        );
                                }
                                else if (msg.Text == "-")
                                {
                                    _confirm.Remove(msg.From.Id);

                                    await botClient.SendMessage(
                                            chatId: msg.From.Id,
                                            text: "Отправка отменена"
                                        );
                                }
                            }
                        }

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
                            else if (msg.Text == "/cancel")
                            {
                                switch (_adminStates[msg.From.Id])
                                {
                                    case AdminState.WaitingForQuestion:
                                        if (tempCourses[msg.Chat.Id].Questions.Count > 0)
                                        {
                                            await botClient.SendMessage(
                                                    chatId: msg.From.Id,
                                                    text: $"Вопрос с вариантами ответов уже внесен и отменить нельзя, " +
                                                    $"двигайтесь дальше",
                                                    cancellationToken: cancellationToken
                                                );

                                            return;
                                        }

                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Ввод предшествующего курса отменен. Повторите попытку",
                                                cancellationToken: cancellationToken
                                            );

                                        _adminStates[msg.From.Id] = AdminState.WaitingForParentCourseName;
                                        return;

                                    case AdminState.WaitingForShortNameQuestion:
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Ввод вопроса отменен. Повторите попытку",
                                                cancellationToken: cancellationToken
                                            );

                                        var lastQue = tempCourses[msg.From.Id].Questions.Last();
                                        tempCourses[msg.From.Id].Questions.Remove(lastQue);

                                        _adminStates[msg.From.Id] = AdminState.WaitingForQuestion;
                                        return;

                                    case AdminState.WaitingForAnswers:
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Ввод короткого названия вопроса отменен. Повторите попытку",
                                                cancellationToken: cancellationToken
                                            );
                                        _adminStates[msg.From.Id] = AdminState.WaitingForShortNameQuestion;
                                        return;

                                    case AdminState.WaitingForRightAnswer:
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Ввод вариантов ответа отменен. Повторите попытку",
                                                cancellationToken: cancellationToken
                                            );
                                        _adminStates[msg.From.Id] = AdminState.WaitingForAnswers;
                                        return;

                                    default:
                                        break;
                                }
                            }
                            else if (msg.Text == "/go")
                            {
                                var currentCourse = tempCourses[msg.From.Id];

                                if (string.IsNullOrWhiteSpace(currentCourse.Title))
                                {
                                    await botClient.SendMessage(
                                            chatId: msg.Chat.Id,
                                            text: "Данные не заполнены! Отправка не удалась"
                                        );
                                    return;
                                }

                                if (currentCourse.Questions.Count < 5)
                                {
                                    await botClient.SendMessage(
                                            chatId: msg.Chat.Id,
                                            text: "Мало вопросов! Добавьте больше"
                                        );
                                    return;
                                }

                                foreach (var question in currentCourse.Questions)
                                {
                                    if (question.AnswerOption.Count == 0)
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.Chat.Id,
                                                text: "Данные не заполнены! Отправка не удалась"
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

                                    tempCourses[msg.From.Id] = new CourseInput();
                                    string courseName = msg.Text;

                                    if (await techRepo.CheckExistsTechnologyByTitle(courseName))
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Курс '{courseName}' уже существует. Повторите попытку"
                                            );
                                    }
                                    else
                                    {
                                        tempCourses[msg.From.Id].Title = courseName;
                                        _adminStates[msg.From.Id] = AdminState.WaitingForParentCourseName;
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Название курса '{courseName}' сохранено. Введите <b>название " +
                                                $"предшествующей технологии</b> (если такой нет, введите 'no'):",
                                                parseMode: ParseMode.Html
                                            );

                                        tempCourses[msg.From.Id].Questions = new List<Question>();
                                    }

                                    break;


                                case AdminState.WaitingForParentCourseName:

                                    string parentTech = msg.Text;
                                    if (parentTech == "no")
                                    {
                                        tempCourses[msg.From.Id].TitleOfParentCourse = null;

                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: "Курс будет для начинающих\n" +
                                                "Для отмены отправьте /cancel\n" +
                                                "Введите <b>текст первого вопроса</b>",
                                                parseMode: ParseMode.Html
                                            );

                                        _adminStates[msg.From.Id] = AdminState.WaitingForQuestion;
                                    }

                                    else if (await techRepo.CheckExistsTechnologyByTitle(parentTech))
                                    {
                                        tempCourses[msg.From.Id].TitleOfParentCourse = parentTech;
                                        var nameOfCourse = tempCourses[msg.From.Id].Title;

                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Курс '{nameOfCourse}' успешно прикреплен к курсу '{parentTech}'\n" +
                                                $"Для отмены отправьте /cancel\n" +
                                                $"Введите <b>текст первого вопроса</b>",
                                                parseMode: ParseMode.Html
                                            );

                                        _adminStates[msg.From.Id] = AdminState.WaitingForQuestion;
                                    }
                                    else
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Технологии '{parentTech}' не существует. Повторите попытку"
                                            );
                                    }

                                    break;


                                case AdminState.WaitingForQuestion:

                                    string question = msg.Text;

                                    if (await questionRepo.CheckExistsQuestionByText(question))
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: "Такой вопрос уже существует. Повторите попытку"
                                            );

                                        break;
                                    }

                                    if (question[question.Length - 1] != '?')
                                    {
                                        question += "?";
                                    }

                                    var quest = new Question();
                                    tempCourses[msg.From.Id].Questions.Add(quest);

                                    tempCourses[msg.From.Id].Questions.Last().Text = question;

                                    await botClient.SendMessage(
                                            chatId: msg.From.Id,
                                            text: $"Вопрос '{question}' успешно добавлен. " +
                                            $"Для отмены отправьте /cancel\n" +
                                            $"Введите <b>короткое название вопроса</b> (например 'variablesPython')",
                                            parseMode: ParseMode.Html
                                        );

                                    _adminStates[msg.From.Id] = AdminState.WaitingForShortNameQuestion;
                                    break;



                                case AdminState.WaitingForShortNameQuestion:

                                    string shortName = msg.Text;

                                    if (await questionRepo.CheckExistsQuestionByShortName(shortName))
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Короткое название '{shortName}' уже существует. Повторите попытку"
                                            );

                                        break;
                                    }

                                    string questionText = tempCourses[msg.From.Id].Questions.Last().Text;

                                    await botClient.SendMessage(
                                            chatId: msg.From.Id,
                                            text: $"Короткое название '{shortName}' успешно добавлено.\n" +
                                            $"Для отмены отправьте /cancel\n" +
                                            $"Введите <b>варианты ответов</b> на вопрос '{questionText}' <b>через точку с запятой " +
                                            $"без пробелов между ними</b>",
                                            parseMode: ParseMode.Html
                                        );

                                    tempCourses[msg.From.Id].Questions.Last().ShortName = shortName;

                                    _adminStates[msg.From.Id] = AdminState.WaitingForAnswers;
                                    break;


                                case AdminState.WaitingForAnswers:

                                    string textOfAnswers = msg.Text;

                                    if (!textOfAnswers.Contains(";"))
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Нельзя внести только один вариант ответа. Повторите попытку"
                                            );

                                        break;
                                    }

                                    string[] Answers = textOfAnswers.Split(";");

                                    var lastQuestion = tempCourses[msg.From.Id].Questions.Last();

                                    lastQuestion.AnswerOption.Clear();
                                    var answ = new List<string>();

                                    foreach (var answer in Answers)
                                    {
                                        if (answer == string.Empty)
                                        {
                                            continue;
                                        }
                                        answ.Add(answer);
                                        lastQuestion.AnswerOption.Add(new AnswerOption { Text = answer });
                                    }

                                    string strAnswers = "";
                                    for (int i = 0; i < answ.Count; i++)
                                    {
                                        strAnswers += $"{i + 1}. {answ[i]}\n";
                                    }


                                    await botClient.SendMessage(
                                            chatId: msg.From.Id,
                                            text: $"Варианты ответов успешно внесены, " +
                                            $"теперь отправьте <b>номер правильного варианта ответа среди них</b>\n" +
                                            $"<b>Важно: отменить это действие не получится!</b>\n" +
                                            $"{strAnswers}",
                                            parseMode: ParseMode.Html
                                        );

                                    _adminStates[msg.From.Id] = AdminState.WaitingForRightAnswer;
                                    break;


                                case AdminState.WaitingForRightAnswer:

                                    var currentCourse = tempCourses[msg.From.Id];
                                    var lastQuest = tempCourses[msg.From.Id].Questions.Last();

                                    foreach (var answer in lastQuest.AnswerOption)
                                    {
                                        answer.IsCorrect = false;
                                    }

                                    if (int.TryParse(msg.Text, out int correctIndex) && correctIndex > 0 && correctIndex <= lastQuest.AnswerOption.Count)
                                    {
                                        lastQuest.AnswerOption.ElementAt(correctIndex - 1).IsCorrect = true;
                                        var countQuestions = tempCourses[msg.From.Id].Questions.Count;

                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Ответ '{lastQuest.AnswerOption.ElementAt(correctIndex - 1).Text}' " +
                                                $"установлен как правильный. Введите <b>следующий вопрос</b> для этого курса. " +
                                                $"Если вопросов достаточно ({countQuestions}), для завершения отправьте '/go'",
                                                parseMode: ParseMode.Html
                                            );
                                        _adminStates[msg.From.Id] = AdminState.WaitingForQuestion;
                                    }
                                    else
                                    {
                                        await botClient.SendMessage(
                                                chatId: msg.From.Id,
                                                text: $"Некорректный номер ответа. Повторите попытку"
                                            );
                                    }

                                    break;

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
                                else
                                {
                                    await botClient.SendMessage(
                                            chatId: chatId,
                                            text: $"Поздравляем! Тест успешно пройден.\nВаш результат: {userState.CurrentScore} / {questions.Count()}",
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
        /// Метод для построения кнопок с технологиями
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

        /// <summary>
        /// Метод для чтения файла, отправленного пользователем
        /// </summary>
        /// <param name="msg">Объект сообщения</param>
        /// <param name="botClient">Объект бота</param>
        /// <returns>Текст из файла</returns>
        private static async Task<string> ReadFile(Message msg, ITelegramBotClient botClient)
        {
            var fileId = msg.Document.FileId;

            // Получаем информацию о файле
            var fileInfo = await botClient.GetFile(fileId);

            // Формируем URL для скачивания файла
            var filePath = fileInfo.FilePath;
            var fileStream = new MemoryStream();

            // Загружаем содержимое
            await botClient.DownloadFile(filePath, fileStream);

            // Перемещаем указатель потока на начало
            fileStream.Position = 0;

            // Читаем содержимое
            using var reader = new StreamReader(fileStream);
            string fileContent = await reader.ReadToEndAsync();

            return fileContent;
        }

        private static async Task<Technology> MakeDtoToTechnology(ITechnologyRepository repo, TechnologyDto dto)
        {
            var parentId = await repo.GetIdByTitleAsync(dto.ParentTechnologyTitle);

            var technology = new Technology()
            {
                Title = dto.Title,
                ParentTechnologyId = parentId,
                Questions = dto.Questions
            };

            return technology;
        }
    }
}