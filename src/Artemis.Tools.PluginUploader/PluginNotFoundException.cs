namespace Artemis.Tools.PluginUploader;

public class PluginNotFoundException : Exception
{
    public Guid PluginGuid { get; }
    
    public PluginNotFoundException(Guid pluginGuid)
    {
        PluginGuid = pluginGuid;
    }
}