using System.Collections;
using System.Collections.Generic;

using App;

using UnityEngine;
using UnityEngine.UI;

public static class ScreenUtils
{
    public static float ConvertPixelsToDp(float pixels)
    {
#if UNITY_ANDROID || UNITY_IOS
        //https://developer.android.com/training/multiscreen/screendensities#java
        //160dpi; the "baseline" density)

        if (Screen.dpi > 0)
        {
            return pixels / (Screen.dpi / 160);
        }
        return pixels;
#else
        return pixels;
#endif
    }

    public static float ConvertPixelsToUnit(float pixels)
    {
        var ratio = 600.0f / PhysicSreenHeight();
        return pixels * ratio;
    }

    public static float ConvertUnitToPixels(float unit)
    {
        var ratio = PhysicSreenHeight() / 600.0f;
        return unit * ratio;
    }

    public static float GetSizeOfWord(Text text, string word)
    {
        var width = 0.0f;
        CharacterInfo charInfo;
        foreach (var c in word)
        {
            text.font.GetCharacterInfo(c, out charInfo, text.fontSize);

            width += charInfo.advance;

        }
        return width;
    }

    public static float PhysicScreenWidth()
    {
        var width = Screen.width;
#if UNITY_IOS
        if (width == 1920)
        {
            width = 2208;
        }
#endif
        return width;
    }

    public static float PhysicSreenHeight()
    {
        var height = Screen.height;
#if UNITY_IOS
        if (height == 1080)
        {
            height = 1242;
        }
#endif
        return height;
    }

    public static float GetScreenRatio() {
        return (float) Screen.width / Screen.height;
    }

    public static bool IsIPadScreen() {
        if (AppConfig.IsTon() || AppConfig.IsWebGL()) {
            return false;
        }
        return GetScreenRatio() <= 1.4f;
    }

}
