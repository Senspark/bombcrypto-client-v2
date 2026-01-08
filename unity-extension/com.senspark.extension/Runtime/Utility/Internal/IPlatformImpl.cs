using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public interface IPlatformImpl {
        [NotNull]
        string GetVersionCode();

        UniTask<TrackingAuthorizationStatus> RequestTrackingAuthorization();

        [NotNull]
        string GetSha1();

        UniTask<string> GetAdvertisingId();
        
        bool IsApplicationInstalled(string appId);

        bool OpenApplication(string appId);

        UniTask<string> FetchSocket(string hostName, int portNumber, string message);

        UniTask<bool> TestConnection(string hostName, float timeOutSeconds);

        void AddNativePlugin(string className);
        
        void RemoveNativePlugin(string className);
    }
}