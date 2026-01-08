namespace App
{
    public interface IDataStorage
    {
        int GetInt(string key, int defaultValue);
        float GetFloat(string key, float defaultValue);
        string GetString(string key, string defaultValue);

        void SetInt(string key, int value);
        void SetFloat(string key, float value);
        void SetString(string key, string value);
    }
}
