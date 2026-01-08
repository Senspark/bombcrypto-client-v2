using JetBrains.Annotations;

using UnityEngine;

namespace Senspark {
    public static class GameObjectUtils {
        public static void ClearChildren(this GameObject gameObject) {
#if UNITY_EDITOR
            foreach (Transform child in gameObject.transform) {
                Object.DestroyImmediate(child.gameObject);
            }
#else
            foreach (Transform child in gameObject.transform) {
                Object.Destroy(child.gameObject);
            }
#endif
        }

        public static void ClearChildren(this Transform transform) {
#if UNITY_EDITOR
            foreach (Transform child in transform) {
                Object.DestroyImmediate(child.gameObject);
            }
#else
            foreach (Transform child in transform) {
                Object.Destroy(child.gameObject);
            }
#endif
        }

        /// <summary>
        /// Tạo 1 Game Object
        /// </summary>
        /// <param name="parent">GameObject cha (có thể null)</param>
        /// <param name="name">Tên cần đặt</param>
        /// <returns>Game Object vừa tạo</returns>
        public static GameObject CreateNewGameObject([CanBeNull] Transform parent, string name) {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            return go;
        }

        /// <summary>
        /// Tạo 1 Game Object và thêm 1 component vào đó
        /// </summary>
        /// <param name="parent">GameObject cha (có thể null)</param>
        /// <param name="name">Tên cần đặt</param>
        /// <typeparam name="T">Component cần thêm</typeparam>
        /// <returns>Game Object với Component vừa tạo</returns>
        public static T CreateNewGameObject<T>([CanBeNull] Transform parent, string name) where T : MonoBehaviour {
            return CreateNewGameObject(parent, name).AddComponent<T>();
        }
    }
}