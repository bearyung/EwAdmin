using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EwAdminApi.Extensions;
using EwAdminApi.Models.Monday;
using MondaySharp.NET.Application.Interfaces;
using MondaySharp.NET.Infrastructure.Persistence;

namespace EwAdminApi.Middlewares;

public class MondayAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _clientFactory;

    public MondayAuthorizationMiddleware(RequestDelegate next, IHttpClientFactory clientFactory)
    {
        _next = next;
        _clientFactory = clientFactory;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<MondayAuthorizationMiddleware> logger)
    {
        // bypass the authorization check for the general based API
        // Health Check Endpoint: api/general/*
        if (context.Request.Path.StartsWithSegments("/api/general"))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        logger.LogInformation("Attempting to retrieve user data from Monday.com");
        string? apiKey = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new { errorMessage = "Unauthorized: API key is missing." })
                .ConfigureAwait(false);
            return;
        }

        var client = _clientFactory.CreateClient("MondayClient");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var meQuery = new
        {
            query = @"
            {
              me {
                id
                email
                name
              }
            }"
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            PropertyNameCaseInsensitive = true
        };
        var content = new StringContent(JsonSerializer.Serialize(meQuery, jsonOptions), Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("", content).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"Failed to retrieve user data: {response.StatusCode}");
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new { errorMessage = "Unauthorized access" }).ConfigureAwait(false);
            return;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var userData = JsonSerializer.Deserialize<MondayUserResponse>(jsonResponse, jsonOptions);

        // check if the user has access right for calling this API
        // Determine the API path from the request
        logger.LogInformation("User data retrieved successfully, checking access rights...");

        string apiPath = context.Request.Path.ToString();
        string userIdWithPrefix = $"person-{userData?.Data?.Me?.Id}";

        // Prepare the GraphQL query to check user access on the specific API path
        var checkAccessRightsQuery = new
        {
            query = @"
            query ($api_path: CompareValue!, $user_id_with_prefix: CompareValue!, $board_id: [ID!]) {
              boards(ids: $board_id) {
                items_page(
                  limit: 5
                  query_params: {rules: [{column_id: ""people__1"", compare_value: $user_id_with_prefix, operator: any_of},
                  {column_id: ""name"", compare_value: $api_path, operator: any_of}]}
                ) {
                  items {
                    id
                    name
                    column_values {
                      id
                      value
                    }
                  }
                }
              }
            }",
            variables = new
            {
                api_path = apiPath,
                user_id_with_prefix = userIdWithPrefix,
                board_id = 6492725441
            }
        };

        var checkAccessContent = new StringContent(JsonSerializer.Serialize(checkAccessRightsQuery, jsonOptions),
            Encoding.UTF8, "application/json");
        var checkAccessResponse = await client.PostAsync("", checkAccessContent).ConfigureAwait(false);
        if (!checkAccessResponse.IsSuccessStatusCode)
        {
            logger.LogError($"Failed to retrieve user data: {checkAccessResponse.StatusCode}");
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new { errorMessage = "Unauthorized access" }).ConfigureAwait(false);
            return;
        }

        var jsonCheckAcccessResponse = await checkAccessResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        var checkAccessResult = JsonSerializer.Deserialize<MondayApiResponse>(jsonCheckAcccessResponse, jsonOptions);

        // check if the result has any number of records
        var userHasAccess = checkAccessResult?.Data?.Boards?.FirstOrDefault()?.ItemsPage?.Items != null &&
                            checkAccessResult?.Data?.Boards?.FirstOrDefault()?.ItemsPage?.Items?.Count > 0;

        if (!userHasAccess)
        {
            logger.LogWarning($"Access denied for user {userData?.Data?.Me?.Id} to path {apiPath}");
            context.Response.StatusCode = 403; // Forbidden
            await context.Response
                .WriteAsJsonAsync(new { errorMessage = "Access Denied: You do not have rights to access this API." })
                .ConfigureAwait(false);
            return;
        }

        // add a record to Monday access log, Monday Board ID: 6709158498
        /* Sample item structure
         {
           "name": "202405261305",
           "id": "6709158504",
           "column_values": [
             {
               "id": "person",
               "value": "{\"changed_at\":\"2024-05-26T03:35:46.257Z\",\"personsAndTeams\":[{\"id\":39629823,\"kind\":\"person\"}]}"
             },
             {
               "id": "text__1",
               "value": "\"285.123.1.10\""
             },
             {
               "id": "text3__1",
               "value": "\"/api/account/detail/\""
             }
        */
        // person column id: person
        // Source IP: text__1
        // API Path: text3__1

        var logQuery = new
        {
            query = """
                        mutation ($item_name: String!, $column_values: JSON!) {
                          create_item(
                            board_id: 6709158498
                            item_name: $item_name
                            column_values: $column_values
                          ) {
                            id
                          }
                        }
                    """,
            variables = new
            {
                item_name = DateTime.UtcNow.Ticks.ToString(),
                column_values = JsonSerializer.Serialize(new
                {
                    text37__1 = userData?.Data?.Me?.Name,
                    text__1 = context.Connection.RemoteIpAddress?.ToString(),
                    text3__1 = apiPath,
                    date__1 = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")
                }, jsonOptions)
            }
        };

        var logContent = new StringContent(JsonSerializer.Serialize(logQuery, jsonOptions), Encoding.UTF8,
            "application/json");

        var logResponse = await client.PostAsync("", logContent).ConfigureAwait(false);

        if (!logResponse.IsSuccessStatusCode)
        {
            logger.LogError($"Failed to log user access: {logResponse.StatusCode}");

            // return error response to user even if the log fails
            context.Response.StatusCode = 500; // Internal Server Error
            await context.Response.WriteAsJsonAsync(new { errorMessage = "Failed to log user access." })
                .ConfigureAwait(false);
            return;
        }

        // store the user data in the context for further use
        context.Items["MondayUserData"] = userData;

        logger.LogInformation($"Access granted for user {userData?.Data?.Me?.Id} to path {apiPath}");

        await _next(context).ConfigureAwait(false);
    }
}