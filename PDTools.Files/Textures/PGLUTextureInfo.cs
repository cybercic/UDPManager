﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Syroot.BinaryData;

namespace PDTools.Files.Textures
{
    public class PGLUTextureInfo
    {
        public uint Head0 { get; set; }
        public uint Offset { get; set; }
        public byte Mipmap { get; set; }
        public CELL_GCM_TEXTURE_FORMAT FormatBits { get; set; }

        /// <summary>
        /// 1 = 1D
        /// 2 = 2D
        /// </summary>
        public CELL_GCM_TEXTURE_DIMENSION Dimension { get; set; } = CELL_GCM_TEXTURE_DIMENSION.CELL_GCM_TEXTURE_DIMENSION_2;

        public bool NoBorder { get; set; } = true;
        public bool CubeMap { get; set; } = false;

        /// <summary>
        /// 1 = local memory
        /// 2 = main memory
        /// </summary>
        public byte Location { get; set; } = 1;
        public byte ZFunc { get; set; } = 0;
        public byte Gamma { get; set; } = 0;
        public byte SignExt { get; set; } = 6;
        public CELL_GCM_TEXTURE_WRAP WrapT { get; set; } = CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_CLAMP_TO_EDGE;
        public byte UseAniso { get; set; } = 0;
        public CELL_GCM_TEXTURE_WRAP WrapS { get; set; } = CELL_GCM_TEXTURE_WRAP.CELL_GCM_TEXTURE_CLAMP_TO_EDGE;
        public CELL_GCM_BOOL VertexTextureSamplerEnable { get; set; } = CELL_GCM_BOOL.CELL_GCM_TRUE;

        /// <summary>
        /// min LOD of texture reduction filter
        /// 12-bit unsigned fixed point value from 0 to 12
        /// </summary>
        public short LODMin { get; set; }

        /// <summary>
        /// max LOD of texture reduction filter
        /// 12-bit unsigned fixed point value from 0 to 12
        /// </summary>
        public short LODMax { get; set; } = 3840;
        public CELL_GCM_TEXTURE_MAX_ANISO MaxAniso { get; set; }

        /// <summary>
        /// 1 bit per color, to hold CELL_GCM_BOOL on whether they are handled as complement of 2
        /// </summary>
        public byte SignedRGBA { get; set; }

        public CELL_GCM_TEXTURE_MAG Mag { get; set; } = CELL_GCM_TEXTURE_MAG.CELL_GCM_TEXTURE_LINEAR_MAG;
        public CELL_GCM_TEXTURE_MIN Min { get; set; } = CELL_GCM_TEXTURE_MIN.CELL_GCM_TEXTURE_LINEAR;
        public CELL_GCM_TEXTURE_CONVOLUTION Convultion { get; set; } = CELL_GCM_TEXTURE_CONVOLUTION.CELL_GCM_TEXTURE_CONVOLUTION_QUINCUNX;
        public byte LODBias { get; set; }
        public int BorderColor { get; set; }

        public ushort Width { get; set; }
        public ushort Height { get; set; }

        public short Depth { get; set; } = 1;
        public int Pitch { get; set; }

        public uint ImageId { get; set; }

        public void Write(BinaryStream bs)
        {
            bs.WriteInt32(6656); // head0
            bs.WriteInt32(0); // offset (runtime)
            bs.WriteByte(0); // pad0
            bs.WriteByte((byte)(Mipmap + 1));
            bs.WriteByte((byte)FormatBits);

            byte bits = 0;
            bits |= (byte)(((byte)Dimension & 0b_1111) << 4);
            bits |= (byte)(((NoBorder ? 1 : 0) & 1) << 3);
            bits |= (byte)(((CubeMap ? 1 : 0) & 1) << 2);
            bits |= (byte)(Location & 0b_11);
            bs.WriteByte(bits);

            int bits2 = 0;
            bits2 |= ((ZFunc & 0b_1_1111) << 27);
            bits2 |= ((Gamma & 0b_1111_1111) << 19);
            bits2 |= ((SignExt & 0b_1111) << 15);
            bits2 |= ((0 << 0b111) << 12);
            bits2 |= (((byte)WrapT & 0b_1111) << 8);
            bits2 |= ((UseAniso & 0b_111) << 5);
            bits2 |= ((byte)WrapS & 0b_1_1111);
            bs.WriteInt32(bits2);

            int bits3 = 0;
            bits3 |= (((byte)VertexTextureSamplerEnable & 31) << 31);
            bits3 |= ((LODMin & 0b_1111_1111_1111) << 19);
            bits3 |= ((LODMax & 0b_1111_1111_1111) << 7);
            bits3 |= (((byte)MaxAniso << 5) & 0b_111);
            // 4 bit pad
            bs.WriteInt32(bits3);
            bs.WriteInt32(43748); // remap

            int bits4 = 0;
            bits4 |= ((SignedRGBA & 0b1_1111) << 27);
            bits4 |= (((byte)Mag & 0b_111) << 24);
            bits4 |= (((byte)Min & 0b_1111_1111) << 16);
            bits4 |= (((byte)Convultion & 0b_111) << 13);
            bits4 |= (LODBias & 0b_1111_1111_1111);
            bs.WriteInt32(bits4);

            bs.WriteUInt16(Width);
            bs.WriteUInt16(Height);
            bs.WriteInt32(BorderColor);
            bs.WriteInt32(6208); // head1 fixme

            int bits5 = 0;
            bits5 |= (int)((Depth & 0x1111_1111_1111) << 20);
            bits5 |= (Pitch & 0b1111_1111_1111_1111_1111);
            bs.WriteInt32(bits5); // head1 fixme

            bs.WriteInt32(0); // Reserved.. or not?
            bs.WriteInt32(0); // Same
            bs.WriteInt32(0);
            bs.WriteUInt32(ImageId); // Image Id
            bs.WriteInt32(0);
            bs.WriteInt32(0); // Img name offset to write later if exists
        }

