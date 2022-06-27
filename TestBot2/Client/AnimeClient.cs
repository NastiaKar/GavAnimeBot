using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using basedApi.Models;
using basedApi.Constant;
using Exception = System.Exception;

namespace basedApi.Client;

public class AnimeClient
{
    private HttpClient _httpClient;
    private static string _address;
    private static string _accept = "application/vnd.api+json";
    private static string _contentType = "application/vnd.api+json";

    public AnimeClient()
    {
        _address = Constants.baseAddress;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_address);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_accept));
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", _contentType);
    }

    public async Task<AnimeModel> GetAnimeById(string id)
    {
        var response = await _httpClient.GetAsync($"MainAnime/byId?id={id}");
        try
        {
            var content = response.Content.ReadAsStringAsync().Result;
            if (content == "Not found!")
            {
                return null;
            }
            var result = JsonConvert.DeserializeObject<AnimeModel>(content);
            return result;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(e);
            Console.ForegroundColor = ConsoleColor.Green;
            return null;
        }
    }

    public async Task<AnimeModelArray> GetGenreByAnimeId(string id)
    {
        var response = await _httpClient.GetAsync($"MainAnime/genreById?id={id}");
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<AnimeModelArray>(content);
        return result;
        }

    public async Task<AnimeModelArray> GetAnimeList()
    {
        var response = await _httpClient.GetAsync($"MainAnime/list");
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<AnimeModelArray>(content);
        return result;
    }
    
    public async Task<AnimeModelArray> GetAnimeByTitle(string title)
    {
        string trueTitle = title.Replace(" ", "%20");
        var response = await _httpClient.GetAsync($"MainAnime/byTitle?title={trueTitle}");
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<AnimeModelArray>(content);
        return result;
    }
    
    public async Task<AnimeModelArray> GetListByCategory(string category)
    {
        string trueCategory = category.Replace(" ", "-");
        var response = await _httpClient.GetAsync($"MainAnime/listByCategory?category={trueCategory}");
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<AnimeModelArray>(content);
        return result;
    }
    
    public async Task<AnimeModelArray> GetAnimeByRating(string rating)
    {
        var response = await _httpClient.GetAsync($"/MainAnime/byRating?rating={rating.ToUpper()}");
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<AnimeModelArray>(content);
        return result;
    }
}