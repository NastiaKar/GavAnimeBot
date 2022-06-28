using basedApi.Models;
using basedApi.Client;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TestBot2;

public static class AnimeDataEditor
{
    public static async Task SendAnime(AnimeModel anime, Message message, ITelegramBotClient _botClient)
    {
        AnimeClient ac = new AnimeClient();
        var genresModel = await ac.GetGenreByAnimeId(anime.Data.Id);
        var genresList = GetAnimeArray(genresModel).Result;

        string genres = "";
        foreach (var genre in genresList)
        {
            genres += $"- {genre.Attributes.Name}";
        }
        

        await _botClient.SendPhotoAsync(chatId: message.Chat.Id,
            photo: $"{anime.Data.Attributes.PosterImage.Large}",
            caption: $"{char.ConvertFromUtf32(0x1F338)} <b>English Title: </b>" +
                     $"<em>{anime.Data.Attributes.Titles.En_Jp}</em>\n" +
                     $"{char.ConvertFromUtf32(0x1F338)} <b>Japanese Title: </b>" +
                     $"<em>{anime.Data.Attributes.Titles.Ja_Jp}</em>\n" +
                     $"\n{anime.Data.Attributes.AgeRatingGuide}" +
                     $"\nGenres:\n{genres}",
            parseMode: ParseMode.Html);
        await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            $"<b>Description:</b> {anime.Data.Attributes.Synopsis}",
            parseMode: ParseMode.Html,
            replyMarkup: await AddInlineButtons());

    }

    public static async Task<string> GetAnimeResultsMessage(List<Data> animeList)
    {
        var msgToSend = "";
        for (int i = 0; i < animeList.Count(); i++)
        {
            msgToSend += $"<b>{i + 1}. {animeList[i].Attributes.Titles.En_Jp} {char.ConvertFromUtf32(0x1F341)}\n</b>";
            var description = animeList[i].Attributes.Synopsis;
            if (description == "")
                description = "<em>\tNo description.</em>\n\n";
            else if (description.Length > 75)
                description = $"<em>\t{animeList[i].Attributes.Synopsis[..75]}...</em>\n\n";
            else
                description = $"<em>\t{animeList[i].Attributes.Synopsis}</em>\n\n";
            msgToSend += description;
        }
        return msgToSend;
    }

    public static async Task<List<Data>> GetAnimeArray(AnimeModelArray animeList)
    {
        var animeArray = animeList.Data.ToList();
        return animeArray;
    }
    public static async Task<List<Data>> AnimeListToDataList(List<AnimeModel> models)
    {
        return models.Select(model => model.Data).ToList();
    }

    public static async Task<AnimeModel> GetRandomAnime(AnimeClient animeClient)
    {
        Random random = new Random();
        var anime = await animeClient.GetAnimeById($"{random.Next(0, 10000)}");
        while (anime == null)
        {
            anime = await animeClient.GetAnimeById($"{random.Next(0, 10000)}");
        }

        return anime;
    }

    public static async Task<string> CheckForResults(AnimeModelArray results)
    {
        if (!results.Data.Any())
        {
            var msgNotFound = "Anime not found!";
            return msgNotFound;
        }

        var options = await GetAnimeArray(results);
        var msgToSend = GetAnimeResultsMessage(options);
        return msgToSend.Result;
    }

    public static async Task<IReplyMarkup?> AddInlineButtons()
    {
        InlineKeyboardMarkup keyboardMarkup = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Add to Favorites")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Delete from Favorites")
            }
        });
        return keyboardMarkup;
    }

    public static async Task<AnimeDbRepository> ModelToDb(AnimeModel anime, string userId)
    {
        var repo = new AnimeDbRepository
        {
            Id = anime.Data.Id,
            EnJpTitle = anime.Data.Attributes.Titles.En_Jp,
            JaJpTitle = anime.Data.Attributes.Titles.Ja_Jp,
            Synopsis = anime.Data.Attributes.Synopsis,
            UserId = userId
        };
        return repo;
    }
    
    public static async Task<AnimeDbRepository> DataToDb(Data anime, string userId)
    {
        var repo = new AnimeDbRepository
        {
            Id = anime.Id,
            EnJpTitle = anime.Attributes.Titles.En_Jp,
            JaJpTitle = anime.Attributes.Titles.Ja_Jp,
            Synopsis = anime.Attributes.Synopsis,
            UserId = userId
        };
        return repo;
    }

    public static async Task<AnimeModel> GetAnimeFromArray(List<Data> anime, int index)
    {
        var current = anime[index - 1];
        AnimeModel animeModel = new AnimeModel()
        {
            Data = new Data()
            {
                Attributes = new Attributes
                {
                    CreatedAt = current.Attributes.CreatedAt,
                    Synopsis = current.Attributes.Synopsis,
                    AgeRating = current.Attributes.AgeRating,
                    AgeRatingGuide = current.Attributes.AgeRatingGuide,
                    Titles = new Titles()
                    {
                        En_Jp = current.Attributes.Titles.En_Jp,
                        Ja_Jp = current.Attributes.Titles.Ja_Jp
                    },
                    PosterImage = current.Attributes.PosterImage
                },
                Id = current.Id,
            }
        };
        return animeModel;
    }
}