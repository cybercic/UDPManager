﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDTools.Files.Textures
{
    [Flags]
    public enum CELL_GCM_TEXTURE_FORMAT : byte
    {
        /// <summary>
        /// Swizzled
        /// </summary>
        CELL_GCM_TEXTURE_SZ = 0,

        /// <summary>
        /// Linear
        /// </summary>
        CELL_GCM_TEXTURE_LN = 0x20,

        CELL_GCM_TEXTURE_B8 = 0x81,
        CELL_GCM_TEXTURE_A1R5G5B5 = 0x82,
        CELL_GCM_TEXTURE_A4R4G4B4 = 0x83,
        CELL_GCM_TEXTURE_R5G6B5 = 0x84,
        CELL_GCM_TEXTURE_A8R8G8B8 = 0x85,
        CELL_GCM_TEXTURE_COMPRESSED_DXT1 = 0x86,
        CELL_GCM_TEXTURE_COMPRESSED_DXT23 = 0x87,
        CELL_GCM_TEXTURE_COMPRESSED_DXT45 = 0x88,
        CELL_GCM_TEXTURE_G8B8 = 0x8B,
        CELL_GCM_TEXTURE_R6G5B5 = 0x8F,
        CELL_GCM_TEXTURE_DEPTH24_D8 = 0x90,
        CELL_GCM_TEXTURE_DEPTH24_D8_FLOAT = 0x91,
        CELL_GCM_TEXTURE_DEPTH16 = 0x92,
        CELL_GCM_TEXTURE_DEPTH16_FLOAT = 0x93,
        CELL_GCM_TEXTURE_X16 = 0x94,
        CELL_GCM_TEXTURE_Y16_X16 = 0x95,
        CELL_GCM_TEXTURE_R5G5B5A1 = 0x97,
        CELL_GCM_TEXTURE_COMPRESSED_HILO8 = 0x98,
        CELL_GCM_TEXTURE_COMPRESSED_HILO_S8 = 0x99,
        CELL_GCM_TEXTURE_W16_Z16_Y16_X16_FLOAT = 0x9A,
        CELL_GCM_TEXTURE_W32_Z32_Y32_X32_FLOAT = 0x9B,
        CELL_GCM_TEXTURE_X32_FLOAT = 0x9C,
        CELL_GCM_TEXTURE_D1R5G5B5 = 0x9D,
        CELL_GCM_TEXTURE_D8R8G8B8 = 0x9E,
        CELL_GCM_TEXTURE_Y16_X16_FLOAT = 0x9F,
        CELL_GCM_TEXTURE_COMPRESSED_B8R8_G8R8 = 0xAD,
        CELL_GCM_TEXTURE_COMPRESSED_R8B8_R8G8 = 0xAE,
    }

    public enum CELL_GCM_BOOL
    {
        CELL_GCM_FALSE,
        CELL_GCM_TRUE,
    }

    public enum CELL_GCM_TEXTURE_DIMENSION
    {
        CELL_GCM_TEXTURE_DIMENSION_1,
        CELL_GCM_TEXTURE_DIMENSION_2,
    }

    public enum CELL_GCM_TEXTURE_MAX_ANISO : byte
    {
        CELL_GCM_TEXTURE_MAX_ANISO_1 = 0,
        CELL_GCM_TEXTURE_MAX_ANISO_2 = 1,
        CELL_GCM_TEXTURE_MAX_ANISO_4 = 2,
        CELL_GCM_TEXTURE_MAX_ANISO_6 = 3,
        CELL_GCM_TEXTURE_MAX_ANISO_8 = 4,
        CELL_GCM_TEXTURE_MAX_ANISO_10 = 5,
        CELL_GCM_TEXTURE_MAX_ANISO_12 = 6,
        CELL_GCM_TEXTURE_MAX_ANISO_16 = 7,
    }

    public enum CELL_GCM_TEXTURE_CONVOLUTION : byte
    {
        CELL_GCM_TEXTURE_CONVOLUTION_NONE,
        CELL_GCM_TEXTURE_CONVOLUTION_QUINCUNX,
        CELL_GCM_TEXTURE_CONVOLUTION_GAUSSIAN,
        CELL_GCM_TEXTURE_CONVOLUTION_QUINCUNX_ALT,

    };

    public enum CELL_GCM_TEXTURE_MIN : byte
    {
        CELL_GCM_TEXTURE_MIN_NONE,
        CELL_GCM_TEXTURE_NEAREST,
        CELL_GCM_TEXTURE_LINEAR,
        CELL_GCM_TEXTURE_NEAREST_NEAREST,
        CELL_GCM_TEXTURE_LINEAR_NEAREST,
        CELL_GCM_TEXTURE_NEAREST_LINEAR,
        CELL_GCM_TEXTURE_LINEAR_LINEAR,
        CELL_GCM_TEXTURE_CONVOLUTION_MIN,
    }

    public enum CELL_GCM_TEXTURE_MAG : byte
    {
        CELL_GCM_TEXTURE_MAG_NONE,
        CELL_GCM_TEXTURE_NEAREST_MAG,
        CELL_GCM_TEXTURE_LINEAR_MAG,
        CELL_GCM_TEXTURE_CONVOLUTION_MAG,
    }

    public enum CELL_GCM_TEXTURE_WRAP : byte
    {
        CELL_GCM_TEXTURE_WRAP_NONE,
        CELL_GCM_TEXTURE_WRAP,
        CELL_GCM_TEXTURE_MIRROR,
        CELL_GCM_TEXTURE_CLAMP_TO_EDGE,
        CELL_GCM_TEXTURE_BORDER,
        CELL_GCM_TEXTURE_CLAMP,
        CELL_GCM_TEXTURE_MIRROR_ONCE_CLAMP_TO_EDGE,
        CELL_GCM_TEXTURE_MIRROR_ONCE_BORDER,
        CELL_GCM_TEXTURE_MIRROR_ONCE_CLAMP,
    }
}
