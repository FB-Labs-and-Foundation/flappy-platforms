mergeInto(LibraryManager.library, {
    SetUserProperties: function(properties) {
    },

    LogEvent: function(eventName) {
        var event_name = Pointer_stringify(eventName);

        if(window.sendFirebaseEvent)
		{
			window.sendFirebaseEvent(event_name);
		}
    },

    LogEventParameter: function(eventName, eventParameter) {
        var event_name = Pointer_stringify(eventName);
        var event_param = JSON.parse(Pointer_stringify(eventParameter));

		if(window.sendFirebaseEventWithParams)
		{
			window.sendFirebaseEventWithParams(event_name, event_param);
		}
    }
});