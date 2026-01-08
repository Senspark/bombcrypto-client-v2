using System;
using System.Collections.Generic;

namespace Senspark.Server {
    public interface IOutputData {
        bool GetBool(string key);
        long GetLong(string key);
        double GetDouble(string key);
        string GetString(string key);
        DateTime GetDate(string key);
        Dictionary<string, bool> GetBoolDict(string key);
        Dictionary<string, long> GetLongDict(string key);
        Dictionary<string, double> GetDoubleDict(string key);
        Dictionary<string, string> GetStringDict(string key);
        Dictionary<string, DateTime> GetDateDict(string key);
        List<bool> GetBoolList(string key);
        List<long> GetLongList(string key);
        List<double> GetDoubleList(string key);
        List<string> GetStringList(string key);
        List<DateTime> GetDateList(string key);
    }
}