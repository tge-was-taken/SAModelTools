using System;
using System.Collections.Generic;
using System.Diagnostics;
using SAModelLibrary.IO;
using SAModelLibrary.Maths;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.Basic
{
    /// <summary>
    /// Represents a basic material structure.
    /// </summary>
    public class Material : ISerializableObject, IEquatable<Material>
    {
        private static readonly BitField sTextureIdField = new BitField( 0, 27 );
        private static readonly BitField sAttributeField = new BitField( 28, 31 );

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the diffuse color of the material.
        /// </summary>
        public Color Diffuse { get; set; }

        /// <summary>
        /// Gets or sets the specular color of the material.
        /// </summary>
        public Color Specular { get; set; }

        /// <summary>
        /// Gets or sets the exponent power of the material.
        /// </summary>
        public float Exponent { get; set; }

        /// <summary>
        /// Gets or sets the texture id associated with the material.
        /// </summary>
        public int TextureId { get; set; }

        /// <summary>
        /// Gets or sets the material attribute.
        /// </summary>
        public uint Attribute { get; set; }

        /// <summary>
        /// Gets or sets the material flags.
        /// </summary>
        public uint Flags { get; set; }

        public byte UserFlags
        {
            get => ( byte )( Flags & 0x7F );
            set => Flags = ( uint )( ( Flags & ~0x7F ) | ( value & 0x7Fu ) );
        }

        public bool PickStatus
        {
            get => ( Flags & 0x80 ) == 0x80;
            set => Flags = ( uint )( ( Flags & ~0x80 ) | ( value ? 0x80u : 0 ) );
        }

        public bool SuperSample
        {
            get => ( Flags & 0x1000 ) == 0x1000;
            set => Flags = ( uint )( ( Flags & ~0x1000 ) | ( value ? 0x1000u : 0 ) );
        }

        public FilterMode FilterMode
        {
            get => ( FilterMode )( ( Flags >> 13 ) & 3 );
            set => Flags = ( uint )( ( Flags & ~0x6000 ) | ( ( uint )value << 13 ) );
        }

        public bool ClampV
        {
            get => ( Flags & 0x8000 ) == 0x8000;
            set => Flags = ( uint )( ( Flags & ~0x8000 ) | ( value ? 0x8000u : 0 ) );
        }

        public bool ClampU
        {
            get => ( Flags & 0x10000 ) == 0x10000;
            set => Flags = ( uint )( ( Flags & ~0x10000 ) | ( value ? 0x10000u : 0 ) );
        }

        public bool FlipV
        {
            get => ( Flags & 0x20000 ) == 0x20000;
            set => Flags = ( uint )( ( Flags & ~0x20000 ) | ( value ? 0x20000u : 0 ) );
        }

        public bool FlipU
        {
            get => ( Flags & 0x40000 ) == 0x40000;
            set => Flags = ( uint )( ( Flags & ~0x40000 ) | ( value ? 0x40000u : 0 ) );
        }

        public bool IgnoreSpecular
        {
            get => ( Flags & 0x80000 ) == 0x80000;
            set => Flags = ( uint )( ( Flags & ~0x80000 ) | ( value ? 0x80000u : 0 ) );
        }

        public bool UseAlpha
        {
            get => ( Flags & 0x100000 ) == 0x100000;
            set => Flags = ( uint )( ( Flags & ~0x100000 ) | ( value ? 0x100000u : 0 ) );
        }

        public bool UseTexture
        {
            get => ( Flags & 0x200000 ) == 0x200000;
            set => Flags = ( uint )( ( Flags & ~0x200000 ) | ( value ? 0x200000u : 0 ) );
        }

        public bool EnvironmentMap
        {
            get => ( Flags & 0x400000 ) == 0x400000;
            set => Flags = ( uint )( ( Flags & ~0x400000 ) | ( value ? 0x400000u : 0 ) );
        }

        public bool DoubleSided
        {
            get => ( Flags & 0x800000 ) == 0x800000;
            set => Flags = ( uint )( ( Flags & ~0x800000 ) | ( value ? 0x800000u : 0 ) );
        }

        public bool FlatShading
        {
            get => ( Flags & 0x1000000 ) == 0x1000000;
            set => Flags = ( uint )( ( Flags & ~0x1000000 ) | ( value ? 0x1000000u : 0 ) );
        }

        public bool IgnoreLighting
        {
            get => ( Flags & 0x2000000 ) == 0x2000000;
            set => Flags = ( uint )( ( Flags & ~0x2000000 ) | ( value ? 0x2000000u : 0 ) );
        }

        public DstAlphaOp DestinationAlpha
        {
            get => ( DstAlphaOp )( ( Flags >> 26 ) & 7 );
            set => Flags = ( uint )( ( Flags & ~0x1C000000 ) | ( ( uint )value << 26 ) );
        }

        public SrcAlphaOp SourceAlpha
        {
            get => ( SrcAlphaOp )( ( Flags >> 29 ) & 7 );
            set => Flags = ( Flags & ~0xE0000000 ) | ( ( uint )value << 29 );
        }

        /// <summary>
        /// Initializes a new default instance of <see cref="Material"/>.
        /// </summary>
        public Material()
        {
        }

        /// <inheritdoc />
        public void Read( EndianBinaryReader reader, object context = null )
        {
            Diffuse = reader.ReadColor();
            Specular = reader.ReadColor();
            Exponent = reader.ReadSingle();
            UnpackTextureId( reader.ReadUInt32() );
            Flags = reader.ReadUInt32();
        }

        /// <inheritdoc />
        public void Write( EndianBinaryWriter writer, object context = null )
        {
            writer.Write( Diffuse );
            writer.Write( Specular );
            writer.Write( Exponent );
            writer.Write( PackTextureId() );
            writer.Write( Flags );
        }

        private void UnpackTextureId( uint textureIdAndAttribute )
        {
            TextureId = ( int )sTextureIdField.Unpack( textureIdAndAttribute );
            Attribute = sAttributeField.Unpack( textureIdAndAttribute );
            Debug.Assert( PackTextureId() == textureIdAndAttribute );
        }

        private uint PackTextureId()
        {
            uint textureId = 0;
            sTextureIdField.Pack( ref textureId, ( uint )TextureId );
            sAttributeField.Pack( ref textureId, ( uint )Attribute );
            return textureId;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as Material );
        }

        public bool Equals( Material other )
        {
            return other != null &&
                   Diffuse.Equals( other.Diffuse ) &&
                   Specular.Equals( other.Specular ) &&
                   Exponent == other.Exponent &&
                   TextureId == other.TextureId &&
                   Attribute == other.Attribute &&
                   Flags == other.Flags &&
                   UserFlags == other.UserFlags &&
                   PickStatus == other.PickStatus &&
                   SuperSample == other.SuperSample &&
                   FilterMode == other.FilterMode &&
                   ClampV == other.ClampV &&
                   ClampU == other.ClampU &&
                   FlipV == other.FlipV &&
                   FlipU == other.FlipU &&
                   IgnoreSpecular == other.IgnoreSpecular &&
                   UseAlpha == other.UseAlpha &&
                   UseTexture == other.UseTexture &&
                   EnvironmentMap == other.EnvironmentMap &&
                   DoubleSided == other.DoubleSided &&
                   FlatShading == other.FlatShading &&
                   IgnoreLighting == other.IgnoreLighting &&
                   DestinationAlpha == other.DestinationAlpha &&
                   SourceAlpha == other.SourceAlpha;
        }

        public override int GetHashCode()
        {
            var hashCode = 285071988;
            hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode( Diffuse );
            hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode( Specular );
            hashCode = hashCode * -1521134295 + Exponent.GetHashCode();
            hashCode = hashCode * -1521134295 + TextureId.GetHashCode();
            hashCode = hashCode * -1521134295 + Attribute.GetHashCode();
            hashCode = hashCode * -1521134295 + Flags.GetHashCode();
            hashCode = hashCode * -1521134295 + UserFlags.GetHashCode();
            hashCode = hashCode * -1521134295 + PickStatus.GetHashCode();
            hashCode = hashCode * -1521134295 + SuperSample.GetHashCode();
            hashCode = hashCode * -1521134295 + FilterMode.GetHashCode();
            hashCode = hashCode * -1521134295 + ClampV.GetHashCode();
            hashCode = hashCode * -1521134295 + ClampU.GetHashCode();
            hashCode = hashCode * -1521134295 + FlipV.GetHashCode();
            hashCode = hashCode * -1521134295 + FlipU.GetHashCode();
            hashCode = hashCode * -1521134295 + IgnoreSpecular.GetHashCode();
            hashCode = hashCode * -1521134295 + UseAlpha.GetHashCode();
            hashCode = hashCode * -1521134295 + UseTexture.GetHashCode();
            hashCode = hashCode * -1521134295 + EnvironmentMap.GetHashCode();
            hashCode = hashCode * -1521134295 + DoubleSided.GetHashCode();
            hashCode = hashCode * -1521134295 + FlatShading.GetHashCode();
            hashCode = hashCode * -1521134295 + IgnoreLighting.GetHashCode();
            hashCode = hashCode * -1521134295 + DestinationAlpha.GetHashCode();
            hashCode = hashCode * -1521134295 + SourceAlpha.GetHashCode();
            return hashCode;
        }

        public static bool operator ==( Material material1, Material material2 )
        {
            return EqualityComparer<Material>.Default.Equals( material1, material2 );
        }

        public static bool operator !=( Material material1, Material material2 )
        {
            return !( material1 == material2 );
        }
    }
}