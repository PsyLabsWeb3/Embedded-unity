// // Assets/Scripts/BetSettingsBehaviour.cs
// using UnityEngine;

// [DefaultExecutionOrder(-500)] // que exista temprano
// public class BetSettingsBehaviour : MonoBehaviour
// {
//     private static BetSettingsBehaviour _instance;

//     // Garantiza que exista un BetSettings en cada arranque, aunque olvides ponerlo en la escena
//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     private static void AutoCreate()
//     {
//         if (_instance != null) return;
//         var go = new GameObject("BetSettings");
//         _instance = go.AddComponent<BetSettingsBehaviour>();
//         DontDestroyOnLoad(go);
//     }

//     private void Awake()
//     {
//         if (_instance != null && _instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         _instance = this;
//         DontDestroyOnLoad(gameObject);
//         Debug.Log("✅ BetSettingsBehaviour listo (escucha SendMessage).");
//     }

//     // ----- Métodos llamados desde JS/React via SendMessage -----

//     // Unity SendMessage con string
//     public void SetMode(string mode)
//     {
//         BetSettings.SetMode(mode);
//     }

//     // Si lo mandas como string (tu caso en React), usa este:
//  public void SetBetAmount(string amountStr)
// {
//     BetSettings.SetBetAmount(amountStr);
// }


//     // Útil para debug desde JS
//     public string GetSummary()
//     {
//         return $"mode={BetSettings.Mode}, effective={BetSettings.GetEffectiveBetAmount()}";
//     }
// }

// //Test