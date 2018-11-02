namespace SAModelLibrary
{
    /// <summary>
    /// Defines source alpha operations.
    /// </summary>
    public enum SrcAlphaOp
    {
        /// <summary>
        /// NJD_SA_ZERO
        /// </summary>
        Zero = 0,

        /// <summary>
        /// NJD_SA_ONE
        /// </summary>
        One = 1 << 0,

        /// <summary>
        /// NJD_SA_OTHER
        /// </summary>
        Other = 1 << 1,

        /// <summary>
        /// NJD_SA_INV_OTHER
        /// </summary>
        InverseOther = Other | One,

        /// <summary>
        /// NJD_SA_SRC
        /// </summary>
        Src = 1 << 2,

        /// <summary>
        /// NJD_SA_INV_SRC
        /// </summary>
        InverseSrc = Src | One,

        /// <summary>
        /// NJD_SA_DST
        /// </summary>
        Dst,

        /// <summary>
        /// NJD_SA_INV_DST
        /// </summary>
        InverseDst = Src | One | One,
    };
}