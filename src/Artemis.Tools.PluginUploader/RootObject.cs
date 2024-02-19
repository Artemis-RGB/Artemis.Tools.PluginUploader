namespace Artemis.Tools.PluginUploader;

public class RootObject
{
    public Data data { get; set; }
}


public class Data
{
    public PluginInfos pluginInfos { get; set; }
}

public class PluginInfos
{
    public Items[] items { get; set; }
}

public class Items
{
    public string pluginGuid { get; set; }
    public long entryId { get; set; }
    public Entry entry { get; set; }
}

public class Entry
{
    public LatestRelease latestRelease { get; set; }
}

public class LatestRelease
{
    public Version version { get; set; }
}