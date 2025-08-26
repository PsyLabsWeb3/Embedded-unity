namespace QuantumMobileInputTools
{
  using UnityEngine;
  using System.Collections.Generic;
  using System.Runtime.InteropServices;

  public enum ControlMap
  {
    Fire,
    ChangeWeapon,
    Aim,
    Jump,
    CastSkill,
    Move,
    MousePosition
  }

  [StructLayout(LayoutKind.Explicit)]
  public struct ControlValue
  {
    [FieldOffset(0)] public bool BoolValue;

    [FieldOffset(0)] public float FloatValue;

    [FieldOffset(0)] public Vector2 Vector2Value;
  }

  public class QuantumLocalInputValuesControl : MonoBehaviour
  {
    public static QuantumLocalInputValuesControl Instance;

    private Dictionary<ControlMap, ControlValue> ControlMapValues = new Dictionary<ControlMap, ControlValue>();

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
    }

    public ControlValue GetControlValue(ControlMap controlMap)
    {
      if (ControlMapValues.TryGetValue(controlMap, out var value))
      {
        return value;
      }

      return default;
    }

    public void SendValueToControl(ControlMap controlMap, float value)
    {
      ControlValue controlValue = new ControlValue
      {
        FloatValue = value
      };
      if (ControlMapValues.TryAdd(controlMap, controlValue) == false)
      {
        ControlMapValues[controlMap] = controlValue;
      }

      
    }

    public void SendValueToControl(ControlMap controlMap, bool value)
    {
      ControlValue controlValue = new ControlValue
      {
        BoolValue = value
      };
      if (ControlMapValues.TryAdd(controlMap, controlValue) == false)
      {
        ControlMapValues[controlMap] = controlValue;
      }
    }

    public void SendValueToControl(ControlMap controlMap, Vector2 value)
    {
      ControlValue controlValue = new ControlValue
      {
        Vector2Value = value
      };
      if (ControlMapValues.TryAdd(controlMap, controlValue) == false)
      {
        ControlMapValues[controlMap] = controlValue;
      }
    }
  }
}