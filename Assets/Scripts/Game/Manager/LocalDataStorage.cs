using CodeStage.AntiCheat.Storage;

namespace App
{
    public class LocalDataStorage : IDataStorage
    {
        public int GetInt(string key, int defaultValue)
        {
            return ObscuredPrefs.GetInt(key, defaultValue);
        }

        public float GetFloat(string key, float defaultValue)
        {
            return ObscuredPrefs.GetFloat(key, defaultValue);
        }

        public string GetString(string key, string defaultValue)
        {
            return ObscuredPrefs.GetString(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            ObscuredPrefs.SetInt(key, value);
        }

        public void SetFloat(string key, float value)
        {
            ObscuredPrefs.SetFloat(key, value);
        }

        public void SetString(string key, string value)
        {
            ObscuredPrefs.SetString(key, value);
        }
    }
}
