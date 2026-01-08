using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Share.Scripts.PrefabsManager
{
    public interface IPrefabLoader : ILoader {
        bool Contains<T>() where T : MonoBehaviour;
        UniTask<T> Instantiate<T>() where T : MonoBehaviour;
    }
}