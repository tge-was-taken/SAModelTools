using System;
using System.Numerics;

namespace SAModelLibrary.Maths
{
    public static class QuaternionExtensions
    {
        public static Vector3 ToEulerAngles( this Quaternion q )
        {
            // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            // roll (x-axis rotation)
            var sinr = +2.0 * ( q.W * q.X + q.Y * q.Z );
            var cosr = +1.0 - 2.0 * ( q.X * q.X + q.Y * q.Y );
            var roll = Math.Atan2( sinr, cosr );

            // pitch (y-axis rotation)
            var sinp = +2.0 * ( q.W * q.Y - q.Z * q.X );
            double pitch;
            if ( Math.Abs( sinp ) >= 1 )
            {
                var sign = sinp < 0 ? -1f : 1f;
                pitch = ( Math.PI / 2 ) * sign; // use 90 degrees if out of range
            }
            else
            {
                pitch = Math.Asin( sinp );
            }

            // yaw (z-axis rotation)
            var siny = +2.0 * ( q.W * q.Z + q.X * q.Y );
            var cosy = +1.0 - 2.0 * ( q.Y * q.Y + q.Z * q.Z );
            var yaw = Math.Atan2( siny, cosy );

            return new Vector3( ( float ) ( roll * 57.2958d ), ( float ) ( pitch * 57.2958d ), ( float ) ( yaw * 57.2958d ) );
        }
    }
}