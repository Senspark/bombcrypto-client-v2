using System.Collections;
using System.Collections.Generic;

using App;

using UnityEngine;

public class CheckAndDestroyCamera : MonoBehaviour
{
    void Start()
    {
        //Tự destroy camera này nếu là Ton vì Ton sử dụng canvas có type là Screen Space và sử dụng camera riêng
        // if (AppConfig.IsTon()) {
        //     Destroy(gameObject);
        // }
    }
}
