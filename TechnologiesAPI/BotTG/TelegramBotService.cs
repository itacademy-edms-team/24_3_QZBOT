using Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
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

        private static readonly Dictionary<long, UserState> _userStates = new();
        private static readonly object _lock = new();

        public TelegramBotService(ILogger<TelegramBotService> logger)
        {
            _logger = logger;
            var envPath = Path.Combine(AppContext.BaseDirectory, "BotTG", ".env");
            Env.Load(envPath);
            string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            _cts = new CancellationTokenSource();
            _bot = new TelegramBotClient(botToken, cancellationToken: _cts.Token);

            var recieverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, recieverOptions, _cts.Token);
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

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            DataRepository repo = new DataRepository();

            var ListOfTechnologies = new List<string> { "python", "java" };

            try
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    var callbackQuery = update.CallbackQuery;

                    if (callbackQuery == null) return;


                    var fromId = callbackQuery.Message.From.Id;
                    var chatId = callbackQuery.Message.Chat.Id;
                    var messageId = callbackQuery.Message.MessageId;
                    var callbackData = callbackQuery.Data;


                    // --- Выбор технологии ---
                    if (ListOfTechnologies.Contains(callbackData))
                    {
                        // Создаём состояние пользователя
                        var newUserState = new UserState
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

                        await botClient.SendMessage(
                            chatId: chatId,
                            text: $"Хорошо, вы выбрали {callbackData}, начинаем тестирование.",
                            cancellationToken: cancellationToken);

                        var question = repo.GetQuestions(callbackData).Values.First();

                        var sentMessage = await botClient.SendMessage(
                            chatId: chatId,
                            text: $"Вопрос 1. {question.Text}",
                            replyMarkup: GetAnswerButtons(question),
                            cancellationToken: cancellationToken);

                        lock (_lock)
                        {
                            _userStates[chatId].LastQuestionMessageId = sentMessage.MessageId;
                        }

                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            cancellationToken: cancellationToken);
                    }

                    // --- Ответ на вопрос ---
                    else if (callbackData == "True" || callbackData == "False")
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

                        var questions = repo.GetQuestions(userState.Technology).Values.ToList();

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
                        if (userState.CurrentQuestionIndex < questions.Count)
                        {
                            var nextQuestion = questions[userState.CurrentQuestionIndex];

                            var sentMessage = await botClient.SendMessage(
                                chatId: chatId,
                                text: $"Вопрос {userState.CurrentQuestionIndex + 1}. {nextQuestion.Text}",
                                replyMarkup: GetAnswerButtons(nextQuestion),
                                cancellationToken: cancellationToken);

                            lock (_lock)
                            {
                                userState.LastQuestionMessageId = sentMessage.MessageId;
                            }
                        }
                        else
                        {
                            await botClient.SendMessage(
                                chatId: chatId,
                                text: $"🎉 Поздравляем! Тест завершён.\nВаш результат: {userState.CurrentScore} / {questions.Count}",
                                cancellationToken: cancellationToken);

                            lock (_lock)
                            {
                                _userStates.Remove(chatId);
                            }
                        }
                    }
                }

                // --- Работа с текстовыми сообщениями ---
                else if (update.Type == UpdateType.Message)
                {
                    var msg = update.Message;

                    if (msg.Text != null)
                    {
                        if (msg.Text == "/start")
                        {
                            await botClient.SendMessage(
                                chatId: msg.Chat.Id,
                                text: $"Привет, {msg.Chat.FirstName}! Выбери технологию для прохождения теста",
                                replyMarkup: new InlineKeyboardMarkup(new[]
                                {
                            new[] { InlineKeyboardButton.WithCallbackData("Python", "python") },
                            new[] { InlineKeyboardButton.WithCallbackData("Java", "java") }
                                }));

                            var db = new AppDbContext();
                            var user = new Models.User
                            {
                                ChatId = msg.Chat.Id,
                                FullName = $"{msg.Chat.FirstName} {msg.Chat.LastName}",
                                FirstName = msg.Chat.FirstName,
                                LastName = msg.Chat.LastName
                            };
                            db.Add(user);
                            db.SaveChanges();
                        }
                        else
                        {
                            await botClient.DeleteMessage(msg.Chat.Id, msg.MessageId, cancellationToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке обновления: {ex.Message}");
            }
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiException => $"Telegram API Error:\n[{apiException.ErrorCode}]\n{apiException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private static InlineKeyboardMarkup GetAnswerButtons(Question question)
        {
            var buttonRows = new List<InlineKeyboardButton[]>();

            foreach (var answer in question.Answers)
            {
                var isCorrect = answer == question.CorrectAnswer;
                buttonRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(answer, isCorrect.ToString())
                });
            }

            return new InlineKeyboardMarkup(buttonRows.ToArray());
        }
    }
}