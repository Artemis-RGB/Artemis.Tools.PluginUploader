using System.IO.Compression;
using System.Text.Json;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace Artemis.Tools.PluginUploader.Commands;

[Command("upload", Description = "Uploads a single plugin")]
public class Upload : ICommand
{
    [CommandOption("pat", 'p', Description = "Personal Access Token")]
    public required string PersonalAccessToken { get; set; }

    [CommandOption("folder", 'f', Description = "Folder to the compiled plugin. Should contain a plugin.json")]
    public required string PluginFolder { get; set; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        if (string.IsNullOrWhiteSpace(PersonalAccessToken))
            throw new CommandException("Personal Access Token invalid");
        if (!Directory.Exists(PluginFolder))
            throw new CommandException("Plugin folder does not exist");
        var pluginJsonPath = Path.Combine(PluginFolder, "plugin.json");
        if (!File.Exists(pluginJsonPath))
            throw new CommandException("plugin.json does not exist in the plugin folder");

        using var workshopClient = new WorkshopClient(PersonalAccessToken);

        var localPluginInfo = JsonSerializer.Deserialize<PluginInfo>(await File.ReadAllTextAsync(pluginJsonPath));
        if (localPluginInfo == null)
            throw new CommandException("plugin.json is invalid");

        Items? remotePluginInfo;
        try
        {
            remotePluginInfo = await workshopClient.GetPluginInfo(localPluginInfo.Guid);
        }
        catch
        {
            //the plugin  has never been published to the workshop. tell the user and exit
            await console.Output.WriteLineAsync("Plugin has never been published to the workshop, skipping");
            return;
        }

        var remoteVersion = remotePluginInfo.entry.latestRelease.version;
        if (localPluginInfo.Version <= remoteVersion)
        {
            await console.Output.WriteLineAsync($"Local version {localPluginInfo.Version} is not newer than remote version {remoteVersion}");
            return;
        }
        
        await console.Output.WriteLineAsync($"Uploading {localPluginInfo.Name} v{localPluginInfo.Version}...");

        using var zipStream = new MemoryStream();
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);
        var files = Directory.EnumerateFiles(PluginFolder, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var entry = archive.CreateEntry(Path.GetRelativePath(PluginFolder, file));
            await using var entryStream = entry.Open();
            await using var fileStream = File.OpenRead(file);
            await fileStream.CopyToAsync(entryStream);
        }

        zipStream.Seek(0, SeekOrigin.Begin);

        await workshopClient.PublishPlugin(zipStream, remotePluginInfo.entryId);
        await console.Output.WriteLineAsync("Plugin uploaded");
    }
}