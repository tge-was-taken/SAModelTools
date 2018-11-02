namespace SAModelLibrary.GeometryFormats.GC
{
    public struct Index
    {
        /// <summary>
        /// Index of the vertex position to use.
        /// </summary>
        public ushort PositionIndex;

        /// <summary>
        /// Index of the vertex normal to use.
        /// </summary>
        public ushort NormalIndex;

        /// <summary>
        /// Index of the vertex color to use.
        /// </summary>
        public ushort ColorIndex;

        /// <summary>
        /// Index of the uv color to use.
        /// </summary>
        public ushort UVIndex;
    }
}