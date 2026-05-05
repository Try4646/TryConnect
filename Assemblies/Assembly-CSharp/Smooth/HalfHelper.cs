using System;
using System.Runtime.InteropServices;

namespace Smooth
{
	// Token: 0x0200035A RID: 858
	public static class HalfHelper
	{
		// Token: 0x06001C54 RID: 7252 RVA: 0x00078ABF File Offset: 0x00076CBF
		private static uint FloatToUInt(float v)
		{
			HalfHelper.floatToIntConverter.FloatValue = v;
			return HalfHelper.floatToIntConverter.UIntValue;
		}

		// Token: 0x06001C55 RID: 7253 RVA: 0x00078AD6 File Offset: 0x00076CD6
		private static float UIntToFloat(uint v)
		{
			HalfHelper.floatToIntConverter.UIntValue = v;
			return HalfHelper.floatToIntConverter.FloatValue;
		}

		// Token: 0x06001C56 RID: 7254 RVA: 0x00078AF0 File Offset: 0x00076CF0
		private static uint ConvertMantissa(int i)
		{
			uint num = (uint)((uint)i << 13);
			uint num2 = 0U;
			while ((num & 8388608U) == 0U)
			{
				num2 -= 8388608U;
				num <<= 1;
			}
			num &= 4286578687U;
			num2 += 947912704U;
			return num | num2;
		}

		// Token: 0x06001C57 RID: 7255 RVA: 0x00078B30 File Offset: 0x00076D30
		private static uint[] GenerateMantissaTable()
		{
			uint[] array = new uint[2048];
			array[0] = 0U;
			for (int i = 1; i < 1024; i++)
			{
				array[i] = HalfHelper.ConvertMantissa(i);
			}
			for (int j = 1024; j < 2048; j++)
			{
				array[j] = (uint)(939524096 + (j - 1024 << 13));
			}
			return array;
		}

		// Token: 0x06001C58 RID: 7256 RVA: 0x00078B90 File Offset: 0x00076D90
		private static uint[] GenerateExponentTable()
		{
			uint[] array = new uint[64];
			array[0] = 0U;
			for (int i = 1; i < 31; i++)
			{
				array[i] = (uint)((uint)i << 23);
			}
			array[31] = 1199570944U;
			array[32] = 2147483648U;
			for (int j = 33; j < 63; j++)
			{
				array[j] = (uint)((ulong)int.MinValue + (ulong)((long)((long)(j - 32) << 23)));
			}
			array[63] = 3347054592U;
			return array;
		}

		// Token: 0x06001C59 RID: 7257 RVA: 0x00078BFC File Offset: 0x00076DFC
		private static ushort[] GenerateOffsetTable()
		{
			ushort[] array = new ushort[64];
			array[0] = 0;
			for (int i = 1; i < 32; i++)
			{
				array[i] = 1024;
			}
			array[32] = 0;
			for (int j = 33; j < 64; j++)
			{
				array[j] = 1024;
			}
			return array;
		}

		// Token: 0x06001C5A RID: 7258 RVA: 0x00078C48 File Offset: 0x00076E48
		private static ushort[] GenerateBaseTable()
		{
			ushort[] array = new ushort[512];
			for (int i = 0; i < 256; i++)
			{
				sbyte b = (sbyte)(127 - i);
				if (b > 24)
				{
					array[i | 0] = 0;
					array[i | 256] = 32768;
				}
				else if (b > 14)
				{
					array[i | 0] = (ushort)(1024 >> (int)(18 + b));
					array[i | 256] = (ushort)(1024 >> (int)(18 + b) | 32768);
				}
				else if (b >= -15)
				{
					array[i | 0] = (ushort)(15 - b << 10);
					array[i | 256] = (ushort)((int)(15 - b) << 10 | 32768);
				}
				else if (b > -128)
				{
					array[i | 0] = 31744;
					array[i | 256] = 64512;
				}
				else
				{
					array[i | 0] = 31744;
					array[i | 256] = 64512;
				}
			}
			return array;
		}

		// Token: 0x06001C5B RID: 7259 RVA: 0x00078D34 File Offset: 0x00076F34
		private static sbyte[] GenerateShiftTable()
		{
			sbyte[] array = new sbyte[512];
			for (int i = 0; i < 256; i++)
			{
				sbyte b = (sbyte)(127 - i);
				if (b > 24)
				{
					array[i | 0] = 24;
					array[i | 256] = 24;
				}
				else if (b > 14)
				{
					array[i | 0] = b - 1;
					array[i | 256] = b - 1;
				}
				else if (b >= -15)
				{
					array[i | 0] = 13;
					array[i | 256] = 13;
				}
				else if (b > -128)
				{
					array[i | 0] = 24;
					array[i | 256] = 24;
				}
				else
				{
					array[i | 0] = 13;
					array[i | 256] = 13;
				}
			}
			return array;
		}

