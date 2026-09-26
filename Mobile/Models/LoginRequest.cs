using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;
public class LoginRequest
{
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
