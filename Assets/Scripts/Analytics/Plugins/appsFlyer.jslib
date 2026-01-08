mergeInto(LibraryManager.library, {
    initializeAppsFlyer: function(webDevKey) {
        var webDevKey = UTF8ToString(webDevKey);
        !function (t, e, n, s, a, c, i, o, p) {
            t.AppsFlyerSdkObject = a, t.AF = t.AF || function () {
                (t.AF.q = t.AF.q || []).push([Date.now()].concat(Array.prototype.slice.call(arguments)))
            },
            t.AF.id = t.AF.id || i, t.AF.plugins = {}, o = e.createElement(n), p = e.getElementsByTagName(n)[0], o.async = 1,
            o.src = "https://websdk.appsflyer.com?" + (c.length > 0 ? "st=" + c.split(",").sort().join(",") + "&" : "") + (i.length > 0 ? "af_id=" + i : ""),
            p.parentNode.insertBefore(o, p)
        }(window, document, "script", 0, "AF", "pba", { pba: { webAppId: webDevKey } })
    },
    
    setAppsFlyerCUID: function(userIdPtr) {
        var cuid = UTF8ToString(userIdPtr);
        AF("pba", "setCustomerUserId", cuid);
    },
    
    logEventAppsFlyer: function(eventNamePtr, eventDataJsonPtr) {
        var eventName = UTF8ToString(eventNamePtr);
        var d = {
            "eventType": "EVENT",
            "eventName": eventName
        };
        if (eventDataJsonPtr) {
            d.eventValue = JSON.parse(UTF8ToString(eventDataJsonPtr));
        }
        AF("pba", "event", d);
    }
});