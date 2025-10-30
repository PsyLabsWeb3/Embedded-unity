using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct Input
    {
        public FPVector2 Direction
        {
            get => DecodeDirection(Encoded);
            set => Encoded = EncodeDirection(value);
        }
        
        private byte EncodeDirection(FPVector2 direction)
        {
            if (direction == default)
            {
                return default;
            }

            FP angle = FPVector2.RadiansSigned(FPVector2.Up, direction) * FP.Rad2Deg;
            angle = (((angle + 360) % 360) / 2) + 1;
            return (byte)angle.AsInt;
        }

        private FPVector2 DecodeDirection(byte directionEncoded)
        {
            if (directionEncoded == default)
            {
                return default;
            }

            int angle = (directionEncoded - 1) * 2;
            return FPVector2.Rotate(FPVector2.Up, angle * FP.Deg2Rad);
        }
    }
}
