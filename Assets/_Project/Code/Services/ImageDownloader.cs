using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace _Project.Code.Services
{
    public sealed class ImageDownloader
    {
        private const string ImageUrl = "https://picsum.photos/200";
        private const int TimeoutSeconds = 20;

        public async Task<byte[]> DownloadImageAsync()
        {
            using (var request = UnityWebRequest.Get(ImageUrl))
            {
                request.timeout = TimeoutSeconds;
                request.redirectLimit = 5;
                await request.SendAsync();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new InvalidOperationException($"Avatar download failed. HTTP {request.responseCode}: {request.error}");
                }

                var bytes = request.downloadHandler.data;
                if (!IsImageDataValid(bytes))
                {
                    throw new InvalidOperationException("Avatar download did not contain a readable image.");
                }

                return bytes;
            }
        }

        public static bool IsImageDataValid(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return false;
            }

            if (!TryCreateTexture(bytes, out var texture))
            {
                return false;
            }

            UnityEngine.Object.Destroy(texture);
            return true;
        }

        public static bool TryCreateTexture(byte[] bytes, out Texture2D texture)
        {
            texture = null;
            if (bytes == null || bytes.Length == 0)
            {
                return false;
            }

            var candidate = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!candidate.LoadImage(bytes, true))
                {
                    UnityEngine.Object.Destroy(candidate);
                    return false;
                }

                texture = candidate;
                return true;
            }
            catch (Exception)
            {
                UnityEngine.Object.Destroy(candidate);
                return false;
            }
        }
    }
}