        public static PGLUTextureInfo FromStream(BinaryStream bs)
        {
            var info = new PGLUTextureInfo();
            info.Head0 = bs.ReadUInt32();
            info.Offset = bs.ReadUInt32();
            bs.ReadByte(); // pad0
            info.Mipmap = bs.Read1Byte();
            info.FormatBits = (CELL_GCM_TEXTURE_FORMAT)bs.Read1Byte();

            byte bits1 = bs.Read1Byte();
            info.Dimension = (CELL_GCM_TEXTURE_DIMENSION)(bits1 >> 4);
            info.NoBorder = (byte)((bits1 >> 3) & 1) == 1;
            info.CubeMap = (byte)((bits1 >> 2) & 1) == 1;
            info.Location = (byte)(bits1 & 0b11);

            uint bits2 = bs.ReadUInt32();
            info.ZFunc = (byte)((bits2 >> 27) & 0x11111);
            info.Gamma = (byte)((bits2 >> 19) & 0x1111_1111);
            info.SignExt = (byte)((bits2 >> 15) & 0x1111);
            // unk 3 bits
            info.WrapT = (CELL_GCM_TEXTURE_WRAP)((bits2 >> 8) & 0b1111);
            info.UseAniso = (byte)((bits2 >> 5) & 0b111);
            info.WrapS = (CELL_GCM_TEXTURE_WRAP)(bits2 & 0b11111);

            uint bits3 = bs.ReadUInt32();
            info.VertexTextureSamplerEnable = (CELL_GCM_BOOL)((bits3 >> 31) & 1);
            info.LODMin = (short)((bits3 >> 19) & 0b_1111_1111_1111);
            info.LODMax = (short)((bits3 >> 7) & 0b_1111_1111_1111);
            info.MaxAniso = (CELL_GCM_TEXTURE_MAX_ANISO)((bits3 >> 5) & 0b_111);
            // 4 bit pad

            bs.ReadInt32(); // remap

            uint bits4 = bs.ReadUInt32();
            info.SignedRGBA = (byte)((bits4 >> 27) & 0b1_1111);
            info.Mag = (CELL_GCM_TEXTURE_MAG)((bits4 >> 24) & 0b_111);
            info.Min = (CELL_GCM_TEXTURE_MIN)((bits4 >> 16) & 0b_1111_1111);
            info.Convultion = (CELL_GCM_TEXTURE_CONVOLUTION)((bits4 >> 13) & 0b_111);
            info.LODBias = (byte)(bits4 & 0b_1111_1111_1111);

            info.Width = bs.ReadUInt16();
            info.Height = bs.ReadUInt16();
            info.BorderColor = bs.ReadInt32();
            bs.ReadInt32(); // head1

            uint bits5 = bs.ReadUInt32();
            info.Depth = (short)((bits5 >> 20) & 0x1111_1111_1111);
            info.Pitch = (int)((bits5 & 0b1111_1111_1111_1111_1111));

            bs.ReadUInt32();
            bs.ReadUInt32();
            bs.ReadUInt32();
            info.ImageId = bs.ReadUInt32();
            bs.ReadUInt32();
            uint imageNameOffset = bs.ReadUInt32();

            return info;
        }
    }
}
