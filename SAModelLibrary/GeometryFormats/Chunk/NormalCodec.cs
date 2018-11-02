using System.Diagnostics;
using System.Numerics;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Normal vector encoder & decoder utility.
    /// </summary>
    public static class NormalCodec
    {
        private const float FIXED_POINT = 1023f;

        private static readonly BitField sUnused = new BitField( 0, 1 );
        private static readonly BitField sX = new BitField( 2, 11 );
        private static readonly BitField sY = new BitField( 12, 21 );
        private static readonly BitField sZ = new BitField( 22, 31 );

        /// <summary>
        /// Unpack and decode a normal vector from the encoded input.
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static Vector3 Decode( uint encoded )
        {
#if DEBUG
            var unused = sUnused.Unpack( encoded );
            Debug.Assert( unused == 0, "Unused bits in encoded normal are used" );
#endif
            var x = sX.Unpack( encoded );
            var y = sY.Unpack( encoded );
            var z = sZ.Unpack( encoded );

            var xDec = x / FIXED_POINT;
            var yDec = y / FIXED_POINT;
            var zDec = z / FIXED_POINT;

            return new Vector3( xDec, yDec, zDec );
        }

        /// <summary>
        /// Encode and pack a normal vector from the unencoded input.
        /// </summary>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static uint Encode( Vector3 normal )
        {
            var xEnc = ( uint ) ( normal.X * FIXED_POINT );
            var yEnc = ( uint ) ( normal.Y * FIXED_POINT );
            var zEnc = ( uint ) ( normal.Z * FIXED_POINT );

            uint encoded = 0;
            sX.Pack( ref encoded, xEnc );
            sY.Pack( ref encoded, yEnc );
            sZ.Pack( ref encoded, zEnc );

            return encoded;
        }
    }
}
