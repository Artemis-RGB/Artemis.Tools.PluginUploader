namespace Artemis.Tools.PluginUploader;

public class PluginInfo
{
    public Guid Guid { get; set; }
    public Version Api { get; set; }
    public string Name { get; set; }
    public Version Version { get; set; }
}