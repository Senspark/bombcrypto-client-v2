using App;

using Cysharp.Threading.Tasks;

using Share.Scripts;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

public interface ILoginWith 
{
    UniTask<JwtData> GetJwtData(IUnityReactSupportMethod unityReact, HandshakeType type);
}
