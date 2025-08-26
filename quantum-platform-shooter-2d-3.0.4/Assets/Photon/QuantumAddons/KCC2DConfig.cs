using Quantum.Physics2D;
using Quantum.Prototypes;
using UnityEngine;

namespace Quantum
{
  using Photon.Deterministic;

  public unsafe class KCC2DConfig : AssetObject
  {
    public class KCC2DContext
    {
      public Frame Frame;
      public EntityRef Entity;
      public Transform2D* Transform;
      public KCC2D* KCC;
      public KCC2DSettings Settings;
    }
    
    [Space(5)][Header("KCC Default Settings")]
    // defaults
    public Prototypes.KCC2DSettingsPrototype BaseSettings = new KCC2DSettingsPrototype()
    {
      CapsuleRadius = FP._0_25,
      CapsuleHeight = 1,
      Mask = 1 << 0, 
      SolverIterations = 4,
      IterationCorrectionRate = FP._0_50,
      AllowedPenetration = FP.EN2,
      CCD = true,
      Acceleration = 30,
      FlipDirectionMultiplier = 1,
      Deceleration = 10,
      MaxBaseSpeed = 6,
      DirectionType = DashDirection.Input,
      DashSuspendsGravity = true,
      DashDuration = FP._0_25,
      MaxDashSpeed = 10,
      BaseGravity = -10,
      DownGravityMultiplier = 2,
      MaxSlopeAngle = 50,
      SlopeMaxSpeed = 10,
      FreeFallMaxSpeed = 25,
      JumpImpulse = 10,
      AirControlFactor = 1,
      FastFlipOnAir = true,
      DownGravityOnRelease = true,
      CoyoteTime = FP.EN1,
      InputBufferTime = FP.EN1,
      DoubleJumpEnabled = true,
      DoubleJumpWhenFreeFalling = true,
      DecelerationOnAir = 5,
      WallJumpEnabled = true,
      RequiresOppositeInput = true,
      WalledStateExtention = FP._0_25,
      MinWallAngle = 75,
      MaxWallAngle = 100,
      WallJumpImpulse = new FPVector2(1, 6),
      WallMaxSpeed = 10,
    };
    // Misc
    [Space(5)][Header("Misc")] public bool Debug = false;
    public ColorRGBA ColorFinal = ColorRGBA.Blue;


    private KCCQueryResult[] _contacts = new KCCQueryResult[16];
    private int _contactsCount = 0;
    private Shape2D _capsuleShape;
    private KCC2DContext _context = new KCC2DContext();

    public void Move(Frame frame, EntityRef e, Transform2D* transform, KCC2D* KCC)
    {
      Move(frame, e, transform, KCC, null);
    }

    public void Move(Frame frame, EntityRef e, Transform2D* transform, KCC2D* KCC, KCC2DSettings? settings)
    {
      _context.Entity = e;
      _context.Transform = transform;
      _context.Frame = frame;
      if (settings.HasValue)
      {
        _context.Settings =  settings.Value;
      }
      else
      {
        BaseSettings.Materialize(_context.Frame, ref _context.Settings);
      }
      _context.KCC = KCC;
      
      if (_context.KCC->IgnoreStep)
      {
        // skip one time
        _context.KCC->IgnoreStep = false;
        return;
      }

      _capsuleShape = Shape2D.CreateCapsule(_context.Settings.CapsuleRadius, _context.Settings.CapsuleHeight / 2 - _context.Settings.CapsuleRadius);
      var position = transform->Position + _context.Settings.Offset;

      // based on current state
      IntegrateForces();

      ProcessJump();

      ProcessDash();

      int steps = 1;
      if (_context.Settings.CCD)
      {
        var fullStepLength = FPMath.Abs(_context.KCC->CombinedVelocity.Magnitude) * _context.Frame.DeltaTime;
        steps = (fullStepLength / _context.Settings.CapsuleRadius).AsInt + 1;
      }
      for (int step = 0; step < steps; step++)
      {
        _context.KCC->Closest.Overlapping = false;
        _context.KCC->Closest.ContactType = KCCContactType.NONE;
        // pre-move (velocity * delta / steps)
        position += (_context.KCC->CombinedVelocity * _context.Frame.DeltaTime) / steps;

        // apply movement step
        transform->Position = position - _context.Settings.Offset;

        _context.KCC->IgnoreStep = false;

        // find contacts (no details yet)
        FindContacts(position);
        if (_context.KCC->IgnoreStep) return;

        // for each solver iteration
        if (_contactsCount > 0)
        {
          for (int s = 0; s < _context.Settings.SolverIterations; s++)
          {
            // verify if done for this step (solver)
            if (SolverIteration(ref position, s) == false) break;

            // apply movement corrections
            transform->Position = position - _context.Settings.Offset;
          }

          for (int c = 0; c < _contactsCount; c++)
          {
            var contact = _contacts[c];
            if (contact.Ignore)
            {
              // continue to next contact (do not apply anything about this)
              continue;
            }
            _context.Frame.Signals.OnKCC2DPostSolverCollision(_context.Entity, _context.KCC, ref _context.Settings, &contact);
          }
        }
      }

      if (Debug) Draw.Capsule(position, _capsuleShape.Capsule, color: ColorFinal);

      // switch state
      ComputeState();
    }

