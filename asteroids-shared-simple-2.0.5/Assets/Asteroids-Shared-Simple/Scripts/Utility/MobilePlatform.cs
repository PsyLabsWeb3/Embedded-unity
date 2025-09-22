using UnityEngine;

namespace Asteroids.SharedSimple.Utility
{
    public static class MobilePlatform
    {
        public static bool IsMobile()
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#elif UNITY_WEBGL
            // 🔍 En WebGL hacemos un pequeño check adicional (ej. userAgent)
            return Application.isMobilePlatform;
#else
            return false;
#endif
        }
    }
}
