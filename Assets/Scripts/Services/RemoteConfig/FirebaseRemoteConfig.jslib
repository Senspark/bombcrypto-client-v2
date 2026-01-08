mergeInto(LibraryManager.library, {

    FetchAndActivate: function(configValue, callback) {
        const remoteConfig = firebase.remoteConfig();
        remoteConfig.settings = {
            minimumFetchIntervalMillis: 3600000,
        };
        const defaultConfig = UTF8ToString(configValue);
        remoteConfig.defaultConfig = defaultConfig;
        remoteConfig.fetchAndActivate().then(function () {
            Module['dynCall_v'](callback);
        });
    },

    GetDoubleValue: function(key) {
        const k = UTF8ToString(key);
        const v = firebase.remoteConfig().getValue(k).asNumber();
        return v;
    },
    
    GetStringValue: function(key) {
        const k = UTF8ToString(key);
        const value = firebase.remoteConfig().getValue(k).asString();
        var bufferSize = lengthBytesUTF8(value) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(value, buffer, bufferSize);
        return buffer;
    },
    
    GetBoolValue: function(key) {
        const k = UTF8ToString(key);
        const v = firebase.remoteConfig().getValue(k).asBoolean();
        return v;
    }
    
});
