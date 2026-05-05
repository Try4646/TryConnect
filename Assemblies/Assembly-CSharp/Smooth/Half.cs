using System;
using System.Diagnostics;
using System.Globalization;

namespace Smooth
{
	// Token: 0x02000359 RID: 857
	[Serializable]
	public struct Half : IComparable, IFormattable, IConvertible, IComparable<Half>, IEquatable<Half>
	{
		// Token: 0x06001BF4 RID: 7156 RVA: 0x000783FA File Offset: 0x000765FA
		public Half(float value)
		{
			this = HalfHelper.SingleToHalf(value);
		}

		// Token: 0x06001BF5 RID: 7157 RVA: 0x00078408 File Offset: 0x00076608
		public Half(int value)
		{
			this = new Half((float)value);
		}

		// Token: 0x06001BF6 RID: 7158 RVA: 0x00078408 File Offset: 0x00076608
		public Half(long value)
		{
			this = new Half((float)value);
		}

		// Token: 0x06001BF7 RID: 7159 RVA: 0x00078408 File Offset: 0x00076608
		public Half(double value)
		{
			this = new Half((float)value);
		}

		// Token: 0x06001BF8 RID: 7160 RVA: 0x00078412 File Offset: 0x00076612
		public Half(decimal value)
		{
			this = new Half((float)value);
		}

		// Token: 0x06001BF9 RID: 7161 RVA: 0x00078421 File Offset: 0x00076621
		public Half(uint value)
		{
			this = new Half(value);
		}

		// Token: 0x06001BFA RID: 7162 RVA: 0x00078421 File Offset: 0x00076621
		public Half(ulong value)
		{
			this = new Half(value);
		}

		// Token: 0x06001BFB RID: 7163 RVA: 0x0007842C File Offset: 0x0007662C
		public static Half Negate(Half half)
		{
			return -half;
		}

		// Token: 0x06001BFC RID: 7164 RVA: 0x00078434 File Offset: 0x00076634
		public static Half Add(Half half1, Half half2)
		{
			return half1 + half2;
		}

		// Token: 0x06001BFD RID: 7165 RVA: 0x0007843D File Offset: 0x0007663D
		public static Half Subtract(Half half1, Half half2)
		{
			return half1 - half2;
		}

		// Token: 0x06001BFE RID: 7166 RVA: 0x00078446 File Offset: 0x00076646
		public static Half Multiply(Half half1, Half half2)
		{
			return half1 * half2;
		}

		// Token: 0x06001BFF RID: 7167 RVA: 0x0007844F File Offset: 0x0007664F
		public static Half Divide(Half half1, Half half2)
		{
			return half1 / half2;
		}

		// Token: 0x06001C00 RID: 7168 RVA: 0x00078458 File Offset: 0x00076658
		public static Half operator +(Half half)
		{
			return half;
		}

		// Token: 0x06001C01 RID: 7169 RVA: 0x0007845B File Offset: 0x0007665B
		public static Half operator -(Half half)
		{
			return HalfHelper.Negate(half);
		}

		// Token: 0x06001C02 RID: 7170 RVA: 0x00078463 File Offset: 0x00076663
		public static Half operator ++(Half half)
		{
			return (Half)(half + 1f);
		}

		// Token: 0x06001C03 RID: 7171 RVA: 0x00078476 File Offset: 0x00076676
		public static Half operator --(Half half)
		{
			return (Half)(half - 1f);
		}

		// Token: 0x06001C04 RID: 7172 RVA: 0x00078489 File Offset: 0x00076689
		public static Half operator +(Half half1, Half half2)
		{
			return (Half)(half1 + half2);
		}

		// Token: 0x06001C05 RID: 7173 RVA: 0x0007849F File Offset: 0x0007669F
		public static Half operator -(Half half1, Half half2)
		{
			return (Half)(half1 - half2);
		}

		// Token: 0x06001C06 RID: 7174 RVA: 0x000784B5 File Offset: 0x000766B5
		public static Half operator *(Half half1, Half half2)
		{
			return (Half)(half1 * half2);
		}

