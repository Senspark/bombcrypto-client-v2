using System.Threading.Tasks;

using Senspark;

namespace Services.WebGL {
    [Service(nameof(IWebGLBridgeUtils))]
    public interface IWebGLBridgeUtils : IService {
        void CopyToClipboard(string text);
        void OpenUrl(string url);
        Task<string> GetStartTelegramParams();
        Task<bool> IsIOSBrowser();
        Task<bool> IsAndroidBrowser();
    }
}