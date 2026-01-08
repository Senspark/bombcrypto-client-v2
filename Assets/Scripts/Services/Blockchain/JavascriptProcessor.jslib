mergeInto(LibraryManager.library, {
    Javascript_CallMethod: function(sender, tag, callback, callbackId, args) {
        const _sender = UTF8ToString(sender);
        const _tag = UTF8ToString(tag);
        const _callback = UTF8ToString(callback);
        const argsJson = JSON.parse(UTF8ToString(args));       
        Javascript_CallMethod(_tag, function(response) {
            const _response = JSON.stringify({
                id: callbackId,
                response: response,
            });
            unityInstance.Module.SendMessage(_sender, _callback, _response);
        }, argsJson);
    },
    Javascript_Subscribe: function(sender, tag, callback) {
        const _sender = UTF8ToString(sender);
        const _tag = UTF8ToString(tag);
        const _callback = UTF8ToString(callback);
        const invoker = function (response) {
            const _response = JSON.stringify({
                tag: _tag,
                response: response,
            });
            unityInstance.Module.SendMessage(_sender, _callback, _response);
        };
        Javascript_Subscribe(_tag, invoker);
    },
    Javascript_Unsubscribe: function(tag) {
        const _tag = UTF8ToString(tag);
        Javascript_Unsubscribe(_tag);
    }
});