using System.Threading.Tasks;
using UnityEngine.Networking;

namespace EmployeeContacts.Services
{
    internal static class UnityWebRequestExtensions
    {
        public static Task SendAsync(this UnityWebRequest request)
        {
            var completion = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => completion.TrySetResult(true);
            return completion.Task;
        }
    }
}
