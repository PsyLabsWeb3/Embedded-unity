namespace PlatformShooter2D
{
  using QuantumMobileInputTools;
  using UnityEngine;

  /// <summary>
  /// Sets local inputs for local character when on standalone platform
  /// </summary>
  public class LocalGameplayInputStandalone : MonoBehaviour
  {
    void Update()
    {
#if UNITY_STANDALONE || UNITY_WEBGL
      var control = QuantumLocalInputValuesControl.Instance;
      
      //Set the movement input
      var movement = Input.GetAxis("Horizontal");
      control.SendValueToControl(ControlMap.Move, new Vector2(movement, 0));

      //Set the mouse position input
      var mousePosition = Input.mousePosition;
      control.SendValueToControl(ControlMap.MousePosition, new Vector2(mousePosition.x, mousePosition.y));

      //Set the fire input
      control.SendValueToControl(ControlMap.Fire, Input.GetMouseButton(0));

      //Set the change weapon input
      var changeWeapon = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.X);
      control.SendValueToControl(ControlMap.ChangeWeapon, changeWeapon);
      
      //Set the jump input
      control.SendValueToControl(ControlMap.Jump, Input.GetKey(KeyCode.Space));
      
      //Set the cast skill input
      var  castSkill = Input.GetKey(KeyCode.F) || Input.GetMouseButton(1);
      control.SendValueToControl(ControlMap.CastSkill, castSkill);
#endif
    }
  }
}