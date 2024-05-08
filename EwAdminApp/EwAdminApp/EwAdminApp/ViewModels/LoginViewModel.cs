using System;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EwAdmin.Common.Models.Setting;
using EwAdminApp.Events;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string? _apiKey;

    public LoginViewModel()
    {
        // implement the SaveApiKeyCommand, if SaveApiKeyAsync , pop up a message to the user
        // code here
        SaveApiKeyCommand = ReactiveCommand.CreateFromTask(SaveApiKeyAsync);
        SaveApiKeyCommand.Subscribe(success =>
        {
            // show a message to the user
            Console.WriteLine(success ? "API key saved successfully" : "API key is invalid");

            if (success)
            {
                // navigate to the next page
                // code here
                //var mainViewModel = new MainViewModel();
                //Locator.Current.GetService<MainViewModel>().ContentViewModel = mainViewModel;
                
                // emit a message event to the parent view model
                var messageEventAggregator = Locator.Current.GetService<IEventAggregator>();
                messageEventAggregator?.Publish(new MessageEvent("API key saved successfully"));
            }
        });
        // if SaveApiKeyAsync fails, pop up a message to the user
        // code here
        SaveApiKeyCommand.ThrownExceptions.Subscribe(ex =>
        {
            // show a message to the user, with the exception message
            Console.WriteLine("Failed to save API key");
            Console.WriteLine(ex.Message);
        });
    }

    public string? ApiKey
    {
        get => _apiKey;
        set => this.RaiseAndSetIfChanged(ref _apiKey, value);
    }
    
    // add an Icommand property to save the API key to a file
    // code here
    public ReactiveCommand<Unit, bool> SaveApiKeyCommand { get; }

    // make a method to check if the API key is valid by calling the API (https://localhost:7045/api/webAdmin/hello) using HttpClient registered in the DI container
    // ApiKey should be passed as a bearer HEADER
    // code here
    private async Task<LoginUserResponse?> CheckApiKeyAsync()
    {
        try
        {
            var httpClient = Locator.Current.GetService<HttpClient>();
            var request =
                new System.Net.Http.HttpRequestMessage(HttpMethod.Get, "https://localhost:7045/api/webAdmin/hello");
            request.Headers.Add("Authorization", $"Bearer {ApiKey}");
            if (httpClient == null) return null;
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;
            var content = await response.Content.ReadAsStringAsync();
            // Deserialize the response content to LoginUserResponse, with options to case insensitive
            // code here
            return JsonSerializer.Deserialize<LoginUserResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            // Log the exception or show a message to the user
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private async Task<bool> SaveApiKeyAsync()
    {
        try
        {
            var loginUserResponse = await CheckApiKeyAsync();
            if (loginUserResponse?.Data?.Me == null) return false;
            var settings = new LoginSettings
            {
                ApiKey = ApiKey,
                UserId = loginUserResponse.Data.Me.Id,
                UserEmail = loginUserResponse.Data.Me.Email,
            };
            var settingsJson = JsonSerializer.Serialize(settings);

            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EwAdminApp");
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            var settingsFilePath = Path.Combine(appDataFolder, "userSettings.json");
            await File.WriteAllTextAsync(settingsFilePath, settingsJson);
            return true;
        }
        catch (Exception ex)
        {
            // Log the exception or show a message to the user
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}

public record LoginSettings
{
    public string? ApiKey { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
}