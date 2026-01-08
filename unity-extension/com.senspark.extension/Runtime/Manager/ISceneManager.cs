using System.Threading.Tasks;

using JetBrains.Annotations;

using UnityEngine;

namespace Senspark {
    [Service(nameof(ISceneManager))]
    public interface ISceneManager {
        /// <summary>
        /// Loads the specified scene.
        /// </summary>
        [NotNull]
        Task<T> LoadScene<T>([NotNull] string sceneName) where T : MonoBehaviour;
    }
}