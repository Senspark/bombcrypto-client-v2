using System;
using System.Collections.Generic;

using UnityEngine;

namespace Utils
{
    using ClickCallback = Action;

    public class ClickHelper
    {
        private class ClickInfo
        {
            public float DownTime { get; set; }
            public float UpTime { get; set; }
        }

        private readonly List<ClickInfo> clicksInfo = new List<ClickInfo>();

        private ClickInfo currentInfo;
        private ClickCallback singleClickCallback;
        private ClickCallback doubleClickCallback;

        public void OnSingleClick(ClickCallback callback)
        {
            singleClickCallback = callback;
        }

        public void OnDoubleClick(ClickCallback callback)
        {
            doubleClickCallback = callback;
        }

        public float SingleClickThreshold { get; set; }
        public float DoubleClickThreshold { get; set; }
        public bool DoubleClickEnabled { get; set; }

        public void Down()
        {
            currentInfo = new ClickInfo
            {
                DownTime = Time.time
            };
        }

        public void Up()
        {
            currentInfo.UpTime = Time.time;
            clicksInfo.Add(currentInfo);
            currentInfo = null;
        }

        public void Update()
        {
            if (DoubleClickEnabled)
            {
                ProcessClicks();
            }
            else
            {
                ProcessSingleClicks();
            }
        }

        private void ProcessClicks()
        {
            var index = 0;
            while (index < clicksInfo.Count)
            {
                if (index + 1 < clicksInfo.Count)
                {
                    if (clicksInfo[index + 1].UpTime - clicksInfo[index].DownTime <= DoubleClickThreshold)
                    {
                        doubleClickCallback?.Invoke();
                        index += 2;
                        continue;
                    }
                    if (clicksInfo[index].UpTime - clicksInfo[index].DownTime <= SingleClickThreshold)
                    {
                        singleClickCallback?.Invoke();
                        index += 1;
                        continue;
                    }
                    index += 1;
                    continue;
                }
                var now = Time.time;
                if (now - clicksInfo[index].DownTime > DoubleClickThreshold &&
                    clicksInfo[index].UpTime - clicksInfo[index].DownTime <= SingleClickThreshold)
                {
                    singleClickCallback?.Invoke();
                    index += 1;
                    continue;
                }
                break;
            }
            clicksInfo.RemoveRange(0, index);
        }

        private void ProcessSingleClicks()
        {
            foreach (var info in clicksInfo)
            {
                if (info.UpTime - info.DownTime <= SingleClickThreshold)
                {
                    singleClickCallback?.Invoke();
                }
            }
            clicksInfo.Clear();
        }
    }
}