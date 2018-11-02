namespace SAModelLibrary
{
    /// <summary>
    /// Represents the possible mip-map D adjust values.
    /// </summary>
    public enum MipMapDAdjust
    {
        D025 = 1 << 1,
        D050 = 1 << 2,
        D075 = D025 | D050,
        D100 = 1 << 3,
        D125 = D100 | D025,
        D150 = D100 | D050,
        D175 = D100 | D050 | D025,
        D200 = 1 << 4,
        D225 = D200 | D025,
        D250 = D200 | D050,
        D275 = D200 | D050 | D025,
        D300 = 1 << 5,
        D325 = D300 | D025,
        D350 = D300 | D050,
        D375 = D300 | D050 | D025,
    }
}