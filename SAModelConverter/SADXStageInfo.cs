namespace SAModelExporter
{
    internal struct SADXStageInfo
    {
        public int Offset { get; }

        public string TexturePakFileName { get; }

        public string Name { get; }

        public SADXStageInfo( int offset, string texturePakFileName, string name )
        {
            Offset             = offset;
            TexturePakFileName = texturePakFileName;
            Name               = name;
        }
    }
}
