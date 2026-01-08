using System;
using System.Collections.Generic;

namespace Senspark.Server {
    public class InputData {
        private readonly Dictionary<string, object> _rawData = new();

        public Dictionary<string, object> ToDictionary() {
            return _rawData;
        }

        public void SetBool(string key, bool value) {
            _rawData[key] = value;
        }

        public void SetLong(string key, long value) {
            _rawData[key] = value;
        }

        public void SetDouble(string key, double value) {
            _rawData[key] = value;
        }

        public void SetString(string key, string value) {
            _rawData[key] = value;
        }

        public void SetDate(string key, DateTime value) {
            _rawData[key] = value;
        }

        public void SetBoolDict(string key, Dictionary<string, bool> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetLongDict(string key, Dictionary<string, long> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetDoubleDict(string key, Dictionary<string, double> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetStringDict(string key, Dictionary<string, string> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetDateDict(string key, Dictionary<string, DateTime> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetBoolList(string key, List<bool> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetLongList(string key, List<long> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetDoubleList(string key, List<double> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetStringList(string key, List<string> value) {
            _rawData[key] = value.ToDataField();
        }

        public void SetDateList(string key, List<DateTime> value) {
            _rawData[key] = value.ToDataField();
        }
    }
}