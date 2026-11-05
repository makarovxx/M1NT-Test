using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace _Project.Code.Services
{
    internal static class UnityWebRequestExtensions
    {
        public static Task SendAsync(this UnityWebRequest request)
        {
            var completion = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            if (operation.isDone)
            {
                return Task.CompletedTask;
            }

            operation.completed += HandleRequestCompleted;
            return WaitForCompletionAsync();

            async Task WaitForCompletionAsync()
            {
                await completion.Task;
                operation.completed -= HandleRequestCompleted;
            }

            void HandleRequestCompleted(AsyncOperation _)
            {
                completion.TrySetResult(true);
            }
        }
    }
}