		// Token: 0x06001C07 RID: 7175 RVA: 0x000784CB File Offset: 0x000766CB
		public static Half operator /(Half half1, Half half2)
		{
			return (Half)(half1 / half2);
		}

		// Token: 0x06001C08 RID: 7176 RVA: 0x000784E1 File Offset: 0x000766E1
		public static bool operator ==(Half half1, Half half2)
		{
			return !Half.IsNaN(half1) && half1.internalValue == half2.internalValue;
		}

		// Token: 0x06001C09 RID: 7177 RVA: 0x000784FB File Offset: 0x000766FB
		public static bool operator !=(Half half1, Half half2)
		{
			return half1.internalValue != half2.internalValue;
		}

		// Token: 0x06001C0A RID: 7178 RVA: 0x0007850E File Offset: 0x0007670E
		public static bool operator <(Half half1, Half half2)
		{
			return half1 < half2;
		}

		// Token: 0x06001C0B RID: 7179 RVA: 0x00078520 File Offset: 0x00076720
		public static bool operator >(Half half1, Half half2)
		{
			return half1 > half2;
		}

		// Token: 0x06001C0C RID: 7180 RVA: 0x00078532 File Offset: 0x00076732
		public static bool operator <=(Half half1, Half half2)
		{
			return half1 == half2 || half1 < half2;
		}

		// Token: 0x06001C0D RID: 7181 RVA: 0x00078546 File Offset: 0x00076746
		public static bool operator >=(Half half1, Half half2)
		{
			return half1 == half2 || half1 > half2;
		}

