namespace PlatformShooter2D
{
  using Photon.Deterministic;
  using Quantum;
  using TMPro;
  using UnityEngine;
  using UnityEngine.UI;

  /// <summary>
  /// Manages the Skill HUD, displaying cooldown status and related information.
  /// </summary>
  public class SkillHud : QuantumSceneViewComponent<CustomViewContext>
  {
    /// <summary>
    /// The GameObject representing the cooldown UI element.
    /// </summary>
    public GameObject CooldownObject;

    /// <summary>
    /// The Image component used to visualize the cooldown progress.
    /// </summary>
    public Image CooldownFill;

    /// <summary>
    /// The TMP_Text component used to display the remaining cooldown time.
    /// </summary>
    public TMP_Text CooldownText;

    /// <summary>
    /// Updates the skill HUD view, reflecting the current state of skill cooldowns.
    /// </summary>
    public override void OnUpdateView()
    {
      if (ViewContext.LocalCharacterView == null) return;

      // Get the predicted frame and skill inventory of the local character.
      var frame = ViewContext.LocalCharacterView.PredictedFrame;
      var skillInventory = frame.Get<SkillInventory>(ViewContext.LocalCharacterView.EntityRef);
      var data = frame.FindAsset(skillInventory.SkillInventoryData);

      // Update the cooldown UI visibility and progress.
      CooldownObject.SetActive(skillInventory.CastRateTimer.IsRunning(frame));
      CooldownFill.fillAmount = (skillInventory.CastRateTimer.RemainingSeconds(frame) / data.CastRate).AsFloat;

      // Display the remaining cooldown time as an integer.
      CooldownText.text = $"{FPMath.CeilToInt(skillInventory.CastRateTimer.RemainingSeconds(frame)):0}";
    }
  }
}