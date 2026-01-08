mergeInto(LibraryManager.library, {
    BlockchainManager_Initialize: function(message) {
        var returnStr = BlockchainManager_Initialize(JSON.parse(UTF8ToString(message)));
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    }
});