		// Token: 0x06001C0E RID: 7182 RVA: 0x0007855A File Offset: 0x0007675A
		public static implicit operator Half(byte value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C0F RID: 7183 RVA: 0x0007855A File Offset: 0x0007675A
		public static implicit operator Half(short value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C10 RID: 7184 RVA: 0x0007855A File Offset: 0x0007675A
		public static implicit operator Half(char value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C11 RID: 7185 RVA: 0x0007855A File Offset: 0x0007675A
		public static implicit operator Half(int value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x0007855A File Offset: 0x0007675A
		public static implicit operator Half(long value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C13 RID: 7187 RVA: 0x0007855A File Offset: 0x0007675A
		public static explicit operator Half(float value)
		{
			return new Half(value);
		}

		// Token: 0x06001C14 RID: 7188 RVA: 0x0007855A File Offset: 0x0007675A
		public static explicit operator Half(double value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C15 RID: 7189 RVA: 0x00078563 File Offset: 0x00076763
		public static explicit operator Half(decimal value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C16 RID: 7190 RVA: 0x00078571 File Offset: 0x00076771
		public static explicit operator byte(Half value)
		{
			return (byte)value;
		}

		// Token: 0x06001C17 RID: 7191 RVA: 0x0007857B File Offset: 0x0007677B
		public static explicit operator char(Half value)
		{
			return (char)value;
		}

		// Token: 0x06001C18 RID: 7192 RVA: 0x00078585 File Offset: 0x00076785
		public static explicit operator short(Half value)
		{
			return (short)value;
		}

		// Token: 0x06001C19 RID: 7193 RVA: 0x0007858F File Offset: 0x0007678F
		public static explicit operator int(Half value)
		{
			return (int)value;
		}

		// Token: 0x06001C1A RID: 7194 RVA: 0x00078599 File Offset: 0x00076799
		public static explicit operator long(Half value)
		{
			return (long)value;
		}

		// Token: 0x06001C1B RID: 7195 RVA: 0x000785A3 File Offset: 0x000767A3
		public static implicit operator float(Half value)
		{
			return HalfHelper.HalfToSingle(value);
		}

		// Token: 0x06001C1C RID: 7196 RVA: 0x000785AC File Offset: 0x000767AC
		public static implicit operator double(Half value)
		{
			return (double)value;
		}

		// Token: 0x06001C1D RID: 7197 RVA: 0x000785B6 File Offset: 0x000767B6
		public static explicit operator decimal(Half value)
		{
			return (decimal)value;
		}

		// Token: 0x06001C1E RID: 7198 RVA: 0x0007855A File Offset: 0x0007675A
		public static implicit operator Half(sbyte value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C1F RID: 7199 RVA: 0x0007855A File Offset: 0x0007675A
		public static implicit operator Half(ushort value)
		{
			return new Half((float)value);
		}

		// Token: 0x06001C20 RID: 7200 RVA: 0x000785C4 File Offset: 0x000767C4
		public static implicit operator Half(uint value)
		{
			return new Half(value);
		}

		// Token: 0x06001C21 RID: 7201 RVA: 0x000785C4 File Offset: 0x000767C4
		public static implicit operator Half(ulong value)
		{
			return new Half(value);
		}

		// Token: 0x06001C22 RID: 7202 RVA: 0x000785CE File Offset: 0x000767CE
		public static explicit operator sbyte(Half value)
		{
			return (sbyte)value;
		}

		// Token: 0x06001C23 RID: 7203 RVA: 0x0007857B File Offset: 0x0007677B
		public static explicit operator ushort(Half value)
		{
			return (ushort)value;
		}

		// Token: 0x06001C24 RID: 7204 RVA: 0x000785D8 File Offset: 0x000767D8
		public static explicit operator uint(Half value)
		{
			return (uint)value;
		}

		// Token: 0x06001C25 RID: 7205 RVA: 0x000785E2 File Offset: 0x000767E2
		public static explicit operator ulong(Half value)
		{
			return (ulong)value;
		}

		// Token: 0x06001C26 RID: 7206 RVA: 0x000785EC File Offset: 0x000767EC
		public int CompareTo(Half other)
		{
			int result = 0;
			if (this < other)
			{
				result = -1;
			}
			else if (this > other)
			{
				result = 1;
			}
			else if (this != other)
			{
				if (!Half.IsNaN(this))
				{
					result = 1;
				}
				else if (!Half.IsNaN(other))
				{
					result = -1;
				}
			}
			return result;
		}

		// Token: 0x06001C27 RID: 7207 RVA: 0x0007864C File Offset: 0x0007684C
		public int CompareTo(object obj)
		{
			int result;
			if (obj == null)
			{
				result = 1;
			}
			else
			{
				if (!(obj is Half))
				{
					throw new ArgumentException("Object must be of type Half.");
				}
				result = this.CompareTo((Half)obj);
			}
			return result;
		}

		// Token: 0x06001C28 RID: 7208 RVA: 0x00078685 File Offset: 0x00076885
		public bool Equals(Half other)
		{
			return other == this || (Half.IsNaN(other) && Half.IsNaN(this));
		}

		// Token: 0x06001C29 RID: 7209 RVA: 0x000786AC File Offset: 0x000768AC
		public override bool Equals(object obj)
		{
			bool result = false;
			if (obj is Half)
			{
				Half half = (Half)obj;
				if (half == this || (Half.IsNaN(half) && Half.IsNaN(this)))
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06001C2A RID: 7210 RVA: 0x000786F0 File Offset: 0x000768F0
		public override int GetHashCode()
		{
			return this.internalValue.GetHashCode();
		}

		// Token: 0x06001C2B RID: 7211 RVA: 0x000786FD File Offset: 0x000768FD
		public TypeCode GetTypeCode()
		{
			return (TypeCode)255;
		}

		// Token: 0x06001C2C RID: 7212 RVA: 0x00078704 File Offset: 0x00076904
		public static byte[] GetBytes(Half value)
		{
			return BitConverter.GetBytes(value.internalValue);
		}

		// Token: 0x06001C2D RID: 7213 RVA: 0x00078711 File Offset: 0x00076911
		public static ushort GetBits(Half value)
		{
			return value.internalValue;
		}

		// Token: 0x06001C2E RID: 7214 RVA: 0x00078719 File Offset: 0x00076919
		public static Half ToHalf(byte[] value, int startIndex)
		{
			return Half.ToHalf((ushort)BitConverter.ToInt16(value, startIndex));
		}

		// Token: 0x06001C2F RID: 7215 RVA: 0x00078728 File Offset: 0x00076928
		public static Half ToHalf(ushort bits)
		{
			return new Half
			{
				internalValue = bits
			};
		}

		// Token: 0x06001C30 RID: 7216 RVA: 0x00078746 File Offset: 0x00076946
		public static int Sign(Half value)
		{
			if (value < 0)
			{
				return -1;
			}
			if (value > 0)
			{
				return 1;
			}
			if (value != 0)
			{
				throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
			}
			return 0;
		}

		// Token: 0x06001C31 RID: 7217 RVA: 0x00078782 File Offset: 0x00076982
		public static Half Abs(Half value)
		{
			return HalfHelper.Abs(value);
		}

		// Token: 0x06001C32 RID: 7218 RVA: 0x0007878A File Offset: 0x0007698A
		public static Half Max(Half value1, Half value2)
		{
			if (!(value1 < value2))
			{
				return value1;
			}
			return value2;
		}

		// Token: 0x06001C33 RID: 7219 RVA: 0x00078798 File Offset: 0x00076998
		public static Half Min(Half value1, Half value2)
		{
			if (!(value1 < value2))
			{
				return value2;
			}
			return value1;
		}

		// Token: 0x06001C34 RID: 7220 RVA: 0x000787A6 File Offset: 0x000769A6
		public static bool IsNaN(Half half)
		{
			return HalfHelper.IsNaN(half);
		}

		// Token: 0x06001C35 RID: 7221 RVA: 0x000787AE File Offset: 0x000769AE
		public static bool IsInfinity(Half half)
		{
			return HalfHelper.IsInfinity(half);
		}

		// Token: 0x06001C36 RID: 7222 RVA: 0x000787B6 File Offset: 0x000769B6
		public static bool IsNegativeInfinity(Half half)
		{
			return HalfHelper.IsNegativeInfinity(half);
		}

		// Token: 0x06001C37 RID: 7223 RVA: 0x000787BE File Offset: 0x000769BE
		public static bool IsPositiveInfinity(Half half)
		{
			return HalfHelper.IsPositiveInfinity(half);
		}

		// Token: 0x06001C38 RID: 7224 RVA: 0x000787C6 File Offset: 0x000769C6
		public static Half Parse(string value)
		{
			return (Half)float.Parse(value, CultureInfo.InvariantCulture);
		}

		// Token: 0x06001C39 RID: 7225 RVA: 0x000787D8 File Offset: 0x000769D8
		public static Half Parse(string value, IFormatProvider provider)
		{
			return (Half)float.Parse(value, provider);
		}

		// Token: 0x06001C3A RID: 7226 RVA: 0x000787E6 File Offset: 0x000769E6
		public static Half Parse(string value, NumberStyles style)
		{
			return (Half)float.Parse(value, style, CultureInfo.InvariantCulture);
		}

		// Token: 0x06001C3B RID: 7227 RVA: 0x000787F9 File Offset: 0x000769F9
		public static Half Parse(string value, NumberStyles style, IFormatProvider provider)
		{
			return (Half)float.Parse(value, style, provider);
		}

		// Token: 0x06001C3C RID: 7228 RVA: 0x00078808 File Offset: 0x00076A08
		public static bool TryParse(string value, out Half result)
		{
			float value2;
			if (float.TryParse(value, out value2))
			{
				result = (Half)value2;
				return true;
			}
			result = default(Half);
			return false;
		}

		// Token: 0x06001C3D RID: 7229 RVA: 0x00078838 File Offset: 0x00076A38
		public static bool TryParse(string value, NumberStyles style, IFormatProvider provider, out Half result)
		{
			bool result2 = false;
			float value2;
			if (float.TryParse(value, style, provider, out value2))
			{
				result = (Half)value2;
				result2 = true;
			}
			else
			{
				result = default(Half);
			}
			return result2;
		}

		// Token: 0x06001C3E RID: 7230 RVA: 0x0007886C File Offset: 0x00076A6C
		public override string ToString()
		{
			return this.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x06001C3F RID: 7231 RVA: 0x00078894 File Offset: 0x00076A94
		public string ToString(IFormatProvider formatProvider)
		{
			return this.ToString(formatProvider);
		}

		// Token: 0x06001C40 RID: 7232 RVA: 0x000788B8 File Offset: 0x00076AB8
		public string ToString(string format)
		{
			return this.ToString(format, CultureInfo.InvariantCulture);
		}

		// Token: 0x06001C41 RID: 7233 RVA: 0x000788E0 File Offset: 0x00076AE0
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToString(format, formatProvider);
		}

		// Token: 0x06001C42 RID: 7234 RVA: 0x00078903 File Offset: 0x00076B03
		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return this;
		}

		// Token: 0x06001C43 RID: 7235 RVA: 0x00078911 File Offset: 0x00076B11
		TypeCode IConvertible.GetTypeCode()
		{
			return this.GetTypeCode();
		}

		// Token: 0x06001C44 RID: 7236 RVA: 0x00078919 File Offset: 0x00076B19
		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this);
		}

		// Token: 0x06001C45 RID: 7237 RVA: 0x0007892C File Offset: 0x00076B2C
		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this);
		}

		// Token: 0x06001C46 RID: 7238 RVA: 0x0007893F File Offset: 0x00076B3F
		char IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "Char"));
		}

		// Token: 0x06001C47 RID: 7239 RVA: 0x0007895F File Offset: 0x00076B5F
		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "DateTime"));
		}