		// Token: 0x06001C5C RID: 7260 RVA: 0x00078DE3 File Offset: 0x00076FE3
		public static float HalfToSingle(Half half)
		{
			return HalfHelper.UIntToFloat(HalfHelper.mantissaTable[(int)(HalfHelper.offsetTable[half.internalValue >> 10] + (half.internalValue & 1023))] + HalfHelper.exponentTable[half.internalValue >> 10]);
		}

		// Token: 0x06001C5D RID: 7261 RVA: 0x00078E1C File Offset: 0x0007701C
		public static Half SingleToHalf(float single)
		{
			uint num = HalfHelper.FloatToUInt(single);
			return Half.ToHalf((ushort)((uint)HalfHelper.baseTable[(int)(num >> 23 & 511U)] + ((num & 8388607U) >> (int)HalfHelper.shiftTable[(int)(num >> 23)])));
		}

		// Token: 0x06001C5E RID: 7262 RVA: 0x00078E5C File Offset: 0x0007705C
		public static float Decompress(ushort compressedFloat)
		{
			return HalfHelper.UIntToFloat(HalfHelper.mantissaTable[(int)(HalfHelper.offsetTable[compressedFloat >> 10] + (compressedFloat & 1023))] + HalfHelper.exponentTable[compressedFloat >> 10]);
		}

		// Token: 0x06001C5F RID: 7263 RVA: 0x00078E88 File Offset: 0x00077088
		public static ushort Compress(float uncompressedFloat)
		{
			uint num = HalfHelper.FloatToUInt(uncompressedFloat);
			return (ushort)((uint)HalfHelper.baseTable[(int)(num >> 23 & 511U)] + ((num & 8388607U) >> (int)HalfHelper.shiftTable[(int)(num >> 23)]));
		}

		// Token: 0x06001C60 RID: 7264 RVA: 0x00078EC3 File Offset: 0x000770C3
		public static Half Negate(Half half)
		{
			return Half.ToHalf(half.internalValue ^ 32768);
		}

		// Token: 0x06001C61 RID: 7265 RVA: 0x00078ED7 File Offset: 0x000770D7
		public static Half Abs(Half half)
		{
			return Half.ToHalf(half.internalValue & 32767);
		}

		// Token: 0x06001C62 RID: 7266 RVA: 0x00078EEB File Offset: 0x000770EB
		public static bool IsNaN(Half half)
		{
			return (half.internalValue & 32767) > 31744;
		}

		// Token: 0x06001C63 RID: 7267 RVA: 0x00078F00 File Offset: 0x00077100
		public static bool IsInfinity(Half half)
		{
			return (half.internalValue & 32767) == 31744;
		}

		// Token: 0x06001C64 RID: 7268 RVA: 0x00078F15 File Offset: 0x00077115
		public static bool IsPositiveInfinity(Half half)
		{
			return half.internalValue == 31744;
		}

		// Token: 0x06001C65 RID: 7269 RVA: 0x00078F24 File Offset: 0x00077124
		public static bool IsNegativeInfinity(Half half)
		{
			return half.internalValue == 64512;
		}

		// Token: 0x040012BA RID: 4794
		private static uint[] mantissaTable = HalfHelper.GenerateMantissaTable();

		// Token: 0x040012BB RID: 4795
		private static uint[] exponentTable = HalfHelper.GenerateExponentTable();

		// Token: 0x040012BC RID: 4796
		private static ushort[] offsetTable = HalfHelper.GenerateOffsetTable();

		// Token: 0x040012BD RID: 4797
		private static ushort[] baseTable = HalfHelper.GenerateBaseTable();

		// Token: 0x040012BE RID: 4798
		private static sbyte[] shiftTable = HalfHelper.GenerateShiftTable();

		// Token: 0x040012BF RID: 4799
		private static HalfHelper.UIntFloat floatToIntConverter = new HalfHelper.UIntFloat
		{
			FloatValue = 0f
		};

		// Token: 0x0200035B RID: 859
		[StructLayout(LayoutKind.Explicit)]
		private struct UIntFloat
		{
			// Token: 0x040012C0 RID: 4800
			[FieldOffset(0)]
			public uint UIntValue;

			// Token: 0x040012C1 RID: 4801
			[FieldOffset(0)]
			public float FloatValue;
		}
	}
}
