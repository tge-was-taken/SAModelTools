namespace SAModelLibrary
{
    /// <summary>
    /// Defines destination alpha operations.
    /// </summary>
    public enum DstAlphaOp
    {
        /// <summary>
        /// NJD_DA_ZERO
        /// </summary>
        Zero = 0,

        /// <summary>
        /// NJD_DA_ONE
        /// </summary>
        One = 1 << 0,

        /// <summary>
        /// NJD_DA_OTHER
        /// </summary>
        Other = 1 << 1,

        /// <summary>
        /// NJD_DA_INV_OTHER
        /// </summary>
        InverseOther = Other | One,

        /// <summary>
        /// NJD_DA_SRC
        /// </summary>
        Src = 1 << 2,

        /// <summary>
        /// NJD_DA_INV_SRC
        /// </summary>
        InverseSrc = Src | One,

        /// <summary>
        /// NJD_DA_DST
        /// </summary>
        Dst,

        /// <summary>
        /// NJD_DA_INV_DST
        /// </summary>
        InverseDst = Src | One | One,
    };
}