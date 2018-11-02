using System;

namespace SAModelLibrary.Maths
{
    /// <summary>
    /// Utility class for converting between different methods to describe rotation.
    /// </summary>
    public static class RotationConverter
    {
        /// <summary>
        /// Convert degrees to radians.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float DegreesToRadians( float degrees ) => ( float ) ( ( degrees * Math.PI ) / 180f );

        /// <summary>
        /// Convert degrees to angle.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static int DegreesToAngle( float degrees ) => ( int )( ( degrees * 65536f ) / 360f );

        /// <summary>
        /// Convert radians to angle.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static int RadiansToAngle( float radians ) => ( int ) ( radians * 65536f / ( 2 * Math.PI ) );

        /// <summary>
        /// Convert radians to degrees.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static float RadiansToDegrees( float radians ) => ( float ) ( ( radians * 180f ) / Math.PI );

        /// <summary>
        /// Convert angle to degrees.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float AngleToDegrees( int angle ) => ( angle * 360f ) / 65536f;

        /// <summary>
        /// Convert angle to radians.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float AngleToRadians( int angle ) => ( float ) ( ( angle * ( 2 * Math.PI ) / 65536f ) );
    }
}
