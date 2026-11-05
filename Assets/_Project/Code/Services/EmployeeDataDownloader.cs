using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace EmployeeContacts.Services
{
    public sealed class EmployeeDataDownloader
    {
        // The user-facing Drive URL requires a direct-download endpoint in a player build.
        private const string DownloadUrl = "https://drive.usercontent.google.com/download?id=1YvE6Y5-vxVWXrrYyb83ssqMfVUUkndLw&export=download&confirm=t";
        private const int TimeoutSeconds = 20;

        public async Task<string> DownloadJsonAsync()
        {
            using var request = UnityWebRequest.Get(DownloadUrl);
            request.timeout = TimeoutSeconds;
            request.SetRequestHeader("Accept", "application/json");
            await request.SendAsync();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Employee download failed. HTTP {request.responseCode}: {request.error}");
            }

            var json = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
            {
                throw new InvalidOperationException("Employee download did not contain a JSON object.");
            }

            return json;
        }
    }
}
