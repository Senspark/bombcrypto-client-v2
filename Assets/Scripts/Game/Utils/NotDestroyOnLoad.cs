using UnityEngine;

namespace App {
    public class NotDestroyOnLoad : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }
    }
}