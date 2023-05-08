using Discord;
using Tofu.BallBoi.Core;
namespace Tofu.BallBoi.Core.Models;
public class UserService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    //TODO: so all of these should have checks for the appropriate response status code.
    public UserService(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }
    public async Task<List<User>> GetAllUsersAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<User>>(_baseUrl + "/Users");
        return response;
    }
    public async  Task<User> GetUserByIdAsync(int id)
    {
        var response = await _httpClient.GetFromJsonAsync<User>(_baseUrl + $"/Users/{id}");
        return response;
    }
    public async Task<User> AddUserAsync(User user)
    {
        var response = await _httpClient.PostAsJsonAsync<User>(_baseUrl + $"/Users", user);

        throw new NotImplementedException();
    }
    public async Task<User> UpdateUserAsync(User user)
    {
        var response = await _httpClient.PutAsJsonAsync(_baseUrl + $"/Users/{user.Id}", user);
        throw new NotImplementedException();
    }
    public async Task<bool> DeleteUserAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(_baseUrl + $"/Users/{id}");
        return response.IsSuccessStatusCode;
    }
}
