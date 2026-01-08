using System;
using UnityEngine;

namespace Engine.Utils
{
    public class PlayTimer : MonoBehaviour
    {
        private float timer;
        private bool running;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            timer = 0;
            //PlayerPrefs.SetInt(nameof(ConversionManager.numMinutePlayed), 0);
        }

        private void Update()
        {
            if (running)
            {
                timer += Time.deltaTime;
                if (timer >= 60)
                {
                    timer = 0;
                    //ConversionManager.TrackMinutePlayed();
                }
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            running = focus;
        }
    }
}
