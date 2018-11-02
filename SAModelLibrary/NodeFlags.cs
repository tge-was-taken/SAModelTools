using System;

namespace SAModelLibrary
{
    /// <summary>
    /// Node evaluation flags
    /// </summary>
    [Flags]
    public enum NodeFlags
    {
        /// <summary>
        /// NJD_EVAL_UNIT_POS
        /// </summary>
        IgnoreTranslation = 1 << 0,

        /// <summary>
        /// NJD_EVAL_UNIT_ANG
        /// </summary>
        IgnoreRotation = 1 << 1,

        /// <summary>
        /// NJD_EVAL_UNIT_SCL
        /// </summary>
        IgnoreScale = 1 << 2,

        /// <summary>
        /// NJD_EVAL_HIDE
        /// </summary>
        Hide = 1 << 3,

        /// <summary>
        /// NJD_EVAL_BREAK
        /// </summary>
        IgnoreChildren = 1 << 4,

        /// <summary>
        /// NJD_EVAL_ZXY_ANG
        /// </summary>
        UseZXYRotation = 1 << 5,

        /// <summary>
        /// NJD_EVAL_SKIP
        /// </summary>
        Skip = 1 << 6,

        /// <summary>
        /// NJD_EVAL_SHAPE_SKIP
        /// </summary>
        SkipMorphs = 1 << 7,

        /// <summary>
        /// NJD_EVAL_CLIP
        /// </summary>
        Clip = 1 << 8,

        /// <summary>
        /// NJD_EVAL_MODIFIER
        /// </summary>
        Modifier = 1 << 9,
    };
}