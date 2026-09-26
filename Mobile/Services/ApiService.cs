using EmergencyHomeServiceMobile.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace EmergencyHomeServiceMobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://10.0.2.2:5127/")
        };
    }

    /////////////////////////////////////////////////////////////////////////////////
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/Auth/login",
                request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return await response.Content.ReadFromJsonAsync<LoginResponse>(
                options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            return null;
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////
    public async Task<LoginResponse?> RegisterAsync(
    RegisterRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/Auth/register",
                request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<LoginResponse>(
            responseBody,
            options);
    }
    /////////////////////////////////////////////////////////////////////////////////////////////
    public async Task<UserProfileResponse?>
    GetMyProfileAsync()
    {
        string token =
            Preferences.Get("jwt_token", "");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/Users/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(request);

        var body =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{body}");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<UserProfileResponse>(
            body,
            options);
    }

    //////////////////////////////////////////////////////////////////////////////
    public async Task<UserProfileResponse?>
    UpdateMyProfileAsync(
    string fullName,
    string phone,
    string? avatarUrl)
    {
        string token =
            Preferences.Get("jwt_token", "");

        var body = new
        {
            fullName,
            phone,
            avatarUrl
        };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                "api/Users/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Content =
            JsonContent.Create(body);

        var response =
            await _httpClient.SendAsync(request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");

        return JsonSerializer.Deserialize<UserProfileResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
    ////////////////////////////////////////////////////////////////////////////////
    public async Task<WorkerProfileResponse?>
    GetMyWorkerProfileAsync()
    {
        string token =
            Preferences.Get("jwt_token", "");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/Workers/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(request);

        var body =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{body}");

        return JsonSerializer.Deserialize<WorkerProfileResponse>(
            body,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    ////////////////////////////////////////////////////////////////////
    public async Task<bool> UpdateWorkerProfileAsync(
    string? bio,
    int experienceYears)
    {
        string token =
            Preferences.Get("jwt_token", "");

        var body = new
        {
            bio,
            experienceYears
        };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                "api/Workers/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Content =
            JsonContent.Create(body);

        var response =
            await _httpClient.SendAsync(request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");

        return true;
    }

    //////////////////////////////////////////////////////////////////////////////////
    public async Task<List<Service>?> GetServicesAsync()
    {
        try
        {
            string token = Preferences.Get("jwt_token", "");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Services");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"GetServices failed: {response.StatusCode}");

                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return await response.Content.ReadFromJsonAsync<List<Service>>(
                options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            return null;
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////
    public async Task<bool> UpdateWorkerServicesAsync(
    List<int> serviceIds)
    {
        string token =
            Preferences.Get("jwt_token", "");

        var body = new
        {
            serviceIds
        };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                "api/Workers/services");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Content =
            JsonContent.Create(body);

        var response =
            await _httpClient.SendAsync(request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");

        return true;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////

    public async Task<ServiceRequestResponse?> CreateServiceRequestAsync(
    CreateServiceRequestRequest request)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "api/ServiceRequests");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<ServiceRequestResponse>(
            responseBody,
            options);
    }


    public async Task<MatchingWorkersResponse?> GetMatchingWorkersAsync(
    long requestId)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/ServiceRequests/{requestId}/matching-workers");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return System.Text.Json.JsonSerializer.Deserialize<MatchingWorkersResponse>(
            responseBody,
            options);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public async Task<WorkerAvailableRequestsResponse?>
    GetAvailableRequestsForWorkerAsync()
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "api/ServiceRequests/worker-available");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return System.Text.Json.JsonSerializer
            .Deserialize<WorkerAvailableRequestsResponse>(
                responseBody,
                options);
    }


    //////////////////////////////////////////////////////////////////////////
    ///
    public async Task<bool> AcceptRequestAsync(long requestId)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/ServiceRequests/{requestId}/accept");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        return true;
    }
    //////////////////////////////////////////////////////////////
    public async Task<bool> RejectRequestAsync(
    long requestId,
    string? reason)
    {
        string token =
            Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "Không tìm thấy JWT.");

        var body = new RejectServiceRequestRequest
        {
            Reason = reason
        };

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/ServiceRequests/{requestId}/reject");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        httpRequest.Content =
            JsonContent.Create(body);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");

        return true;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    public async Task<WorkerLocationResponse?> UpdateWorkerLocationAsync(
    double latitude,
    double longitude)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        var requestBody = new UpdateWorkerLocationRequest
        {
            Latitude = latitude,
            Longitude = longitude
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            "api/WorkerLocations");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        httpRequest.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return System.Text.Json.JsonSerializer.Deserialize<WorkerLocationResponse>(
            responseBody,
            options);
    }
    /////////////////////////////////////////////////////////////////////////////////
    public async Task<WorkerLocationResponse?> GetMyWorkerLocationAsync()
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "api/WorkerLocations");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return System.Text.Json.JsonSerializer.Deserialize<WorkerLocationResponse>(
            responseBody,
            options);
    }
    /////////////////////////////////////////////////////////////////////////////////////////////
    public async Task<UpdateRequestStatusResponse?>
    UpdateRequestStatusAsync(
        long requestId,
        string status,
        string? note = null)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        var requestBody = new UpdateRequestStatusRequest
        {
            Status = status,
            Note = note
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/ServiceRequests/{requestId}/status");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        httpRequest.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return System.Text.Json.JsonSerializer.Deserialize<
            UpdateRequestStatusResponse>(
                responseBody,
                options);
    }
    ///////////////////////////////////////////////////////////////
    public async Task<bool> CreateRatingAsync(
    long requestId,
    int score,
    string? comment)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        var requestBody = new
        {
            score = score,
            comment = comment
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/Ratings/{requestId}");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        httpRequest.Content =
            JsonContent.Create(requestBody);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        return true;
    }

    //////////////////////////////////////////////////////////////////////////////////////
    public async Task<RatingResponse?>
