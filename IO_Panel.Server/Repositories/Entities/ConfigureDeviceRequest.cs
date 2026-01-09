using IO_Panel.Server.Repositories.Entities;
public class ConfigureDeviceRequest
{
    public ApiDevice Device { get; set; }
    public string DisplayName { get; set; }
}