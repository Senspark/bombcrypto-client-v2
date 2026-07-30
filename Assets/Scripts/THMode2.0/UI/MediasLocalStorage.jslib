mergeInto(LibraryManager.library, {
  // Real browser localStorage bridge for the Farm Averages panel, so values are visible
  // in DevTools -> Application -> Local Storage (PlayerPrefs would use IndexedDB instead).
  MediasLS_Set: function (keyPtr, valPtr) {
    try { window.localStorage.setItem(UTF8ToString(keyPtr), UTF8ToString(valPtr)); } catch (e) {}
  },
  MediasLS_Get: function (keyPtr) {
    var value = "";
    try { value = window.localStorage.getItem(UTF8ToString(keyPtr)) || ""; } catch (e) { value = ""; }
    var size = lengthBytesUTF8(value) + 1;
    var buffer = _malloc(size);
    stringToUTF8(value, buffer, size);
    return buffer;
  },
  MediasLS_Remove: function (keyPtr) {
    try { window.localStorage.removeItem(UTF8ToString(keyPtr)); } catch (e) {}
  }
});
