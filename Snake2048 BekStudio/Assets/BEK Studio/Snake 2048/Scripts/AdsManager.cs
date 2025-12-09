using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

namespace BEKStudio{
    public class AdsManager : MonoBehaviour{
        public static AdsManager Instance;
        RewardedAd rewardedAd;
        InterstitialAd interstitialAd;


        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
        }

        void Start() {
#if UNITY_ANDROID || UNITY_IPHONE
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            RequestInterstitialAd();
            RequestRewardedAd();
#endif
        }

        void RequestInterstitialAd() {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            if (interstitialAd != null){
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            string adUnitId = "";
#if UNITY_ANDROID
            adUnitId = GameController.Instance.bekso.admobSettings.interstitialAndroidID;
#elif UNITY_IPHONE
            adUnitId = GameController.Instance.bekso.admobSettings.interstitialIOSID;
#endif
            InterstitialAd.Load(adUnitId, new AdRequest(),
                (InterstitialAd ad, LoadAdError error) =>{
                    if (error != null || ad == null){
                        return;
                    }
                    interstitialAd = ad;

                    ad.OnAdFullScreenContentClosed += () =>{
                        RequestInterstitialAd();
                    };

                    ad.OnAdFullScreenContentFailed += (AdError error) =>{
                        RequestInterstitialAd();
                    };
            });
        }

        public void ShowInterstitialAd() {
            if (interstitialAd != null && interstitialAd.CanShowAd()){
                interstitialAd.Show();
            }
        }

        void RequestRewardedAd() {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            if (rewardedAd != null) {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            string adUnitId = "";
#if UNITY_ANDROID
            adUnitId = GameController.Instance.bekso.admobSettings.rewardedAndroidID;
#elif UNITY_IPHONE
            adUnitId = GameController.Instance.bekso.admobSettings.rewardedIOSId;
#endif

            RewardedAd.Load(adUnitId, new AdRequest(),
                (RewardedAd ad, LoadAdError error) => {
                    if (error != null || ad == null) {
                        return;
                    }

                    rewardedAd = ad;
                });
        }

        public void ShowRewardedAd() {
            if (rewardedAd != null && rewardedAd.CanShowAd()) {
                rewardedAd.Show((Reward reward) => {
                    GameController.Instance.playerController.currentNumber = 32;
                    GameController.Instance.playerController.LevelUpForStart();
                    GameController.Instance.ShowGameScreen();
                    RequestRewardedAd();
                });
            }
        }
    }
}