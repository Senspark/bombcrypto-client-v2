using App;
using UnityEngine;

public class HighLightNavigate : MonoBehaviour
{
    private void Start() {
        if (!AppConfig.IsWebGL()) {
            gameObject.SetActive(false);
        }
    }
}
