using basedApi.Client;
using basedApi.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;

namespace TestBot2;

public delegate Task<AnimeModelArray> AnimeActions(string search);

public class GavnukePipi2Bot
{
    private TelegramBotClient _botClient = new("5451276731:AAFuyQY0_oRV_7jZywPqRh2_WnJlwIL3Svc");
    private CancellationToken _cancellationToken = new();
    private ReceiverOptions _receiverOptions = new ReceiverOptions {AllowedUpdates = { }};
    private AnimeClient _animeClient = new AnimeClient();
    private Random _random = new Random();
    private DynamoDbClient _dbClient = new DynamoDbClient();
    private string _prevMsg = "haha";
    private List<Data> _currentArray;
    private AnimeModel _currentAnime;

    public async Task Start()
    {
        _botClient.StartReceiving(HandlerUpdateAsync, HandlerError, _receiverOptions, _cancellationToken);
        var botMe = await _botClient.GetMeAsync();
        Console.WriteLine($"Bot {botMe.Username} started working.");
        Console.ReadKey();
    }

    private Task HandlerError(ITelegramBotClient _botClient, Exception e,
        CancellationToken _cancellationToken)
    {
        var errorMessage = e switch
        {
            ApiRequestException apiRequestException => $"Error: {apiRequestException.ErrorCode}\n" +
                                                       $"Message: {apiRequestException.Message}",
            _ => e.ToString()
        };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
    }

    private async Task HandlerUpdateAsync(ITelegramBotClient _botClient, Update update,
        CancellationToken _cancellationToken)
    {
        if (update.Type == UpdateType.Message && update?.Message?.Text != null)
        {
            await HandlerMessageAsync(_botClient, update.Message);
        }

        if (update.Type == UpdateType.CallbackQuery)
        {
            await HandlerCallbackQuery(_botClient, update.CallbackQuery);
        }
    }

