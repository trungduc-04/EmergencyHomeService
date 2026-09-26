using Microsoft.Maui.Storage;

namespace EmergencyHomeServiceMobile.Services;

public static class SessionService
{
    public static void Clear()
    {
        Preferences.Remove("jwt_token");
        Preferences.Remove("user_id");
        Preferences.Remove("user_role");
        Preferences.Remove("full_name");
        Preferences.Remove("email");
        Preferences.Remove("phone");

        // Xóa request hiện tại của phiên đăng nhập cũ
        Preferences.Remove("current_request_id");
    }
}