    private void ProcessDash()
    {
      if (_context.KCC->State == KCCState.DASHING) return;

      if (_context.KCC->Closest.ContactType == KCCContactType.WALL)
      {
        var oppositeInput = _context.KCC->Closest.Contact.Normal.X * _context.KCC->LastInputDirection < 0;
        if (oppositeInput) return;
      }

      if (_context.KCC->Input.Dash.WasPressed)
      {
        _context.KCC->SetState(_context.Frame, KCCState.DASHING, _context.Settings.DashDuration);
        switch (_context.Settings.DirectionType)
        {
          case DashDirection.Velocity:
            _context.KCC->KinematicHorizontalSpeed = FPMath.Sign(_context.KCC->CombinedVelocity.X) * _context.Settings.MaxDashSpeed;
            break;
          case DashDirection.Input:
            _context.KCC->KinematicHorizontalSpeed = _context.KCC->LastInputDirection * _context.Settings.MaxDashSpeed;
            break;
        }
        if (_context.Settings.DashSuspendsGravity) _context.KCC->KinematicVerticalSpeed = 0;
      }
    }

    private void ProcessJump()
    {
      var prematureJump = _context.KCC->GroundedJumpTimer.IsRunning(_context.Frame);
      if (_context.KCC->Input.Jump.WasPressed || prematureJump)
      {
        var impulse = new FPVector2(_context.KCC->KinematicHorizontalSpeed, _context.Settings.JumpImpulse);
        switch (_context.KCC->State)
        {
          case KCCState.GROUNDED:
            _context.KCC->Jump(_context.Frame, _context.Entity, impulse);
            _context.Frame.Events.Jumped(_context.Entity, KCCState.JUMPED, KCCState.GROUNDED, impulse);
            _context.KCC->SetState(_context.Frame, KCCState.JUMPED);
            break;
          case KCCState.JUMPED:
            if (_context.Settings.DoubleJumpEnabled)
            {
              _context.KCC->Jump(_context.Frame, _context.Entity, impulse);
              _context.Frame.Events.Jumped(_context.Entity, KCCState.DOUBLE_JUMPED, KCCState.JUMPED, impulse);
              _context.KCC->SetState(_context.Frame, KCCState.DOUBLE_JUMPED, 1);
            }
            else if (_context.KCC->Input.Jump.WasPressed) _context.KCC->GroundedJumpTimer = FrameTimer.FromSeconds(_context.Frame, _context.Settings.InputBufferTime);
            break;
          case KCCState.WALLED:
            FP wallContactDirection = _context.KCC->Closest.Contact.Normal.X > 0 ? 1 : -1;
            impulse = _context.Settings.WallJumpImpulse;
            impulse.X *= wallContactDirection;
            _context.KCC->Jump(_context.Frame, _context.Entity, impulse);
            _context.Frame.Events.Jumped(_context.Entity, KCCState.JUMPED, KCCState.WALLED, impulse);
            _context.KCC->SetState(_context.Frame, KCCState.JUMPED);
            break;
          case KCCState.FREE_FALLING:
            if (_context.Settings.DoubleJumpEnabled && _context.Settings.DoubleJumpWhenFreeFalling)
            {
              _context.KCC->Jump(_context.Frame, _context.Entity, impulse);
              _context.Frame.Events.Jumped(_context.Entity, KCCState.DOUBLE_JUMPED, KCCState.FREE_FALLING, impulse);
              _context.KCC->SetState(_context.Frame, KCCState.DOUBLE_JUMPED, 1);
            }
            else if (_context.KCC->Input.Jump.WasPressed) _context.KCC->GroundedJumpTimer = FrameTimer.FromSeconds(_context.Frame, _context.Settings.InputBufferTime);
            break;
          case KCCState.DOUBLE_JUMPED:
            if (_context.KCC->Input.Jump.WasPressed) _context.KCC->GroundedJumpTimer = FrameTimer.FromSeconds(_context.Frame, _context.Settings.InputBufferTime);
            break;

        }
      }
    }

