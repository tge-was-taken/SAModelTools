using System.Numerics;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats
{
    /// <summary>
    /// UV vector encoder & decoder utility.
    /// </summary>
    public static class UVCodec
    {
        private const float FIXED_POINT_255 = 255f;
        private const float FIXED_POINT_1023 = 1023f;

        /// <summary>
        /// Decode the given encoded UV vector, assuming a range of 0-255.
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static Vector2 Decode255( Vector2<short> encoded )
        {
            Vector2 decoded;

            decoded.X = encoded.X / FIXED_POINT_255;
            decoded.Y = encoded.Y / FIXED_POINT_255;

            return decoded;
        }

        /// <summary>
        /// Encode the given UV vector into an encoded vector with a range of 0-255.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Vector2<short> Encode255( Vector2 value )
        {
            Vector2<short> encoded;

            encoded.X = ( short ) ( value.X * FIXED_POINT_255 );
            encoded.Y = ( short ) ( value.Y * FIXED_POINT_255 );

            return encoded;
        }

        /// <summary>
        /// Decode the given encoded UV vector, assuming a range of 0-1023.
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static Vector2 Decode1023( Vector2<short> encoded )
        {
            Vector2 decoded;

            decoded.X = encoded.X / FIXED_POINT_1023;
            decoded.Y = encoded.Y / FIXED_POINT_1023;

            return decoded;
        }

        /// <summary>
        /// Encode the given UV vector into an encoded vector with a range of 0-1023.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Vector2<short> Encode1023( Vector2 value )
        {
            Vector2<short> encoded;

            encoded.X = ( short )( value.X * FIXED_POINT_1023 );
            encoded.Y = ( short )( value.Y * FIXED_POINT_1023 );

            return encoded;
        }
    }   
}
