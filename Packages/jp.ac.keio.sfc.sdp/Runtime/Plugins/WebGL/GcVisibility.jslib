mergeInto(LibraryManager.library, {
    $GcVisibility: {
        listeners: {},
        handler: null
    },
    GcVisibilityRegister__deps: ['$GcVisibility'],
    GcVisibilityRegister: function(owner, callback) {
        GcVisibility.listeners[owner] = callback;
        if (!GcVisibility.handler) {
            GcVisibility.handler = function() {
                var owners = Object.keys(GcVisibility.listeners);
                var hidden = document.hidden ? 1 : 0;
                for (var i = 0; i < owners.length; i++) {
                    var callback = GcVisibility.listeners[owners[i]];
                    if (callback) {
                        {{{ makeDynCall('vii', 'callback') }}}(Number(owners[i]), hidden);
                    }
                }
            };
            document.addEventListener('visibilitychange', GcVisibility.handler);
        }
        return document.hidden ? 1 : 0;
    },
    GcVisibilityUnregister__deps: ['$GcVisibility'],
    GcVisibilityUnregister: function(owner) {
        delete GcVisibility.listeners[owner];
        if (GcVisibility.handler && Object.keys(GcVisibility.listeners).length === 0) {
            document.removeEventListener('visibilitychange', GcVisibility.handler);
            GcVisibility.handler = null;
        }
    }
});
