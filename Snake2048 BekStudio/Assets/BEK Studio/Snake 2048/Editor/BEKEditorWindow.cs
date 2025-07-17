using UnityEditor;
using UnityEngine;
using BEKStudio;

namespace BEKStudio {
    public class BEKEditorWindow : EditorWindow {
        static BEKSo data;
        SerializedObject serializedObject;
        int selectedTab = 0;
        Vector2 scrollPos;

        void OnEnable() {
            if (!data) {
                data = AssetDatabase.LoadAssetAtPath<BEKSo>("Assets/BEK Studio/Snake 2048/Resources/EditorSO.asset");

                if (data) return;

                data = CreateInstance<BEKSo>();
            }
        }

        [MenuItem("BEK Studio/BEK Window")]
        static void Init() {
            var window = (BEKEditorWindow)EditorWindow.GetWindow(typeof(BEKEditorWindow), true, "BEK Studio Editor");
            window.maxSize = new Vector2(512, 600);
            window.minSize = window.maxSize;

            window.Show();
        }

        void OnGUI() {
            serializedObject = new SerializedObject(data);
            serializedObject.Update();

            GUILayout.Space(10);
            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Game Settings", "Admob Settings" }, GUILayout.MinHeight(30));
            GUILayout.Space(10);

            switch (selectedTab) {
                case 0:
                    ShowGameSettings();
                    break;
                case 1:
                    ShowAdmobSettings();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        void ShowGameSettings() {
            SerializedProperty gameSettingsProperty = serializedObject.FindProperty("gameSettings");
            GUILayout.BeginVertical();

            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.HelpBox("The color that will be in each new number.", MessageType.Info);
            EditorGUILayout.PropertyField(gameSettingsProperty.FindPropertyRelative("numberColors"), new GUIContent("Color of numbers"));
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Fake bot names.", MessageType.Info);
            EditorGUILayout.PropertyField(gameSettingsProperty.FindPropertyRelative("botNames"), new GUIContent("Bot names"));
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Sounds used in the game.", MessageType.Info);
            EditorGUILayout.PropertyField(gameSettingsProperty.FindPropertyRelative("buttonClip"), new GUIContent("Button Sound"));
            EditorGUILayout.PropertyField(gameSettingsProperty.FindPropertyRelative("foodClip"), new GUIContent("Food Sound"));
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Other settings for game.", MessageType.Info);
            EditorGUILayout.PropertyField(gameSettingsProperty.FindPropertyRelative("maxAiCount"), new GUIContent("Maximum ai count"));
            EditorGUILayout.PropertyField(gameSettingsProperty.FindPropertyRelative("maxFoodCount"), new GUIContent("Maximum food count"));
            GUILayout.Space(10);

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        void ShowAdmobSettings() {
            SerializedProperty admobSettingsProperty = serializedObject.FindProperty("admobSettings");
            GUILayout.BeginVertical();

            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.HelpBox("Write your Interstitial and Rewarded IDs for Android.", MessageType.Info);
            EditorGUILayout.PropertyField(admobSettingsProperty.FindPropertyRelative("interstitialAndroidID"), new GUIContent("Android Interstitial"));
            EditorGUILayout.PropertyField(admobSettingsProperty.FindPropertyRelative("rewardedAndroidID"), new GUIContent("Android Rewarded"));
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Write your Interstitial and Rewarded IDs for IOS.", MessageType.Info);
            EditorGUILayout.PropertyField(admobSettingsProperty.FindPropertyRelative("interstitialIOSID"), new GUIContent("IOS Interstitial"));
            EditorGUILayout.PropertyField(admobSettingsProperty.FindPropertyRelative("rewardedIOSId"), new GUIContent("IOS Rewarded"));

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

    }
}