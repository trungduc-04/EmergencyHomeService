namespace EmergencyHomeServiceMobile.Services;

public class TokenStorageService
{
    private const string TokenKey = "jwt_token";
    private const string UserIdKey = "user_id";
    private const string RoleKey = "user_role";
    private const string FullNameKey = "full_name";

    public async Task SaveAsync(
        string token,
        int userId,
        string role,
        string fullName)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(UserIdKey, userId.ToString());
        await SecureStorage.Default.SetAsync(RoleKey, role);
        await SecureStorage.Default.SetAsync(FullNameKey, fullName);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(TokenKey);
    }

    public async Task<int?> GetUserIdAsync()
    {
        var value = await SecureStorage.Default.GetAsync(UserIdKey);

        if (int.TryParse(value, out var userId))
            return userId;

        return null;
    }

    public async Task<string?> GetRoleAsync()
    {
        return await SecureStorage.Default.GetAsync(RoleKey);
    }

    public async Task<string?> GetFullNameAsync()
    {
        return await SecureStorage.Default.GetAsync(FullNameKey);
    }

    public void Clear()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(UserIdKey);
        SecureStorage.Default.Remove(RoleKey);
        SecureStorage.Default.Remove(FullNameKey);
    }
}