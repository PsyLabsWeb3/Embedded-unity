mergeInto(LibraryManager.library, {
  UaIsMobile: function () {
    try {
      var ua = navigator.userAgent || "";
      // Detecta Android, iPhone, iPad (incluye iPadOS con “desktop site”) e iPod
      var isMob = /Android|iPhone|iPad|iPod/i.test(ua);
      return isMob ? 1 : 0;
    } catch (e) {
      return 0;
    }
  }
});