    private void ComputeState()
    {
      // grounded and walled have priority and always switch
      var forceSwitch = ShouldForcedSwitch(_context.KCC->State, _context.KCC->Closest.ContactType);
      var previousState = _context.KCC->State;
      // switch state based on contacts
      if (forceSwitch || _context.KCC->StateTimer.IsRunning(_context.Frame) == false)
      {
        switch (_context.KCC->Closest.ContactType)
        {
          case KCCContactType.GROUND:
            _context.KCC->SetState(_context.Frame, KCCState.GROUNDED, forceSwitch ? _context.Settings.CoyoteTime : null);
            _context.KCC->_dynamicVelocity *= FPMath.Clamp01(1 - _context.Settings.Deceleration * _context.Frame.DeltaTime);
            _context.KCC->_dynamicVelocity.Y = 0;
            //_context.KCC->DynamicVelocity = default;
            break;
          case KCCContactType.WALL:
            if (_context.Settings.RequiresOppositeInput)
            {
              var inputDirection = _context.KCC->Input.Left.IsDown ? -1 : _context.KCC->Input.Right.IsDown ? 1 : 0;
              var oppositeInput = _context.KCC->Closest.Contact.Normal.X * inputDirection < 0;
              if (oppositeInput) _context.KCC->SetState(_context.Frame, KCCState.WALLED);
            }
            else
            {
              _context.KCC->SetState(_context.Frame, KCCState.WALLED);
            }
            break;
          case KCCContactType.SLOPE:
            _context.KCC->SetState(_context.Frame, KCCState.SLOPED);
            break;
          case KCCContactType.NONE:
            _context.KCC->SetState(_context.Frame, KCCState.FREE_FALLING);
            break;
          case KCCContactType.CEIL:
            _context.KCC->SetState(_context.Frame, KCCState.DOUBLE_JUMPED, 1);
            break;
        }
      }

      // apply stuff based on state
      switch (_context.KCC->State)
      {
        case KCCState.WALLED:
          // keep the state hanging if still in contact
          if (_context.KCC->Closest.ContactType == KCCContactType.WALL)
          {
            var inputDirection = _context.KCC->Input.Left.IsDown ? -1 : _context.KCC->Input.Right.IsDown ? 1 : 0;
            var oppositeInput = _context.KCC->Closest.Contact.Normal.X * inputDirection < 0;

            if (previousState != KCCState.WALLED)
            {
              _context.KCC->SetStateTimer(_context.Frame, oppositeInput ? _context.Settings.WalledStateExtention : 0);
            }
            if (_context.Settings.RequiresOppositeInput == false || oppositeInput)
            {
              _context.KCC->SetStateTimer(_context.Frame, _context.Settings.WalledStateExtention);
            }
          }

          if (previousState != KCCState.WALLED)
          {
            _context.Frame.Events.Landed(_context.Entity, _context.KCC->KinematicHorizontalSpeed, KCCState.WALLED);
            _context.KCC->KinematicHorizontalSpeed = 0;
          }

          break;
        case KCCState.JUMPED:
          // keep the state hanging if pressed
          if (_context.KCC->Input.Jump.IsDown) _context.KCC->SetStateTimer(_context.Frame, FP._1);
          break;
        case KCCState.DOUBLE_JUMPED:
          // keep the state hanging until bumping
          _context.KCC->SetStateTimer(_context.Frame, FP._1);
          break;
        case KCCState.GROUNDED:
          if (previousState != KCCState.GROUNDED)
          {
            _context.Frame.Events.Landed(_context.Entity, _context.KCC->KinematicVerticalSpeed, KCCState.GROUNDED);
            _context.KCC->KinematicVerticalSpeed = 0;
          }
          break;
      }

      if (_context.KCC->Closest.ContactType == KCCContactType.CEIL && _context.KCC->KinematicVerticalSpeed > 0)
        _context.KCC->KinematicVerticalSpeed = 0;
      
      _context.Frame.Signals.OnKCC2DAfterState(_context.Entity, _context.KCC, ref _context.Settings);
    }

    private static bool ShouldForcedSwitch(KCCState currentState, KCCContactType contactType)
    {
      if (contactType == KCCContactType.GROUND && currentState != KCCState.DASHING) return true;
      if (contactType == KCCContactType.WALL) return true;
      if (contactType == KCCContactType.CEIL && currentState != KCCState.DASHING && currentState != KCCState.DOUBLE_JUMPED) return true;
      return false;
    }

