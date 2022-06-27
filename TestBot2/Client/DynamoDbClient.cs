using System.Net.Http.Json;
using System.Text;
using basedApi.Constant;
using basedApi.Models;
using Newtonsoft.Json;

namespace basedApi.Client;

public class DynamoDbClient
{
    private HttpClient _httpClient;
    private static string _address;

    public DynamoDbClient()
    {
        _address = Constants.baseAddress;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_address);
    }
    
    public async Task<List<AnimeModel>> GetFavoritesList(string userId)
    {
        var response = await _httpClient.GetAsync($"AnimeDb/all?userId={userId}");
        try
        {
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<List<AnimeModel>>(content);
            return result;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task AddToFavorites(AnimeDbRepository anime, string userId)
    {
        var json = JsonConvert.SerializeObject(anime);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var post = await _httpClient.PostAsync($"AnimeDb/add?userId={userId}", data);
        post.EnsureSuccessStatusCode();

        var test = post.Content.ReadAsStringAsync().Result;
        Console.WriteLine(test);
    }

    public async Task<HttpResponseMessage> DeleteFromDb(AnimeDbRepository anime, string userId)
    {
        var json = JsonConvert.SerializeObject(anime);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        HttpRequestMessage request = new HttpRequestMessage
        {
            Content = data,
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{Constants.baseAddress}/AnimeDb/delete?userId={userId}")
        };
        return await _httpClient.SendAsync(request);
    }

    public async Task<AnimeModel> FindInDb(string id, string userId)
    {
        var response = await _httpClient.GetAsync($"AnimeDb?id={id}&userId={userId}");
        try
        {
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<AnimeModel>(content);
            return result;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(e);
            Console.ResetColor();
            return null;
        }
    }
}