    private async Task HandlerMessageAsync(ITelegramBotClient _botClient, Message message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message.Chat.FirstName);
        Console.WriteLine(message.Text);
        Console.WriteLine(new string('-', 50));
        //Console.WriteLine(_prevMsg);
        switch (message.Text)
        {
            case "/start":
                await _botClient.SendTextMessageAsync(message.Chat.Id, char.ConvertFromUtf32(0x1F98B) + 
                                                                       "<em> Type /keyboard to see the commands</em>",
                    ParseMode.Html);
                _prevMsg = message.Text;
                return;
            case "/keyboard":
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new
                (new[]
                    {
                        new KeyboardButton[]
                        {
                            "Search by Name",
                            "Search by Genre",
                            "Search by Rating",
                        
                        },
                        new KeyboardButton[] 
                        {
                            "Random Anime",
                            "Top 10 Anime",
                            "Favorites"
                        }
                    }
                )
                {
                    ResizeKeyboard = true
                };
                await _botClient.SendTextMessageAsync(message.Chat.Id, char.ConvertFromUtf32(0x1F33A) + 
                                                                       "<em> Choose a command</em>",
                    replyMarkup: replyKeyboardMarkup,
                    parseMode: ParseMode.Html);
                _prevMsg = message.Text;
                return;
            }
            case "Random Anime":
            {
                var randomAnime = await AnimeDataEditor.GetRandomAnime(_animeClient);
                await AnimeDataEditor.SendAnime(randomAnime, message, _botClient);
                _currentAnime = randomAnime;
                _prevMsg = message.Text;
                return;
            }
            case "Search by Name":
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id,  char.ConvertFromUtf32(0x1F339)+ 
                                                                        "<em> Enter the title</em>",
                    cancellationToken: _cancellationToken,
                    parseMode: ParseMode.Html);
                _prevMsg = message.Text;
                return;
            }
            case "Search by Genre":
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, char.ConvertFromUtf32(0x1F33B) + 
                                                                       "<em> Enter the genre</em>", 
                    cancellationToken: _cancellationToken,
                    parseMode: ParseMode.Html);
                _prevMsg = message.Text;
                return;
            }
            case "Search by Rating":
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, char.ConvertFromUtf32(0x2728) + 
                                                                       "<em> Enter the rating" + 
                                                                        "\nAvailable Ratings: PG, G, R</em>",
                    cancellationToken: _cancellationToken, 
                    parseMode: ParseMode.Html);
                _prevMsg = message.Text;
                return;
            }
            case "Top 10 Anime":
            {
                var topAnimeList = _animeClient.GetAnimeList().Result;
                var animeArray = AnimeDataEditor.GetAnimeArray(topAnimeList);
                var msgToSend = AnimeDataEditor.GetAnimeResultsMessage(animeArray.Result);
                await _botClient.SendTextMessageAsync(message.Chat.Id, msgToSend.Result, ParseMode.Html);
                await _botClient.SendTextMessageAsync(message.Chat.Id, "\nEnter the index of " +
                                                                       "preferred anime");
                _currentArray = animeArray.Result;
                _prevMsg = message.Text;
                return;
            }
            case "Favorites":
            {
                try
                {
                    var favs = _dbClient.GetFavoritesList(message.Chat.Id.ToString()).Result;
                    var favsList = AnimeDataEditor.AnimeListToDataList(favs);
                    var msgToSend = AnimeDataEditor.GetAnimeResultsMessage(favsList.Result);
                    await _botClient.SendTextMessageAsync(message.Chat.Id, msgToSend.Result, parseMode: ParseMode.Html);
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "\nEnter the index of " +
                                                                           "preferred anime");
                    _currentArray = favsList.Result;
                    _prevMsg = message.Text;
                }
                catch (Exception e)
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Your Favorites collection is " +
                                                                           "currently empty. ");
                }
                return;
            }
            default:
            {
                switch (_prevMsg)
                {
                    case "Search by Name":
                    {
                        await GetData(message, _animeClient.GetAnimeByTitle);
                        break;
                    }
                    case "Search by Genre":
                    {
                        await GetData(message, _animeClient.GetListByCategory);
                        break;
                    }
                    case "Search by Rating":
                    {
                        await GetData(message, _animeClient.GetAnimeByRating);
                        break;
                    }
                    case "Favorites":
                    {
                        if (Convert.ToInt32(message.Text) <= 0 ||
                            Convert.ToInt32(message.Text) > _currentArray.Count())
                        {
                            await _botClient.SendTextMessageAsync(message.Chat.Id, "не балуйся");
                        }
                        else
                        {
                            var favList = _dbClient.GetFavoritesList(message.Chat.Id.ToString()).Result;
                            var dataList = AnimeDataEditor.AnimeListToDataList(favList).Result;
                            var anime = AnimeDataEditor.GetAnimeFromArray(dataList,
                                Convert.ToInt32(message.Text));
                            var animeToSend = _animeClient.GetAnimeById(anime.Result.Data.Id).Result;
                            await AnimeDataEditor.SendAnime(animeToSend, message, _botClient);
                            _currentAnime = animeToSend;
                        }

                        return;
                    }
                    default:
                        try
                        {
                            var anime = AnimeDataEditor.GetAnimeFromArray(_currentArray,
                                Convert.ToInt32(message.Text)).Result;
                            await AnimeDataEditor.SendAnime(anime, message, _botClient);
                            _currentAnime = anime;
                        }
                        catch (Exception e)
                        {
                            await _botClient.SendTextMessageAsync(message.Chat.Id, char.ConvertFromUtf32(0x1F319) + 
                                                                                   "<em> An error occurred...</em>",
                                cancellationToken: _cancellationToken,
                                parseMode: ParseMode.Html);
                        }
                        break;
                }
                _prevMsg = message.Text;
                return;
            }
        }
    }

    async Task HandlerCallbackQuery(ITelegramBotClient _botClient, CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data == "Add to Favorites")
        {
            try
            {
                var foundAnime = _dbClient.FindInDb(_currentAnime.Data.Id, 
                    callbackQuery.Message.Chat.Id.ToString()).Result;
                if (foundAnime.Data.Id != null)
                {
                    await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, 
                        "This anime is already in your Favorites collection!");
                    return;
                }
                var addToDb = await AnimeDataEditor.ModelToDb(_currentAnime, callbackQuery.Message.Chat.Id.ToString());
                await _dbClient.AddToFavorites(addToDb, callbackQuery.Message.Chat.Id.ToString());
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Added to Favorites!");
            }
            catch (Exception e)
            {
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, 
                    "<em>An error occurred... See console log</em>",
                    parseMode: ParseMode.Html);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(e);
            }
        }

        if (callbackQuery.Data == "Delete from Favorites")
        {
            try
            {
                var foundAnime = _dbClient.FindInDb(_currentAnime.Data.Id, 
                    callbackQuery.Message.Chat.Id.ToString()).Result;
                if (foundAnime.Data.Id == null)
                {
                    await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, 
                        "This anime is not found in your Favorites collection!");
                    return;
                }
                var animeDb = AnimeDataEditor.ModelToDb(_currentAnime, callbackQuery.Message.Chat.Id.ToString()).Result;
                await _dbClient.DeleteFromDb(animeDb, callbackQuery.Message.Chat.Id.ToString());
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Deleted from Favorites!");
            }
            catch (Exception e)
            {
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, 
                    "<em>An error occurred... See console log</em>",
                    parseMode: ParseMode.Html);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(e);
            }
        }
    }

    public async Task GetData(Message message, AnimeActions animeAction)
    {
        var results = animeAction($"{message.Text}").Result;
        var msgToSend = AnimeDataEditor.CheckForResults(results);
        await _botClient.SendTextMessageAsync(message.Chat.Id, msgToSend.Result, ParseMode.Html);
        if (results.Data.Count() != 0)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, "\n<em>Enter the index of " +
                                                                   "preferred anime, or choose another command!</em>",
                                                                    parseMode: ParseMode.Html);
        }

        var animeArray = AnimeDataEditor.GetAnimeArray(results);
        _currentArray = animeArray.Result;
    }
}