    public bool SolverIteration(ref FPVector2 position, int iteration)
    {
      bool AppliedCorrection = false;
      // for each contact
      for (int c = 0; c < _contactsCount; c++)
      {
        // compute penetration
        var result = _contacts[c];
        
        // already ignored on previous iteration
        if (result.Ignore)
        {
          // continue to next contact (do not apply anything about this)
          continue;
        }
        
        result.ContactType = KCCContactType.NONE;
        result.Overlapping = ComputePenetration(_context.Frame, position, ref _capsuleShape, ref result.Contact);
        result.SurfaceTangent = FPVector2.Rotate(result.Contact.Normal, -FP.Rad_90);
        result.ContactAngle = FPVector2.Angle(FPVector2.Up, result.Contact.Normal);
        // can be used to modify normals, etc
        _context.Frame.Signals.OnKCC2DSolverCollision(_context.Entity, _context.KCC, ref _context.Settings, &result, iteration);

        _contacts[c] = result;
        
        // test after callback
        if (result.Ignore)
        {
          continue;
        }

        if (_context.KCC->IgnoreStep)
        {
          _contactsCount = 0;
          return false;
        }

        // identify contact type
        if (result.ContactAngle < _context.Settings.MaxSlopeAngle)
        {
          result.ContactType = KCCContactType.GROUND;
        }
        else
        {
          if (result.ContactAngle > 90 + _context.Settings.MaxSlopeAngle)
          {
            result.ContactType = KCCContactType.CEIL;
          }
          else
          {
            if (_context.Settings.WallJumpEnabled && result.ContactAngle > _context.Settings.MinWallAngle && result.ContactAngle < _context.Settings.MaxWallAngle)
            {
              result.ContactType = KCCContactType.WALL;
            }
            else
            {
              result.ContactType = KCCContactType.SLOPE;
            }
          }
        }

        // priority for "closest" contact (ground -> wall -> slope -> ceil)
        if (_context.KCC->Closest.ContactType == KCCContactType.NONE || _context.KCC->Closest.ContactType > result.ContactType)
        {
          _context.KCC->Closest = result;
        }

        // apply correction
        if (result.Contact.OverlapPenetration > _context.Settings.AllowedPenetration)
        {
          var fullCorrection = result.Contact.Normal * result.Contact.OverlapPenetration;
          if (Debug) Draw.Ray(position, fullCorrection, ColorRGBA.Red);
          var correction = fullCorrection * _context.Settings.IterationCorrectionRate;
          position += correction;
          AppliedCorrection = true;
        }
        // applying back to collection/stored
        _contacts[c] = result;
      }
      return AppliedCorrection;
    }

    public void FindContacts(FPVector2 position)
    {
      
      var hits = _context.Frame.Physics2D.OverlapShape(position, 0, _capsuleShape, _context.Settings.Mask, QueryOptions.HitAll);
      int index = 0;
      for (int i = 0; i < hits.Count; i++)
      {
        var hit = hits[i];
        if (hit.IsTrigger)
        {
          _context.Frame.Signals.OnKCC2DTrigger(_context.Entity, _context.KCC, ref _context.Settings, hit);
          if (_context.KCC->IgnoreStep)
          {
            _contactsCount = 0;
            return;
          }
          continue;
        }

        if (hit.Entity == _context.Entity)
        {
          continue;
        }

        var contact = new KCCQueryResult();
        contact.Contact = hit;
        _context.Frame.Signals.OnKCC2DPreCollision(_context.Entity, _context.KCC, ref _context.Settings, &contact);
        // add to contacts if not ignored
        if (contact.Ignore == false)
        {
          _contacts[index++] = contact;
        }
        if (_context.KCC->IgnoreStep)
        {
          _contactsCount = 0;
          return;
        }
        if (index >= _contacts.Length) break;
      }
      _contactsCount = index;
    }

    private void IntegrateForces()
    {
      var sideMovement = SideMovement();

      Accelerate(sideMovement);

      // drag
      if (sideMovement == 0)
      {
        if (_context.KCC->State == KCCState.GROUNDED)
        {
          _context.KCC->_kinematicVelocity *= FPMath.Clamp01(1 - _context.Settings.Deceleration * _context.Frame.DeltaTime);
        }
        else
        {
          _context.KCC->KinematicHorizontalSpeed *= FPMath.Clamp01(1 - _context.Settings.DecelerationOnAir * _context.Frame.DeltaTime);
        }
      }

      ApplyGravity();

      ClampVelocity();
    }

