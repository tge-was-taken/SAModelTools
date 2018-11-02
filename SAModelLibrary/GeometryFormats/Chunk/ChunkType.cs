namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// TODO: Documentation
    /// </summary>
    public enum ChunkType : byte
    {
        //
        // Null chunk
        // <Format>=[ChunkHead(16)](16 bits chunk) 
        //

        // NJD_CN (16 bits)
        Null = 0,

        //
        // Chunk bits (16 bits)
        // <Format>=[bits(8)|ChunkHead(8)](16 bits chunk)
        //

        // NJD_CB_BA
        // 13-11 = SRC Alpha Instruction(3)
        // 10- 8 = DST Alpha Instruction(3)
        BlendAlpha = 1,

        // NJD_CB_DA
        // 11- 8 = Mipmap 'D' adjust(4) 
        MipmapDAdjust = 2,

        // NJD_CB_EXP
        // 12- 8 = Exponent(5) range:0-16
        SpecularExponent = 3,

        // NJD_CB_CP
        // 15- 8 = Cache Number range:0-255
        CachePolygonList = 4,

        // NJD_CB_DP
        // 15- 8 = Cache Number range:0-255 
        DrawPolygonList = 5,

        //
        // Chunk tiny (32 bits)
        // <Format>=[headbits(8)|ChunkHead(8)][texbits(3)|TexId(13)] (32 bits chunk)
        //

        /* TID : Set Texture                      */
        /*     <headbits>                         */
        /*       15-14 = FlipUV(2)                */
        /*       13-12 = ClampUV(2)               */
        /*       11- 8 = Mipmap 'D' adjust(4)     */
        /*     <texbits>                          */
        /*       15-14 = Filter Mode(2)           */
        /*       13    = Super Sample(1)          */
        /*     (TexId Max = 8191)                 */
        // NJD_CT_TID
        TextureId = 8,
        TextureId2 = 9,

        //
        // Chunk material
        /* <Format>=[ChunkHead][Size][Data]                        */
        /*       13-11 = SRC Alpha Instruction(3)                  */
        /*       10- 8 = DST Alpha Instruction(3)                  */
        /* D  : Diffuse (ARGB)                            bit 0    */
        /* A  : Ambient (RGB)                             bit 1    */
        /* S  : Specular(ERGB) E:exponent(5) range:0-16   bit 2    */
        //
        MaterialDiffuse = 17,                 /* [CHead][4(Size)][ARGB]              */
        MaterialAmbient = 18,                 /* [CHead][4(Size)][NRGB] N: NOOP(255) */
        MaterialDiffuseAmbient = 19,          /* [CHead][8(Size)][ARGB][NRGB]        */
        MaterialSpecular = 20,                /* [CHead][4(Size)][ERGB] E: Exponent  */
        MaterialDiffuseSpecular = 21,         /* [CHead][8(Size)][ARGB][ERGB]        */
        MaterialAmbientSpecular = 22,         /* [CHead][8(Size)][NRGB][ERGB]        */
        MaterialDiffuseAmbientSpecular = 23,  /* [CHead][12(Size)][ARGB][NRGB][ERGB] */
        MaterialBump = 24,                    /* [CHead][12(Size)][dx(16)][dy(16)][dz(16)][ux(16)][uy(16)][uz(16)] */

        MaterialDiffuse2 = 25,
        MaterialAmbient2 = 26,
        MaterialDiffuseAmbient2 = 27,
        MaterialSpecular2 = 28,
        MaterialDiffuseSpecular2 = 29,
        MaterialAmbientSpecular2 = 30,
        MaterialDiffuseAmbientSpecular2 = 31,

        //
        // Chunk vertex
        /* <Format>=[headbits(8)|ChunkHead(8)]                                    */
        /*          [Size(16)][IndexOffset(16)][nbIndices(16)]                    */
        /*     <headbits>(NF only)                                                */
        /*        9- 8 = WeightStatus(2) Start, Middle, End                       */
        //

        // NJD_CV_SH
        // XYZ|1.0f
        VertexSH = 32,

        // NJD_CV_VN_SH
        // XYZ|1.0f|NormalXYZ|0.0f
        VertexNSH = 33,

        // NJD_CV
        // XYZ
        VertexXYZ = 34,

        // NJD_CV_D8
        // XYZ|Diffuse8888
        VertexD8888 = 35,

        // NJD_CV_UF
        // XYZ|UserFlags32
        VertexUF = 36,

        // NJD_CV_NF
        // XYZ|NinjaFlags32
        VertexNF = 37,

        // NJD_CV_S5
        // XYZ|Diffuse565|Specular565
        VertexD565S565 = 38,

        // NJD_CV_S4
        // XYZ|Diffuse4444|Specular565
        VertexD4444S565 = 39,

        // NJD_CV_IN
        // XYZ|Diffuse16|Specular16
        VertexD16S16 = 40,

        // XYZ|NormalXYZ
        VertexN = 41,

        // XYZ|NormalXYZ|Diffuse8888
        VertexND8888 = 42,

        // XYZ|NormalXYZ|UserFlags32
        VertexNUF = 43,

        // XYZ|NormalXYZ|NinjaFlags32
        VertexNNF = 44,

        // XYZ|NormalXYZ|Difuse565|Specular565
        VertexND565S565 = 45,

        // XYZ|NormalXYZ|Difuse4444|Specular565
        VertexND4444S565 = 46,

        // XYZ|NormalXYZ|Diffuse16|Specular16
        VertexND16S16 = 47,

        // XYZ|NormalXYZ32 32bits vertex normal  reserved(2)|x(10)|y(10)|z(10)
        VertexN32 = 48,

        // XYZ|NormalXYZ32|Diffuse8888
        VertexN32D8888 = 49,

        // XYZ|NormalXYZ32|UserFlags32
        VertexN32UF = 50,

        /*--------------*/
        /* Chunk vOlume */
        /*--------------*/
        /* UserFlags N=0,1(16bit*1),2(16bit*2),3(16bit*3)                         */
        /* <Format P3>=[ChunkHead(16)][Size(16)][UserOffset(2)|nbPolygon(14)]     */
        /*            i0, i1, i2, UserflagPoly0(*N),                              */
        /*            i3, i4, i5, UserflagPoly1(*N), ...                          */
        /* <Format P4>=[ChunkHead(16)][Size(16)][UserOffset(2)|nbPolygon(14)]     */
        /*            i0, i1, i2, i3, UserflagPoly0(*N),                          */
        /*            i4, i5, i6, i7, UserflagPoly1(*N), ...                      */
        /* <Format ST>=[ChunkHead(16)][Size(16)][UserOffset(2)|nbStrip(14)]       */
        /*          [flag|len, i0, i1, i2, Userflag2(*N), i3, Userflag3(*N), ...  */
        /* P3  : Polygon3     (Modifier Volume)                                   */
        /* P4  : Polygon4                                                         */
        /* ST  : triangle STrip(Trimesh)                                          */

        // Format: [ChunkHead(16)][Size(16)][UserOffset(2)|nbPolygon(14)] 
        //          i0, i1, i2, UserflagPoly0(*N), 
        //          i3, i4, i5, UserflagPoly1(*N), ... 
        VolumePolygon3 = 54,

        // Format: [ChunkHead(16)][Size(16)][UserOffset(2)|nbPolygon(14)]
        //          i0, i1, i2, i3, UserflagPoly0(*N),                          
        //          i4, i5, i6, i7, UserflagPoly1(*N), ...                      
        VolumePolygon4 = 55,

        // Format: [ChunkHead(16)][Size(16)][UserOffset(2)|nbStrip(14)]       
        //         [flag|len, i0, i1, i2, Userflag2(*N), i3, Userflag3(*N), ...
        VolumeTristrip = 56,

        // 
        // Chunk Strip
        //
        //

        // [CFlags(8)|CHead(8)][Size(16)][UserOffset(2)|nbStrip(14)]    
        // flag(1)|len(15), index0(16), index1(16),                    
        // index2, UserFlag2(*N), ...]
        Strip = 64,

        // 
        StripUVN = 65,
        StripUVH = 66,
        StripVN = 67,
        StripUVNVN = 68,
        StripUVHVN = 69,
        StripD8 = 70,
        StripUVND8 = 71,
        StripUVHD8 = 72,
        Strip2,
        StripUVN2,
        StripUVH2,


        //
        // End chunk
        // <Format>=[ChunkHead(16)](16 bits chunk) 
        //

        // NJD_CE (16 bits)
        End = 255
    }
}
