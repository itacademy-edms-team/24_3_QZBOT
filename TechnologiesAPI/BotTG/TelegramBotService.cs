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
        //private static Dictionary<long, string> userStates = new Dictionary<long, string>();

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

                    long chatId = callbackQuery.Message.Chat.Id;

                    string callbackData = callbackQuery.Data;


                    var questions = new List<string> { };

                    foreach (var qstn in repo.GetQuestions(callbackData).Keys)
                    {
                        questions.Add(qstn);
                    }

                    

                    // выбор технологии
                    if (callbackData == "python" || callbackData == "java")
                    {
                        var sentMsg = await botClient.SendMessage(
                            chatId: chatId,

                            text: $"Хорошо, вы выбрали {callbackData}, выберите область вопроса",

                            cancellationToken: cancellationToken,
                            replyMarkup: new InlineKeyboardButton[][]
                            {
                                [($"{questions[0]}", $"{questions[0]}"),
                                ($"{questions[1]}", $"{questions[1]}"),
                                ($"{questions[2]}", $"{questions[2]}"),
                                ($"{questions[3]}", $"{questions[3]}"),
                                ($"{questions[4]}", $"{questions[4]}")],
                            }
                        );

                        var msgId = sentMsg.MessageId;

                        Console.WriteLine($"Пользователь {callbackQuery.From.Username} нажал кнопку с данными: {callbackData}");

                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callbackQuery.Id,
                            text: $"Выбор: {callbackData}!",
                            cancellationToken: cancellationToken
                        );
                    }

                    else if (callbackData == questions[0] || callbackData == questions[1] || callbackData == questions[2])
                    {
                        await botClient.SendMessage(
                        chatId: chatId,

                        text: $"Хорошо, вы выбрали {callbackData}, выберите область вопроса",

                        cancellationToken: cancellationToken,
                        replyMarkup: new InlineKeyboardButton[][]
                        {
                                [($"{questions[0]}", $"{questions[0]}"),
                                ($"{questions[1]}", $"{questions[1]}"),
                                ($"{questions[2]}", $"{questions[2]}")],
                        }
                    );

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
                            await botClient.SendMessage(msg.Chat, msg.Text);
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

        //private async Task OnMessage(Message msg, UpdateType type)
        //{
        //    if (msg.Text is null) 
        //    {
        //        await _bot.SendMessage(msg.Chat, "Напиши текст");
        //    }

        //    if (msg.Text == "/start")
        //    {
        //        var sent = await _bot.SendMessage(msg.Chat, $"Привет, {msg.Chat.FirstName}! Выбери технологию для прохождения теста",
        //        replyMarkup: new InlineKeyboardButton[][]
        //        {
        //            [("Python", "python")],
        //            [("Java", "java")]
        //        });
        //    }

        //    _logger.LogInformation($"Received {type} '{msg.Text}' in {msg.Chat}");
        //}
    }
}