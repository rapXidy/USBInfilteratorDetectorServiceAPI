namespace USBInfilteratorDetectorServiceAPI.Services.Options;

/// <summary>
/// Platform SuperAdmin accounts configured outside the tenant auditor store.
/// </summary>
public class SuperAdminOptions
{
    public const string SectionName = "SuperAdmins";

    public List<SuperAdminAccountOptions> Accounts { get; set; } = new();
}

/// <summary>
/// A single SuperAdmin credential pair.
/// </summary>
public class SuperAdminAccountOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