    private void ClampVelocity()
    {
      FP maxYSpeed = _context.Settings.FreeFallMaxSpeed;
      FP maxXSpeed = _context.Settings.MaxBaseSpeed;
      switch (_context.KCC->State)
      {
        case KCCState.WALLED:
          if (_context.Settings.WallJumpEnabled && _context.KCC->StateTimer.IsRunning(_context.Frame)) maxYSpeed = _context.Settings.WallMaxSpeed;
          break;
        case KCCState.SLOPED:
          maxYSpeed = _context.Settings.SlopeMaxSpeed;
          break;
        case KCCState.DASHING:
          maxXSpeed = _context.Settings.MaxDashSpeed;
          break;
      }

      if (_context.KCC->State == KCCState.GROUNDED)
      {
        if (_context.KCC->_kinematicVelocity.Magnitude > maxXSpeed)
        {
          _context.KCC->_kinematicVelocity = _context.KCC->_kinematicVelocity.Normalized * maxXSpeed;
        }
      }
      else if (FPMath.Abs(_context.KCC->KinematicHorizontalSpeed) > maxXSpeed)
      {
        _context.KCC->KinematicHorizontalSpeed = FPMath.Sign(_context.KCC->KinematicHorizontalSpeed) * maxXSpeed;
      }

      if (_context.KCC->KinematicVerticalSpeed < 0)
      {
        _context.KCC->KinematicVerticalSpeed = FPMath.Clamp(_context.KCC->KinematicVerticalSpeed, -maxYSpeed, FP._0);
      }
    }

    private void ApplyGravity()
    {
      bool dashing = _context.KCC->State == KCCState.DASHING;

      if (_context.KCC->State != KCCState.GROUNDED && (_context.Settings.DashSuspendsGravity == false || dashing == false))
      {
        FP gravityModifier = 1;
        var buttonPressed = _context.KCC->Input.Jump.IsDown || _context.Settings.DownGravityOnRelease == false;
        if (_context.KCC->KinematicVerticalSpeed <= 0 || buttonPressed == false || _context.KCC->State == KCCState.FREE_FALLING) gravityModifier = _context.Settings.DownGravityMultiplier;
        _context.KCC->KinematicVerticalSpeed += _context.Settings.BaseGravity * gravityModifier * _context.Frame.DeltaTime;
      }
    }

    private void Accelerate(FP sideMovement)
    {
      if (_context.KCC->State != KCCState.GROUNDED)
      {
        _context.KCC->KinematicHorizontalSpeed += _context.Settings.Acceleration * _context.Frame.DeltaTime * sideMovement;
        if (_context.KCC->State == KCCState.WALLED)
        {
          if (_context.KCC->KinematicHorizontalSpeed * _context.KCC->Closest.Contact.Normal.X < 0) _context.KCC->KinematicHorizontalSpeed = 0;
        }
      }
      else
      {
        _context.KCC->ApplyKinematicAcceleration(_context.Frame, (_context.Settings.Acceleration * sideMovement) * _context.KCC->Closest.SurfaceTangent);
      }
    }

    private FP SideMovement()
    {
      FP sideMovement = 0;
      if (_context.KCC->Input.Left) sideMovement -= 1;
      if (_context.KCC->Input.Right) sideMovement += 1;
      if (sideMovement != 0) _context.KCC->LastInputDirection = sideMovement.AsInt;
      if (_context.KCC->State != KCCState.GROUNDED) sideMovement *= _context.Settings.AirControlFactor;

      var oppositeDirection = _context.KCC->CombinedVelocity.X * sideMovement < 0;
      if (oppositeDirection && (_context.KCC->State == KCCState.GROUNDED || _context.Settings.FastFlipOnAir))
      {
        sideMovement *= _context.Settings.FlipDirectionMultiplier;
      }
      return sideMovement;
    }

    private static bool ComputePenetration(Frame f, FPVector2 position, ref Shape2D shape, ref Hit hit)
    {

      var t = new Transform2D() { Position = position };
      var s = shape;
      var h = hit;
      var hits = f.Physics2D.CheckOverlap(&s, &t, &h);
      if (hits.Count > 0)
      {
        hit.Normal = hits[0].Normal;
        hit.OverlapPenetration = hits[0].OverlapPenetration;
        hit.Point = hits[0].Point;
        return true;
      }
      return false;
    }
  }
}