using System.Runtime.InteropServices;
using UnityEngine;

namespace Embedded.Platform
{
    public static class MobilePlatform
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int UaIsMobile();
#endif

        /// <summary>
        /// Devuelve true si estamos en un dispositivo móvil (WebGL: userAgent; otras plataformas: isMobilePlatform/Handheld).
        /// </summary>
        public static bool IsMobile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: usa userAgent para cubrir iPadOS con "desktop site"
            return UaIsMobile() != 0;
#else
            // Editor / Nativas: heurística estándar
            return Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld || Input.touchSupported;
#endif
        }
    }
}
