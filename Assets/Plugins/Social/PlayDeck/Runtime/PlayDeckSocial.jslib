mergeInto(LibraryManager.library, {
  PlayDeckBridge_PostMessage_CustomShare: function(data) {
    const parent = window.parent.window;
    const jsonData = JSON.parse(UTF8ToString(data));
    console.log(jsonData);
    parent.postMessage(
      { playdeck: { method: "customShare", value: jsonData } },
      "*"
    );
  },

  PlayDeckBridge_PostMessage_GetShareLink: function(data) {
    const parent = window.parent.window;
    const jsonData = JSON.parse(UTF8ToString(data));
    console.log(jsonData);
    parent.postMessage(
      { playdeck: { method: "getShareLink", value: jsonData } },
      "*"
    );
  },

  PlayDeckBridge_PostMessage_OpenTelegramLink: function(data) {
    const parent = window.parent.window;
    const link = UTF8ToString(data);
    console.log(link);
    parent.postMessage(
      { playdeck: { method: "openTelegramLink", value: link } },
      "*"
    );
  }
});