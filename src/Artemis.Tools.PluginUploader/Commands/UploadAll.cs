using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Artemis.Tools.PluginUploader.Commands;

[Command("upload-all", Description = "Uploads all plugins in a solution")]
public class UploadAll : ICommand
{
    [CommandOption("pat", 'p', Description = "Personal Access Token")]
    public required string PersonalAccessToken { get; set; }
    
    [CommandOption("folder", 'f', Description = "Path to the folder containing ")]
    public required string RootFolder { get; set; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var pluginFolders = Directory.GetDirectories(RootFolder, "publish", SearchOption.AllDirectories)
            .Where(d => File.Exists(Path.Combine(d, "plugin.json")))
            .ToList();
        
        await console.Output.WriteLineAsync($"Found {pluginFolders.Count} plugins to upload");
        
        foreach (var pluginFolder in pluginFolders)
        {
            await console.Output.WriteLineAsync($"Uploading {pluginFolder}");
            var uploadCommand = new Upload
            {
                PersonalAccessToken = PersonalAccessToken,
                PluginFolder = pluginFolder
            };
            await uploadCommand.ExecuteAsync(console);
        }
        
        await console.Output.WriteLineAsync("All plugins uploaded");
    }
}