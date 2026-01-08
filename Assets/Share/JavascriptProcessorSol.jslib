mergeInto(LibraryManager.library, {
    JS_CallMethod: function(sender, tag, callback, callbackId, data) {
        const _sender = UTF8ToString(sender);
        const _tag = UTF8ToString(tag);
        const _callback = UTF8ToString(callback);
        const _data = UTF8ToString(data);       
        jsCall(_tag, function(response) {
            const _response = JSON.stringify({
                id: callbackId,
                response: response,
            });
            unityInstance.Module.SendMessage(_sender, _callback, _response);
        }, _data);
    }
});