mergeInto(LibraryManager.library, {
    PlayDeckBridge_PostMessage_GameEnd: function() {
        const parent = window.parent.window;
        parent.postMessage(
          { playdeck: { method: "gameEnd" } },
          "*"
        );
      },

    PlayDeckBridge_PostMessage_SendGameProgress: function(data) {
      const parent = window.parent.window;
      const jsonData = JSON.parse(UTF8ToString(data));
      console.log(jsonData);
      parent.postMessage(
        { playdeck: { method: "sendGameProgress", value: jsonData } },
        "*"
      );
    },

    PlayDeckBridge_PostMessage_SendAnalyticsNewSession: function() {
      const parent = window.parent.window;
      parent.postMessage(
        { playdeck: { method: "sendAnalyticsNewSession" } },
        "*"
      );
    },

    PlayDeckBridge_PostMessage_SendAnalytics: function(data) {
      const parent = window.parent.window;
      const jsonData = JSON.parse(UTF8ToString(data));
      console.log(jsonData);
      parent.postMessage(
        { playdeck: { method: "sendAnalytics", value: jsonData } },
        "*"
      );
    }
});