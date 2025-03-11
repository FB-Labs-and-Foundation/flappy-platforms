mergeInto(LibraryManager.library, {
    PlayDeckBridge_PostMessage_ShowAd: function() {
        const parent = window.parent.window;
        parent.postMessage(
          { playdeck: { method: "showAd" } },
          "*"
        );
    }
});