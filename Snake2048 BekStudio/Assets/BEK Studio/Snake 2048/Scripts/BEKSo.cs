using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEKStudio {
    [CreateAssetMenu(fileName = "EditorSO", menuName = "BEK Studio/BEK Scriptable Object", order = 1)]
    public class BEKSo : ScriptableObject {
        public GameSettings gameSettings;
        public AdmobSettings admobSettings;

        [System.Serializable]
        public class GameSettings {
            public Color32[] numberColors;
            public string[] botNames;
            public AudioClip buttonClip;
            public AudioClip foodClip;
            public int maxAiCount;
            public int maxFoodCount;
        }

        [System.Serializable]
        public class AdmobSettings {
            public string interstitialAndroidID = "ca-app-pub-3940256099942544/1033173712";
            public string rewardedAndroidID = "ca-app-pub-3940256099942544/4411468910";
            public string interstitialIOSID = "ca-app-pub-3940256099942544/5224354917";
            public string rewardedIOSId = "ca-app-pub-3940256099942544/1712485313";
        }
        
    }
}