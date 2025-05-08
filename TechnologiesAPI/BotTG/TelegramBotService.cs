using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DotNetEnv;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using Models;

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
                // работа с кнопками
                if (update.Type == UpdateType.CallbackQuery)
                {
                    var callbackQuery = update.CallbackQuery;

                    if (callbackQuery == null) return;

                    string callbackData = callbackQuery.Data;


                    // выбор технологии
                    if (ListOfTechnologies.Contains(callbackData))
                    {
                        // Сохраняем состояние пользователя
                        var chatId = callbackQuery.Message.Chat.Id;

                        lock (_lock)
                        {
                            if (!_userStates.ContainsKey(chatId))
                            {
                                _userStates[chatId] = new UserState { Technology = callbackData, CurrentQuestionIndex = 0 };
                            }
                            else
                            {
                                _userStates[chatId].Technology = callbackData;
                                _userStates[chatId].CurrentQuestionIndex = 0;
                            }
                        }

                        await botClient.SendMessage(
                            chatId: chatId,
                            text: $"Хорошо, вы выбрали {callbackData}, начинаем тестирование.",
                            cancellationToken: cancellationToken
                        );

                        var question = repo.GetQuestions(callbackData).Values.First();

                        await botClient.SendMessage(
                            chatId: chatId,
                            text: $"Вопрос 1. {question.Text}",
                            replyMarkup: GetAnswerButtons(question),
                            cancellationToken: cancellationToken
                        );

                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            cancellationToken: cancellationToken
                        );
                    }

                    else if (callbackData == "True" || callbackData == "False")
                    {
                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            cancellationToken: cancellationToken
                        );

                        var chatId = callbackQuery.Message.Chat.Id;

                        if (!_userStates.TryGetValue(chatId, out var userState))
                        {
                            await botClient.SendMessage(chatId, "Начните квиз заново с /start", cancellationToken: cancellationToken);
                            return;
                        }

                        if (callbackData == "True")
                        {
                            userState.CurrentScore += 1;
                        }

                        userState.CurrentQuestionIndex++;

                        var questions = repo.GetQuestions(userState.Technology).Values.ToList();

                        if (userState.CurrentQuestionIndex < questions.Count)
                        {
                            var nextQuestion = questions[userState.CurrentQuestionIndex];

                            await botClient.SendMessage(
                                chatId: chatId,
                                text: $"Вопрос {userState.CurrentQuestionIndex + 1}. {nextQuestion.Text}",
                                replyMarkup: GetAnswerButtons(nextQuestion),
                                cancellationToken: cancellationToken
                            );
                        }
                        else
                        {
                            await botClient.SendMessage(chatId, $"Поздравляем! Вы завершили квиз. Ваш результат: {userState.CurrentScore} / {questions.Count}", cancellationToken: cancellationToken);

                            // Очистка состояния (по желанию)
                            lock (_lock)
                            {
                                _userStates.Remove(chatId);
                            }
                        }
                    }
                }


                // работа с сообщениями
                else if (update.Type == UpdateType.Message)
                {
                    var msg = update.Message;

                    if (msg.Text != null)
                    {
                        if (msg.Text == "/start")
                        {
                            var sent = await botClient.SendMessage(msg.Chat, $"Привет, {msg.Chat.FirstName}! Выбери технологию для прохождения теста",
                                replyMarkup: new InlineKeyboardButton[][]
                                    {
                                    [("Python", "python")],
                                    [("Java", "java")]
                                    });
                        }
                        else
                        {
                            //await botClient.SendMessage(msg.Chat, msg.Text);
                            await botClient.DeleteMessage(msg.Chat, update.Message.Id);
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