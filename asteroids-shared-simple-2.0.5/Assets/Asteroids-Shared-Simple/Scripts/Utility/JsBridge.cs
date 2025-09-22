// Assets/Scripts/Bridge/JsBridge.cs
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class JsBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")]
  private static extern void PostMessageToParent(string json);
#endif

  [Serializable]
  private struct Msg { public string source; public string type; public string reason; public string matchId; }

  public static void NotifyGameOver(string reason = "ended", string matchId = "")
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    var msg = new Msg { source = "unity", type = "GAME_OVER", reason = reason, matchId = matchId };
    PostMessageToParent(JsonUtility.ToJson(msg));
#else
    Debug.Log($"[JsBridge] GAME_OVER (editor): {reason} / {matchId}");
#endif
  }
}