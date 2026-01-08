mergeInto(LibraryManager.library, {
    WebGLUtils_OpenUrl: function(url, newTab) {
        const _url = UTF8ToString(url);
        const target = newTab ? "_blank" : "_self";
        window.open(_url, target);
    },
    
    WebGLUtils_IsCetusIntercepted: function() {
        if (typeof(window.cetusIntercepted) === 'undefined') {
            function onCetusMessage() {
                window.cetusIntercepted = true;
            }
            window.addEventListener("cetusMsgIn", onCetusMessage);
            window.addEventListener("cetusMsgOut", onCetusMessage);
            window.cetusIntercepted = false;
        }
        if (typeof(cetus) !== 'undefined' && 
            typeof(cetus.speedhack) !== 'undefined' && 
            typeof(cetus.speedhack.multiplier) !== 'undefined') {
            return cetus.speedhack.multiplier > 1;
        }
        return window.cetusIntercepted === true;
    },
    
    WebGLUtils_InitBeforeUnloadEvent: function () {
        window.addEventListener('beforeunload', function (e) {
            // e.preventDefault();
            // e.returnValue = '';
            // delete e['returnValue'];
            window.unityInstance.SendMessage('WebGLOnApplicationQuit', 'OnBeforeUnload');
        });
    },
    
    WebGLUtils_GetTime: function () {
        const today = new Date();
        var unixTime = today.getTime();
        return unixTime;
    },
    
    WebGLUtils_GetDomain: function () {
        var domain = window.top.location.href;
        var bufferSize = lengthBytesUTF8(domain) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(domain, buffer, bufferSize);
        return buffer;
    }
});