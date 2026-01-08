mergeInto(LibraryManager.library, {

    initialize: function(apiKeyPtr, authDomainPtr, projectIdPtr, storageBucketPtr, messagingSenderIdPtr, appIdPtr, measurementIdPtr) {
        const apiKey = UTF8ToString(apiKeyPtr);
        const authDomain = UTF8ToString(authDomainPtr);
        const projectId = UTF8ToString(projectIdPtr);
        const storageBucket = UTF8ToString(storageBucketPtr);
        const messagingSenderId = UTF8ToString(messagingSenderIdPtr);
        const appId = UTF8ToString(appIdPtr);
        const measurementId = UTF8ToString(measurementIdPtr);
        
        const firebaseConfig = {
            apiKey: apiKey,
            authDomain: authDomain,
            projectId: projectId,
            storageBucket: storageBucket,
            messagingSenderId: messagingSenderId,
            appId: appId,
            measurementId: measurementId
        };
        firebase.initializeApp(firebaseConfig);
        firebase.analytics();
    },
    
    logEvent: function(eventNamePtr) {
        var eventName = UTF8ToString(eventNamePtr);
        firebase.analytics().logEvent(eventName);
    },
    
    logEvent: function(eventNamePtr, paramsPtr) {
        var params = UTF8ToString(paramsPtr)
        var eventName = UTF8ToString(eventNamePtr);
        var arr = {"creator":""};
        var dataArray = params.split(";");
        var arrayLength = dataArray.length / 2;
        for (var i = 0; i < arrayLength; i++) {
            var keyIndex = i * 2;
            var valueIndex = i * 2 + 1;
            var key = dataArray[keyIndex];
            var value = dataArray[valueIndex];
            arr[key] = value;
        }
        firebase.analytics().logEvent(eventName, arr);
    }
    
});
