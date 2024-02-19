using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Artemis.Tools.PluginUploader;

public class WorkshopClient : IDisposable
{
    private const string Url = "https://workshop.artemis-rgb.com";
    private readonly HttpClient _httpClient;

    public WorkshopClient(string pat)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(Url);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);
    }

    public async Task<Items[]> GetPluginInfos(params Guid[] pluginGuids)
    {
        const string QUERY = """
                             query GetPluginInfos($pluginGuids: [UUID]) {
                                 pluginInfos(
                                     take: 100
                                     where: {
                                         pluginGuid: {
                                             in: $pluginGuids
                                         }
                                     }
                                 ) {
                                     items {
                                         pluginGuid
                                         entryId
                                         entry {
                                             latestRelease {
                                                 version
                                             }
                                         }
                                     }
                                 }
                             }
                                                          
                             """;

        var variables = new
        {
            pluginGuids
        };

        var body = new
        {
            query = QUERY,
            variables
        };
        var response = await _httpClient.PostAsJsonAsync("graphql", body);
        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RootObject>(result)!.data.pluginInfos.items;
    }

    public async Task<Items> GetPluginInfo(Guid pluginGuid)
    {
        var result = await GetPluginInfos(pluginGuid);
        var pluginInfo = result.FirstOrDefault(x => x.pluginGuid == pluginGuid.ToString());
        if (pluginInfo == null)
            throw new Exception("Plugin not found");

        return pluginInfo;
    }

    public async Task PublishPlugin(Stream pluginZip, long entryId, CancellationToken cancellationToken = default)
    {
        pluginZip.Seek(0, SeekOrigin.Begin);
        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(pluginZip);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(streamContent, "file", "plugin.zip");

        var response = await _httpClient.PostAsync($"releases/upload/{entryId}", content, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}