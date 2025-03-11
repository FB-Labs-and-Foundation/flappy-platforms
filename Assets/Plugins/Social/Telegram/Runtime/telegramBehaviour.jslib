mergeInto(LibraryManager.library,{
ShareOnTelegram: function (messagePtr, urlPtr) {
        var message = UTF8ToString(messagePtr);
        var url = UTF8ToString(urlPtr);

        function unityLog(message) {
            console.log(message);
            if (typeof myGameInstance !== "undefined") {
                //myGameInstance.SendMessage("TelegramSocialManager", "ReceiveLog", message);
            }
        }

        unityLog("ShareOnTelegram called with message: " + message + " and URL: " + url);

        // Construct the sharing URLs
        var tgAppUrl = "tg://msg_url?url=" + encodeURIComponent(url) + "&text=" + encodeURIComponent(message);
        var tgWebUrl = "https://t.me/share/url?url=" + encodeURIComponent(url) + "&text=" + encodeURIComponent(message);

        if (window.Telegram && window.Telegram.WebApp) {
            unityLog("Detected Telegram WebApp environment");
            try {
                window.Telegram.WebApp.openTelegramLink(tgWebUrl);
                unityLog("Opened sharing link via Telegram WebApp");
            } catch (error) {
                unityLog("Error using Telegram WebApp sharing: " + error);
            }
            myGameInstance.SendMessage("TelegramSocialManager", "ReceiveLog", "Share Complete");
            //window.Telegram.WebApp.onEvent('shareMessageSent',function(){
            //                myGameInstance.SendMessage("TelegramSocialManager", "ReceiveLog", "Share Complete");
            //               });
            //window.Telegram.WebApp.onEvent('shareMessageFailed',function(){
            //                 myGameInstance.SendMessage("TelegramSocialManager", "ReceiveLog", "Share Failed");
            //                });

        } else if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
            // Detect mobile environment and attempt to open the Telegram app
            unityLog("Detected mobile environment, attempting to open Telegram app");
            var now = new Date().valueOf();
            setTimeout(function () {
                if (new Date().valueOf() - now > 100) return; // Indicates app was opened
                unityLog("Falling back to web version");
                window.location = tgWebUrl; // Fallback to the web version if the app doesn't open
            }, 25);
            window.location = tgAppUrl;
        } else {
            // Desktop environment: fallback to opening the web link in a new tab
            unityLog("Detected desktop environment, opening sharing link in new tab");
            window.open(tgWebUrl, '_blank');
        }
    },

    OpenTelegramChannel: function(username) {
        var channelUsername = UTF8ToString(username);
        var telegramUrl = "https://t.me/" + channelUsername;

        function unityLog(message) {
            //console.log(message);
            if (typeof myGameInstance !== "undefined") {
            //    myGameInstance.SendMessage("TelegramSocialManager", "ReceiveLog", message);
            }
        }

        unityLog("OpenTelegramChannel called with username: " + channelUsername);

        if (window.Telegram && window.Telegram.WebApp) {
            unityLog("Detected Telegram WebApp environment");
            try {
                window.Telegram.WebApp.openTelegramLink(telegramUrl);
                unityLog("Opened channel link via Telegram WebApp");
            } catch (error) {
                unityLog("Error using Telegram WebApp to open channel: " + error);
            }
        } else if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
            unityLog("Detected mobile environment, attempting to open Telegram app");
            var now = new Date().valueOf();
            setTimeout(function () {
                if (new Date().valueOf() - now > 100) return;
                unityLog("Falling back to web version");
                window.location = telegramUrl;
            }, 25);
            window.location = "tg://resolve?domain=" + channelUsername;
        } else {
            unityLog("Opening channel in new tab");
            window.open(telegramUrl, '_blank');
        }
    },

OpenTelegramInvoice: function (invoicePayloadPtr) {
        var invoicePayload = UTF8ToString(invoicePayloadPtr);

        console.log("OpenTelegramInvoice called with invoice payload: " + invoicePayload);

        if (window.Telegram && window.Telegram.WebApp) {
            try {
                // Use Telegram WebApp to open the invoice
                Telegram.WebApp.openInvoice(invoicePayload);

                WebApp.onEvent('invoiceClosed', function(object) {
                  if (object.status == 'paid') {
                    WebApp.close();
                    myGameInstance.SendMessage("TelegramSocialManager", "OnPurchaseProduct");

                  } else if (object.status == 'failed') {
                    myGameInstance.SendMessage("TelegramSocialManager", "OnPurchaseFailed");
                    WebApp.showAlert("Failed");
                  }
                });

                console.log("Opened invoice via Telegram WebApp");
            } catch (error) {
                console.log("Error using Telegram WebApp to open invoice: " + error);
            }
        } else {
            console.log("Telegram WebApp not detected.");
        }
    }
});