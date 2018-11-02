using System.Numerics;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats.GC
{
    public abstract class VertexAttributeBuffer
    {
        public abstract VertexAttributeType Type { get; }

        public abstract byte ElementSize { get; }

        public abstract ushort ElementCount { get; }

        public abstract int Field04 { get; }

        public abstract int DataSize { get; }
    }

    public abstract class VertexAttributeBuffer<T> : VertexAttributeBuffer
    {
        public T[] Elements { get; set; }

        public override ushort ElementCount => ( ushort ) Elements.Length;

        public override int DataSize => Elements.Length * ElementSize;

        protected VertexAttributeBuffer( T[] elements )
        {
            Elements = elements;
        }
    }

    public class VertexPositionBuffer : VertexAttributeBuffer<Vector3>
    {
        public override VertexAttributeType Type => VertexAttributeType.Position;

        public override byte ElementSize => 12;

        public override int Field04 => 65;

        public VertexPositionBuffer( Vector3[] elements ) : base( elements ) { }
    }

    public class VertexNormalBuffer : VertexAttributeBuffer<Vector3>
    {
        public override VertexAttributeType Type => VertexAttributeType.Normal;

        public override byte ElementSize => 12;

        public override int Field04 => 66;

        public VertexNormalBuffer( Vector3[] elements ) : base( elements ) { }
    }

    public class VertexColorBuffer : VertexAttributeBuffer<Color>
    {
        public override VertexAttributeType Type => VertexAttributeType.Color;

        public override byte ElementSize => 4;

        public override int Field04 => 166;

        public VertexColorBuffer( Color[] elements ) : base( elements ) { }
    }

    public class VertexUVBuffer : VertexAttributeBuffer<Vector2<short>>
    {
        public override VertexAttributeType Type => VertexAttributeType.UV;

        public override byte ElementSize => 4;

        public override int Field04 => 56;

        public VertexUVBuffer( Vector2<short>[] elements ) : base( elements ) { }
    }

    public enum VertexAttributeType
    {
        Position = 1,
        Normal,
        Color,
        UV = 5,
        End = 0xFF
    }
}
