using EwAdmin.Common.Models.Setting;
using EwAdminApp.Models;

namespace EwAdminApp.Events;

public class LoginEvent (LoginSettings? loginSettings)
{
    public LoginSettings? LoginSettings { get; } = loginSettings;
}