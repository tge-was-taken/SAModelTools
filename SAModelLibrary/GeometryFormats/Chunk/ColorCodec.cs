using SAModelLibrary.Maths;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Color encoder & decoder utility.
    /// </summary>
    public static class ColorCodec
    {
        // R5-G6-B5
        private static readonly BitField s565R = new BitField( 0, 4 );
        private static readonly BitField s565G = new BitField( 5, 10 );
        private static readonly BitField s565B = new BitField( 11, 15 );

        // A4-R4-G4-B4
        private static readonly BitField s4444A = new BitField( 0, 3 );
        private static readonly BitField s4444R = new BitField( 4, 7 );
        private static readonly BitField s4444G = new BitField( 8, 11 );
        private static readonly BitField s4444B = new BitField( 12, 15 );

        /// <summary>
        /// Decode R5-G6-B5 to R8-G8-B8.
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static Color Decode565( ushort encoded )
        {
            var r = s565R.Unpack( encoded );
            var g = s565G.Unpack( encoded );
            var b = s565B.Unpack( encoded );
            return new Color( (byte)r, (byte)g, (byte)b );
        }

        /// <summary>
        /// Encode R8-G8-B8 to R5-G6-B5.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static ushort Encode565( Color color )
        {
            ushort encoded = 0;
            s565R.Pack( ref encoded, color.R );
            s565G.Pack( ref encoded, color.G );
            s565B.Pack( ref encoded, color.B );
            return encoded;
        }

        /// <summary>
        /// Decode A4-R4-G4-B4 to A8-R8-G8-B8.
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static Color Decode4444( ushort encoded )
        {
            var a = s4444A.Unpack( encoded );
            var r = s4444R.Unpack( encoded );
            var g = s4444G.Unpack( encoded );
            var b = s4444B.Unpack( encoded );
            return new Color( ( byte ) r, ( byte ) g, ( byte ) b, ( byte ) a );
        }

        /// <summary>
        /// Encode A8-R8-G8-B8 to A4-R4-G4-B4.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static ushort Encode4444( Color color )
        {
            ushort encoded = 0;
            s4444A.Pack( ref encoded, color.A );
            s4444R.Pack( ref encoded, color.R );
            s4444G.Pack( ref encoded, color.G );
            s4444B.Pack( ref encoded, color.B );
            return encoded;
        }
    }
}
