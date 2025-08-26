using Photon.Deterministic;
using Quantum.Physics2D;

namespace Quantum
{
  // partial extensions to the component struct
  unsafe partial struct KCC2D
  {

    public FPVector2 CombinedVelocity
    {
      get { return _kinematicVelocity + _dynamicVelocity; }
    }

    public KCCState State
    {
      get { return _state; }
    }

    public FrameTimer StateTimer
    {
      get { return _stateTimer; }
    }

    public void SetState(Frame f, KCCState state, FP? time = null)
    {
      _state = state;
      if (time.HasValue)
        _stateTimer = FrameTimer.FromSeconds(f, time.Value);
    }

    public void SetStateTimer(Frame f, FP time)
    {
        _stateTimer = FrameTimer.FromSeconds(f, time);
    }

    public FPVector2 DynamicVelocity
    {
      set { _dynamicVelocity = value; }
    }

    public FP KinematicHorizontalSpeed
    {
      get { return _kinematicVelocity.X; }
      set { _kinematicVelocity.X = value; }
    }

    public FP KinematicVerticalSpeed
    {
      get { return _kinematicVelocity.Y; }
      set { _kinematicVelocity.Y = value; }
    }

    public void ApplyKinematicAcceleration(Frame f, FPVector2 acceleration)
    {
      _kinematicVelocity += f.DeltaTime * acceleration;
    }

    public void ResetAll(Frame f, EntityRef entity)
    {
      _dynamicVelocity = default;
      _kinematicVelocity = default;
      SetState(f, KCCState.FREE_FALLING);
      _stateTimer = default;
      IgnoreStep = true;
      GroundedJumpTimer = default;
      Input = default;
      LastInputDirection = default;
      if (f.Unsafe.TryGetPointer<Transform2D>(entity, out var transform))
      {
        transform->Teleport(f, InitialPosition);
      }
    }


    public void Jump(Frame f, EntityRef entity, FPVector2 impulse)
    {
      _kinematicVelocity = impulse;
    }
  }
}

