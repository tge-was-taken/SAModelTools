using System;

namespace SAModelLibrary
{
    /// <summary>
    /// Land model surface flags.
    /// </summary>
    [Flags]
    public enum SurfaceFlags : uint
    {
        Collidable                 = 1 << 0,
        Water                 = 1 << 1,
        NoFriction            = 1 << 2,
        NoAcceleration        = 1 << 3,
        Bit4                  = 1 << 4,
        Bit5                  = 1 << 5,
        CannotLand            = 1 << 6,
        IncreasedAcceleration = 1 << 7,
        Diggable              = 1 << 8,
        Bit9                  = 1 << 9,
        Bit10                 = 1 << 10,
        Bit11                 = 1 << 11,
        Unclimbable           = 1 << 12,
        Bit13                 = 1 << 13,
        Bit14                 = 1 << 14,
        Bit15                 = 1 << 15,
        Hurt                  = 1 << 16,
        Bit17                 = 1 << 17,
        Bit18                 = 1 << 18,
        Bit19                 = 1 << 19,
        Footprints            = 1 << 20,
        Bit21                 = 1 << 21,
        Bit22                 = 1 << 22,
        Bit23                 = 1 << 23,
        Bit24                 = 1 << 24,
        Bit25                 = 1 << 25,
        Bit26                 = 1 << 26,
        Bit27                 = 1 << 27,
        Bit28                 = 1 << 28,
        Bit29                 = 1 << 29,
        Bit30                 = 1 << 30,      
        Visible               = 1u << 31    // 80000000
    }

    public static class SurfaceFlagsExtensions
    {
        public static bool HasFlagFast( this SurfaceFlags value, SurfaceFlags flag )
        {
            return ( value & flag ) != 0;
        }
    }
}