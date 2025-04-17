mergeInto(LibraryManager.library, {

    PlaygamaBridgeGetInterstitialState: function() {
        var interstitialState = window.getInterstitialState()
        var bufferSize = lengthBytesUTF8(interstitialState) + 1
        var buffer = _malloc(bufferSize)
        stringToUTF8(interstitialState, buffer, bufferSize)
        return buffer
    },

    PlaygamaBridgeMinimumDelayBetweenInterstitial: function() {
        var minimumDelayBetweenInterstitial = window.getMinimumDelayBetweenInterstitial()
        var bufferSize = lengthBytesUTF8(minimumDelayBetweenInterstitial) + 1
        var buffer = _malloc(bufferSize)
        stringToUTF8(minimumDelayBetweenInterstitial, buffer, bufferSize)
        return buffer
    },

    PlaygamaBridgeSetMinimumDelayBetweenInterstitial: function(options) {
        window.setMinimumDelayBetweenInterstitial(UTF8ToString(options))
    },

    PlaygamaBridgeShowInterstitial: function() {
        window.showInterstitial()
    },

    PlaygamaBridgeShowRewarded: function() {
        window.showRewarded()
    },

    PlaygamaBridgeIsBannerSupported: function() {
        var isBannerSupported = window.getIsBannerSupported()
        var bufferSize = lengthBytesUTF8(isBannerSupported) + 1
        var buffer = _malloc(bufferSize)
        stringToUTF8(isBannerSupported, buffer, bufferSize)
        return buffer
    },

    PlaygamaBridgeShowBanner: function(options) {
        window.showBanner(UTF8ToString(options))
    },

    PlaygamaBridgeHideBanner: function() {
        window.hideBanner()
    },

    PlaygamaBridgeCheckAdBlock: function() {
        window.checkAdBlock()
    },
});