//https://forum.unity.com/threads/anti-cheat-toolkit-stop-cheaters-easily.196578/page-12

using UnityEngine;
using System;
using System.Linq;
using Newtonsoft.Json;
using CodeStage.AntiCheat.ObscuredTypes;

public class ObscuredValueConverter : JsonConverter
{

    private readonly Type[] _types = {
            typeof(ObscuredInt),
            typeof(ObscuredFloat),
            typeof(ObscuredDouble),
            typeof(ObscuredDecimal),
            typeof(ObscuredChar),
            typeof(ObscuredByte),
            typeof(ObscuredBool),
            typeof(ObscuredLong),
            typeof(ObscuredQuaternion),
            typeof(ObscuredSByte),
            typeof(ObscuredShort),
            typeof(ObscuredUInt),
            typeof(ObscuredULong),
            typeof(ObscuredUShort),
            typeof(ObscuredVector2),
            typeof(ObscuredVector3),
            typeof(ObscuredString)
        };

    #region implemented abstract members of JsonConverter

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is ObscuredInt)
        {
            writer.WriteValue((int)(ObscuredInt)value);
        }
        else if (value is ObscuredBool)
        {
            writer.WriteValue((bool)(ObscuredBool)value);
        }
        else if (value is ObscuredFloat)
        {
            writer.WriteValue((float)(ObscuredFloat)value);
        }
        else if (value is ObscuredDouble) {
            writer.WriteValue((ObscuredDouble)value);
        }
        else
        {
            Debug.Log("ObscuredValueConverter type " + value.GetType().ToString() + " not implemented");
            writer.WriteValue(value.ToString());
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value != null)
        {
            if (objectType == typeof(ObscuredInt))
            {
                ObscuredInt value = Convert.ToInt32(reader.Value);
                return value;
            }
            else if (objectType == typeof(ObscuredBool))
            {
                ObscuredBool value = Convert.ToBoolean(reader.Value);
                return value;
            }
            else if (objectType == typeof(ObscuredFloat))
            {
                ObscuredFloat value = Convert.ToSingle(reader.Value);
                return value;
            }
            else if (objectType == typeof(ObscuredDouble)) {
                ObscuredDouble value = Convert.ToDouble(reader.Value);
                return value;
            }
            else
            {
                Debug.LogError("Code not implemented yet!");
            }
        }
        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return _types.Any(t => t == objectType);
    }

    #endregion
}
