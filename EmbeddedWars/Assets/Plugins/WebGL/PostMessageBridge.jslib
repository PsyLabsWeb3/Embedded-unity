mergeInto(LibraryManager.library, {
  PostMessageToParent: function (strPtr) {
    const payload = UTF8ToString(strPtr);
    try {
      // Si está incrustado en un iframe, envíalo al parent; si no, al window actual
      (window.parent || window).postMessage(payload, "*");
    } catch (e) {
      console.error("[PostMessageBridge] Error enviando mensaje:", e);
    }
  }
});