		// Token: 0x06001C48 RID: 7240 RVA: 0x0007897F File Offset: 0x00076B7F
		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this);
		}

		// Token: 0x06001C49 RID: 7241 RVA: 0x00078992 File Offset: 0x00076B92
		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this);
		}

		// Token: 0x06001C4A RID: 7242 RVA: 0x000789A5 File Offset: 0x00076BA5
		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this);
		}

		// Token: 0x06001C4B RID: 7243 RVA: 0x000789B8 File Offset: 0x00076BB8
		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this);
		}

		// Token: 0x06001C4C RID: 7244 RVA: 0x000789CB File Offset: 0x00076BCB
		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this);
		}

		// Token: 0x06001C4D RID: 7245 RVA: 0x000789DE File Offset: 0x00076BDE
		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this);
		}

		// Token: 0x06001C4E RID: 7246 RVA: 0x000789F1 File Offset: 0x00076BF1
		string IConvertible.ToString(IFormatProvider provider)
		{
			return Convert.ToString(this, CultureInfo.InvariantCulture);
		}

		// Token: 0x06001C4F RID: 7247 RVA: 0x00078A09 File Offset: 0x00076C09
		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			return ((IConvertible)this).ToType(conversionType, provider);
		}

		// Token: 0x06001C50 RID: 7248 RVA: 0x00078A23 File Offset: 0x00076C23
		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this);
		}

		// Token: 0x06001C51 RID: 7249 RVA: 0x00078A36 File Offset: 0x00076C36
		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this);
		}

		// Token: 0x06001C52 RID: 7250 RVA: 0x00078A49 File Offset: 0x00076C49
		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this);
		}

		// Token: 0x040012B3 RID: 4787
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public ushort internalValue;

		// Token: 0x040012B4 RID: 4788
		public static readonly Half Epsilon = Half.ToHalf(1);

		// Token: 0x040012B5 RID: 4789
		public static readonly Half MaxValue = Half.ToHalf(31743);

		// Token: 0x040012B6 RID: 4790
		public static readonly Half MinValue = Half.ToHalf(64511);

		// Token: 0x040012B7 RID: 4791
		public static readonly Half NaN = Half.ToHalf(65024);

		// Token: 0x040012B8 RID: 4792
		public static readonly Half NegativeInfinity = Half.ToHalf(64512);

		// Token: 0x040012B9 RID: 4793
		public static readonly Half PositiveInfinity = Half.ToHalf(31744);
	}
}
