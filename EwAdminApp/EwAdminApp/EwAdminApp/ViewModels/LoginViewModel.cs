using System;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json;
using System.Threading.Tasks;
using EwAdmin.Common.Models.Setting;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string? _apiKey;

    public LoginViewModel(bool logout = false)
    {
        // if logout is true, clear all the login settings
        if (logout)
        {
            // remove the API key from the DI container
            Locator.CurrentMutable.UnregisterCurrent(typeof(LoginSettings));

            // set the API key to null
            ApiKey = null;

            // delete the userSettings.json file
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EwAdminApp");
            var settingsFilePath = Path.Combine(appDataFolder, "userSettings.json");
            if (File.Exists(settingsFilePath))
            {
                File.Delete(settingsFilePath);
            }
        }

        // add a command to save the API key to a file
        SaveApiKeyCommand = ReactiveCommand.CreateFromTask(SaveApiKeyAsync);

        this.WhenActivated((disposables) =>
        {
            // log the activation of viewmodel
            Console.WriteLine($"{GetType().Name} activated");
            
            SaveApiKeyCommand.Subscribe(result =>
            {
                // show a message to the user
                Console.WriteLine(result.success ? "API key saved successfully" : "API key is invalid");

                if (result.success)
                {
                    // navigate to the next page
                    // code here
                    //var mainViewModel = new MainViewModel();
                    //Locator.Current.GetService<MainViewModel>().ContentViewModel = mainViewModel;

                    // save the LoginSettings to DI container (splat)
                    Locator.CurrentMutable.RegisterConstant(result.settings, typeof(LoginSettings));

                    // emit a message event using MessageBus.Current.SendMessage
                    MessageBus.Current.SendMessage(new LoginEvent(result.settings));
                }
            })
            .DisposeWith(disposables);
            
            // if SaveApiKeyAsync fails, pop up a message to the user
            // code here
            SaveApiKeyCommand.ThrownExceptions.Subscribe(ex =>
            {
                // show a message to the user, with the exception message
                Console.WriteLine("Failed to save API key");
                Console.WriteLine(ex.Message);
            })
            .DisposeWith(disposables);

            // add an async method to load the API key from a file
            // if the file exists, set the API key property
            // and call the CheckApiKeyAsync method to check if the API key is valid
            // code here
            LoadApiKeyAsync().ContinueWith(task =>
            {
                if (task.Result.success)
                {
                    ApiKey = task.Result.settings?.ApiKey;
                    SaveApiKeyCommand.Execute().Subscribe();
                }
            })
            .DisposeWith(disposables);
            
            // log the deactivation of the viewmodel
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }

    public string? ApiKey
    {
        get => _apiKey;
        set => this.RaiseAndSetIfChanged(ref _apiKey, value);
    }

    // add an ReactiveCommand property to save the API key to a file
    public ReactiveCommand<Unit, (bool success, LoginSettings? settings)> SaveApiKeyCommand { get; }

    // make a method to check if the API key is valid by calling the API (/api/webAdmin/hello) using HttpClient registered in the DI container
    // ApiKey should be passed as a bearer HEADER
    private async Task<LoginUserResponse?> CheckApiKeyAsync()
    {
        try
        {
            var httpClient = Locator.Current.GetService<HttpClient>();
            var request =
                new HttpRequestMessage(HttpMethod.Get, "/api/webAdmin/hello");
            request.Headers.Add("Authorization", $"Bearer {ApiKey}");
            if (httpClient == null) return null;
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;
            var content = await response.Content.ReadAsStringAsync();
            // Deserialize the response content to LoginUserResponse, with options to case-insensitive
            // code here
            return JsonSerializer.Deserialize<LoginUserResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            // Log the exception or show a message to the user
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private async Task<(bool success, LoginSettings? settings)> SaveApiKeyAsync()
    {
        try
        {
            var loginUserResponse = await CheckApiKeyAsync();
            if (loginUserResponse?.Data?.Me == null) return (false, null);
            var settings = new LoginSettings
            {
                ApiKey = ApiKey,
                UserId = loginUserResponse.Data.Me.Id,
                UserEmail = loginUserResponse.Data.Me.Email,
                UserName = loginUserResponse.Data.Me.Name
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
            return (true, settings);
        }
        catch (Exception ex)
        {
            // Log the exception or show a message to the user
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    // add an async method to load the API key from a file
    // if the file does not exist, return (false, null)
    // if the file exists, read the content and deserialize it to LoginSettings
    // return (true, LoginSettings)
    // code here
    public async Task<(bool success, LoginSettings? settings)> LoadApiKeyAsync()
    {
        var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EwAdminApp");
        if (!Directory.Exists(appDataFolder))
        {
            return (false, null);
        }

        var settingsFilePath = Path.Combine(appDataFolder, "userSettings.json");
        if (!File.Exists(settingsFilePath))
        {
            return (false, null);
        }

        var settingsJson = await File.ReadAllTextAsync(settingsFilePath);
        return (true, JsonSerializer.Deserialize<LoginSettings>(settingsJson));
    }
}