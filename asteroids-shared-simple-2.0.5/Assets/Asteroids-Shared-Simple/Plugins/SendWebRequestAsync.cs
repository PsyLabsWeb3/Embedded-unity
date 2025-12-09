using System.Threading.Tasks;
using UnityEngine.Networking;

public static class UnityWebRequestExtensions
{
    public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        var operation = request.SendWebRequest();

        operation.completed += _ => {
            tcs.TrySetResult(request);
        };

        return tcs.Task;
    }
}