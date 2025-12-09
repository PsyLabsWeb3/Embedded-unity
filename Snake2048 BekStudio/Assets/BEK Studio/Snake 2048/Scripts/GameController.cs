using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BEKStudio;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BEKStudio {
    public class GameController : MonoBehaviour {
        public static GameController Instance;
        public enum GameStatus { NONE, MENU, PLAYING, DEAD};
        public GameStatus gameStatus;
        public BEKSo bekso;
        public PlayerController playerController;
        public string username;
        public GameObject dontDestroyPrefab;
        [Header("Others")]
        public AudioSource foodAudioSource;        
        public AudioSource buttonAudioSource;   
        public JoystickControl joystickControl;
        [Header("Menu")]
        public GameObject menuScreen;
        public RectTransform menuLogoRect;
        public RectTransform menuUsernameRect;
        public RectTransform menuX5Rect;
        public RectTransform menuPlayRect;
        [Header("Game")]
        public GameObject gameScreen;
        public Transform gameScoreboardParent;
        public TextMeshProUGUI gameKillMessageText;
        [Header("Game")]
        public GameObject gameOverScreen;
        public RectTransform gameOverBackground;
        public RectTransform gameOverPanel;
        public TextMeshProUGUI gameOverBestKillText;
        public TextMeshProUGUI gameOverBestSizeText;
        public TextMeshProUGUI gameOverLastKillText;
        public TextMeshProUGUI gameOverLastSizeText;
        public Image gameBoostImg;
        public TextMeshProUGUI gameBoostInfoText;
        public GameObject gameMusicOnObj;
        public GameObject gameMusicOffObj;
        
        
        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
        }

        void OnEnable() {
            gameStatus = GameStatus.NONE;

            foodAudioSource.clip = bekso.gameSettings.foodClip;
            buttonAudioSource.clip = bekso.gameSettings.buttonClip;
            
            if (PlayerPrefs.HasKey("username")){
                username = PlayerPrefs.GetString("username");
            } else {
                username = "Guest" + Random.Range(0, 9999);
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save();
            }

            GameObject dontDestroyObj = GameObject.Find("DontDestroy");
            if (dontDestroyObj == null){
                dontDestroyObj = Instantiate(dontDestroyPrefab);
                dontDestroyObj.name = "DontDestroy";
            } else {
                AdsManager.Instance.ShowInterstitialAd();
            }
            DontDestroyOnLoad(dontDestroyObj);
            
            menuLogoRect.anchoredPosition = new Vector2(0, 369f);
            menuUsernameRect.anchoredPosition = new Vector2(0, -333f);
            menuX5Rect.anchoredPosition = new Vector2(menuX5Rect.anchoredPosition.x, -177f);
            menuPlayRect.anchoredPosition = new Vector2(menuPlayRect.anchoredPosition.x, -177f);
            menuUsernameRect.GetComponent<InputField>().text = username;

            ShowMenuScreen();
            CheckMusic();
            CheckPlatform();
        }

        void CheckPlatform() {
#if UNITY_ANDROID || UNITY_IPHONE
            joystickControl.gameObject.SetActive(true);
            gameBoostInfoText.gameObject.SetActive(false);
#else
            joystickControl.gameObject.SetActive(false);
            gameBoostInfoText.gameObject.SetActive(true);
#endif
        }

        public void PlayFoodSound() {
            if (foodAudioSource.isPlaying){
                foodAudioSource.Stop();
            }
            foodAudioSource.Play();
        }
        
        void PlayButtonSound() {
            buttonAudioSource.Play();
        }

        void CheckMusic() {
            int music = PlayerPrefs.GetInt("music");
            gameMusicOnObj.SetActive(music == 0);
            gameMusicOffObj.SetActive(music == 1);

            foodAudioSource.mute = music == 1;
            buttonAudioSource.mute = music == 1;
        }

        void ShowMenuScreen() {
            gameStatus = GameStatus.MENU;
            LeanTween.move(menuLogoRect, new Vector2(0, -70), 0.2f);
            LeanTween.move(menuUsernameRect, new Vector2(0, 233), 0.2f).setDelay(0.2f);
            LeanTween.move(menuX5Rect, new Vector2(menuX5Rect.anchoredPosition.x, 45), 0.2f).setDelay(0.4f);
            LeanTween.move(menuPlayRect, new Vector2(menuPlayRect.anchoredPosition.x, 45), 0.2f).setDelay(0.4f);
            menuScreen.SetActive(true);
        }

        public void UpdateScoreboard() {
            PlayerList.Player myPlayer = PlayerList.players.FirstOrDefault(p => p.username == username);
            if (myPlayer != null){
                int playerIndex = PlayerList.players.IndexOf(myPlayer);

                if (playerIndex > 2) {
                    PlayerList.Player tempPlayer = PlayerList.players[2];
                    PlayerList.players[2] = myPlayer;
                    PlayerList.players[playerIndex] = tempPlayer;
                }
            }
            
            for (int i = 0; i < gameScoreboardParent.childCount; i++) {
                if(i > PlayerList.players.Count - 1) return;

                Transform scoreItem = gameScoreboardParent.GetChild(i);
                scoreItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerList.players[i].order + ".";
                scoreItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerList.players[i].username;
                scoreItem.GetChild(2).GetComponent<TextMeshProUGUI>().text = FormatPowerOfTwo(PlayerList.players[i].score);
            }
        }

        public void ShowDeathMessage(string killer, string enemy) {
            gameKillMessageText.text = enemy + "was killed by " + killer;
            if (LeanTween.isTweening(gameKillMessageText.gameObject)){
                LeanTween.cancel(gameKillMessageText.gameObject);
            }
            LeanTween.value(gameKillMessageText.gameObject, 0, 1, 0.8f).setLoopPingPong(1).setOnUpdate((float val) => {
                gameKillMessageText.color = new Color(1, 1, 1, val);
            });
            
        }

        public Color32 GetColor(int number) {
            if (number > bekso.gameSettings.numberColors.Length - 1) {
                number = bekso.gameSettings.numberColors.Length - 1;
            }

            return bekso.gameSettings.numberColors[number];
        }
        
        public string FormatPowerOfTwo(int number) {
            if (number <= 512)
                return number.ToString();

            string[] suffixes = { "K", "M", "B", "T" };
            int index = -1;
            float num = number;

            while (num >= 1024 && index < suffixes.Length - 1)
            {
                num /= 1024f;
                index++;
            }
            return $"{num:0.#}{suffixes[index]}";
        }

        public void MenuBiggerBtn() {
            PlayButtonSound();
            username = menuUsernameRect.GetComponent<InputField>().text;
            PlayerPrefs.SetString("username", username);
            PlayerPrefs.Save();
            
            #if UNITY_ANDROID || UNITY_IPHONE
            AdsManager.Instance.ShowRewardedAd();
            #else
            playerController.currentNumber = 32;
            playerController.LevelUpForStart();
            ShowGameScreen();
            #endif
        }

        public void MenuPlayBtn() {
            PlayButtonSound();
            username = menuUsernameRect.GetComponent<InputField>().text;
            PlayerPrefs.SetString("username", username);
            PlayerPrefs.Save();

            ShowGameScreen();
        }

        public void ShowGameScreen() {
            menuLogoRect.anchoredPosition = new Vector2(0, -70f);
            menuUsernameRect.anchoredPosition = new Vector2(0, 233f);
            menuX5Rect.anchoredPosition = new Vector2(menuX5Rect.anchoredPosition.x, 45f);
            menuPlayRect.anchoredPosition = new Vector2(menuPlayRect.anchoredPosition.x, 45f);
            
            LeanTween.move(menuLogoRect, new Vector2(0, 369f), 0.2f);
            LeanTween.move(menuX5Rect, new Vector2(menuX5Rect.anchoredPosition.x, -177f), 0.2f).setDelay(0.2f);
            LeanTween.move(menuPlayRect, new Vector2(menuPlayRect.anchoredPosition.x, -177f), 0.2f).setDelay(0.2f);
            LeanTween.move(menuUsernameRect, new Vector2(0, -333f), 0.2f).setDelay(0.4f).setOnComplete(() => {
                FoodSpawner.Instance.Spawn();
                AiSpawner.Instance.Spawn();
                gameStatus = GameStatus.PLAYING;
                menuScreen.SetActive(false);
                gameScreen.SetActive(true);
            });
        }

        public void GameBoostDownBtn() {
            playerController.boostActive = true;
        }
        
        public void GameBoostUpBtn() {
            playerController.boostActive = false;
            playerController.boostRefillWait = true;
            playerController.speed = 5;
        }

        public void GameMusicBtn() {
            PlayButtonSound();
            
            if (PlayerPrefs.GetInt("music") == 0) {
                PlayerPrefs.SetInt("music", 1);
                PlayerPrefs.Save();
            } else {
                PlayerPrefs.SetInt("music", 0);
                PlayerPrefs.Save();
            }
            
            CheckMusic();
        }

        public void GameOver() {
            gameStatus = GameStatus.DEAD;
            
            Color bgColor = gameOverBackground.GetComponent<Image>().color;
            bgColor.a = 0;
            gameOverBackground.GetComponent<Image>().color = bgColor;
            gameOverPanel.transform.localScale = Vector2.zero;
            
            gameOverScreen.SetActive(true);

            if (playerController.killCount > PlayerPrefs.GetInt("killCount")){
                PlayerPrefs.SetInt("killCount", playerController.killCount);
                PlayerPrefs.Save();
            }
            
            if (playerController.currentNumber > PlayerPrefs.GetInt("currentNumber")){
                PlayerPrefs.SetInt("currentNumber", playerController.currentNumber);
                PlayerPrefs.Save();
            }

            gameOverBestKillText.text = PlayerPrefs.GetInt("killCount", 0).ToString();
            gameOverBestSizeText.text = FormatPowerOfTwo(PlayerPrefs.GetInt("currentNumber", 0));
            gameOverLastKillText.text = playerController.killCount.ToString();
            gameOverLastSizeText.text = FormatPowerOfTwo(playerController.currentNumber);
            
            LeanTween.alpha(gameOverBackground, 0.5f, 0.2f).setOnComplete(() => {
                LeanTween.scale(gameOverPanel, Vector2.one, 0.2f).setEaseOutBack();
            });
        }

        public void GameOverContinueBtn() {
            PlayButtonSound();
            SceneManager.LoadScene("Game");
        }
    }
}