GetMyRatingAsync(long requestId)
    {
        string token =
            Preferences.Get("jwt_token", "");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Ratings/request/{requestId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(request);

        var body =
            await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{body}");

        return JsonSerializer.Deserialize<RatingResponse>(
            body,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    ///////////////////////////////////////////////////////////////////////////////////
    public async Task<List<WorkerRatingResponse>>
GetMyRatingsAsync()
    {
        string token =
            Preferences.Get("jwt_token", "");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/Ratings/worker");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(request);

        var body =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{body}");

        return JsonSerializer.Deserialize<
            List<WorkerRatingResponse>>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? new List<WorkerRatingResponse>();
    }
    ////////////////////////////////////////////////////////////////////////////
    public async Task<List<ServiceRequestResponse>> GetMyRequestsAsync()
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "api/ServiceRequests/my");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<
            List<ServiceRequestResponse>>(
            responseBody,
            options) ?? new List<ServiceRequestResponse>();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    public async Task<ServiceRequestResponse?> GetRequestDetailAsync(
    long requestId)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/ServiceRequests/{requestId}");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<ServiceRequestResponse>(
            responseBody,
            options);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////
    public async Task<WorkerAvailabilityResponse?>
UpdateWorkerAvailabilityAsync(
    bool isOnline,
    bool isAvailable)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        var requestBody = new UpdateAvailabilityRequest
        {
            IsOnline = isOnline,
            IsAvailable = isAvailable
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            "api/Workers/availability");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        httpRequest.Content =
            JsonContent.Create(requestBody);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<WorkerAvailabilityResponse>(
            responseBody,
            options);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////
    public async Task<List<NotificationResponse>> GetNotificationsAsync()
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "api/Notifications");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<
            List<NotificationResponse>>(
                responseBody,
                options) ?? new List<NotificationResponse>();
    }
    ///////////////////////////////////////////////////////////////////////
    public async Task<int> GetUnreadNotificationCountAsync()
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "api/Notifications/unread-count");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        using var document =
            JsonDocument.Parse(responseBody);

        if (document.RootElement.TryGetProperty(
                "unreadCount",
                out var unreadProperty))
        {
            return unreadProperty.GetInt32();
        }

        return 0;
    }
    ////////////////////////////////////////////////////////////////////////////////
    public async Task<bool> MarkNotificationAsReadAsync(
    long notificationId)
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/Notifications/{notificationId}/read");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        return true;
    }
    //////////////////////////////////////////////////////////////////////////////
    public async Task<bool> MarkAllNotificationsAsReadAsync()
    {
        string token = Preferences.Get("jwt_token", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Không tìm thấy JWT. Vui lòng đăng nhập lại.");
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            "api/Notifications/read-all");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.SendAsync(httpRequest);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}\n{responseBody}");
        }

        return true;
    }
    ////////////////////////////////////







}

