using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace _Project.Code.Services
{
    public sealed class ApiClient
    {
        private const int TimeoutSeconds = 20;
        private const int RedirectLimit = 5;

        public async Task<string> GetTextAsync(string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                ConfigureRequest(request);
                request.SetRequestHeader("Accept", "application/json");

                await SendRequestAsync(request);
                return request.downloadHandler.text;
            }
        }

        public async Task<byte[]> GetBytesAsync(string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                ConfigureRequest(request);
                await SendRequestAsync(request);
                return request.downloadHandler.data;
            }
        }

        private void ConfigureRequest(UnityWebRequest request)
        {
            request.timeout = TimeoutSeconds;
            request.redirectLimit = RedirectLimit;
        }

        private async Task SendRequestAsync(UnityWebRequest request)
        {
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(
                    $"HTTP request failed. Status: {request.responseCode}. {request.error}");
            }
        }
    }
}
