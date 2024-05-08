using System.Threading.Tasks;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _apiKey;

    public LoginViewModel()
    {
    }

    public string ApiKey
    {
        get => _apiKey;
        set => this.RaiseAndSetIfChanged(ref _apiKey, value);
    }
    
    // make a method to check if the API key is valid by calling the API (https://localhost:7045/api/webAdmin/hello) using HttpClient registered in the DI container
    // ApiKey should be passed as a bearer HEADER
    // code here
    public async Task<bool> CheckApiKeyAsync()
    {
        var httpClient = Locator.Current.GetService<System.Net.Http.HttpClient>();
        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://localhost:7045/api/webAdmin/hello");
        request.Headers.Add("Authorization", $"Bearer {ApiKey}");
        var response = await httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
    
}