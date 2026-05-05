using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

// Token: 0x020002AD RID: 685
public static class FathF
{
	// Token: 0x0600181C RID: 6172 RVA: 0x0006614A File Offset: 0x0006434A
	public static IEnumerator DelayedCall(float delay, Action action)
	{
		yield return new WaitForSeconds(delay);
		if (action != null)
		{
			action();
		}
		yield break;
	}

	// Token: 0x0600181D RID: 6173 RVA: 0x00066160 File Offset: 0x00064360
	public static Vector3 GetRandomPerpendicular(Vector3 input)
	{
		if (input == Vector3.zero)
		{
			throw new ArgumentException("Input vector cannot be zero.", "input");
		}
		Vector3 rhs = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		return Vector3.Cross(input, rhs).normalized;
	}

	// Token: 0x0600181E RID: 6174 RVA: 0x000661CD File Offset: 0x000643CD
	public static Vector3 GetHorizontalProjectionOfVector(Vector3 vector)
	{
		return new Vector3(vector.x, 0f, vector.z);
	}

	// Token: 0x0600181F RID: 6175 RVA: 0x000661E8 File Offset: 0x000643E8
	public static Vector3 GetRandomPointInCone(Vector3 origin, Vector3 direction, float angle, float distance)
	{
		direction = direction.normalized;
		float num = Random.Range(Mathf.Cos(angle * 0.017453292f / 2f), 1f);
		float f = Random.Range(0f, 6.2831855f);
		float num2 = Mathf.Sqrt(1f - num * num);
		float x = num2 * Mathf.Cos(f);
		float y = num2 * Mathf.Sin(f);
		Vector3 point = new Vector3(x, y, num);
		float d = Random.Range(0f, distance);
		Vector3 a = Quaternion.LookRotation(direction) * point;
		return origin + a * d;
	}

	// Token: 0x06001820 RID: 6176 RVA: 0x0006627F File Offset: 0x0006447F
	public static T GetNextElement<T>(this List<T> list, int index)
	{
		if (index < 0 || index >= list.Count - 1)
		{
			return list[0];
		}
		return list[index + 1];
	}

	// Token: 0x06001821 RID: 6177 RVA: 0x000662A1 File Offset: 0x000644A1
	public static T GetRandomElement<T>(this List<T> list)
	{
		if (list == null || list.Count == 0)
		{
			throw new ArgumentException("List cannot be null or empty", "list");
		}
		return list[NetworkSingleton<SeededRandomManager>.Instance.Range(0, list.Count)];
	}

	// Token: 0x06001822 RID: 6178 RVA: 0x000662D8 File Offset: 0x000644D8
	public static void DestroyChildren(this Transform transform)
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(transform.GetChild(i).gameObject);
		}
	}

	// Token: 0x06001823 RID: 6179 RVA: 0x0006630C File Offset: 0x0006450C
	public static void DestroyChildrenImmediate(this Transform transform)
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			Object.DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}

	// Token: 0x06001824 RID: 6180 RVA: 0x00066340 File Offset: 0x00064540
	public static T Next<T>(this T src) where T : struct, Enum
	{
		T[] array = (T[])Enum.GetValues(typeof(T));
		int num = Array.IndexOf<T>(array, src) + 1;
		if (num != array.Length)
		{
			return array[num];
		}
		return array[0];
	}

	// Token: 0x06001825 RID: 6181 RVA: 0x00066384 File Offset: 0x00064584
	public static T Previous<T>(this T src) where T : struct, Enum
	{
		T[] array = (T[])Enum.GetValues(typeof(T));
		int num = Array.IndexOf<T>(array, src) - 1;
		if (num >= 0)
		{
			return array[num];
		}
		return array[array.Length - 1];
	}

	// Token: 0x06001826 RID: 6182 RVA: 0x000663C7 File Offset: 0x000645C7
	public static float ClampAngleTo90(float angle)
	{
		angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
		return Mathf.Clamp(angle, -90f, 90f);
	}

	// Token: 0x06001827 RID: 6183 RVA: 0x000663F2 File Offset: 0x000645F2
	public static long RoundByFirstNDigits(long value, int firstDigits)
	{
		return (long)FathF.RoundByFirstNDigits((double)value, firstDigits);
	}

	// Token: 0x06001828 RID: 6184 RVA: 0x00066400 File Offset: 0x00064600
	public static double RoundByFirstNDigits(double value, int firstDigits)
	{
		if (value <= 0.0)
		{
			return value;
		}
		int num = (int)Math.Floor(Math.Log10(Math.Abs(value))) + 1;
		if (firstDigits >= num)
		{
			return value;
		}
		int num2 = num - firstDigits;
		double num3 = Math.Pow(10.0, (double)num2);
		return Math.Round(value / num3, MidpointRounding.AwayFromZero) * num3;
	}

	// Token: 0x06001829 RID: 6185 RVA: 0x00066458 File Offset: 0x00064658
	public static List<int> GetUniqueRandomNumbers(int count, int min, int max, bool shuffle = false)
	{
		if (min > max)
		{
			Debug.LogError("Min cannot be greater than Max.");
			return null;
		}
		int num = max - min + 1;
		if (count > num)
		{
			Debug.LogError(string.Format("Count cannot exceed the range size ({0}).", num));
			return null;
		}
		List<int> list = new List<int>();
		for (int i = min; i <= max; i++)
		{
			list.Add(i);
		}
		if (shuffle)
		{
			for (int j = list.Count - 1; j > 0; j--)
			{
				int num2 = NetworkSingleton<SeededRandomManager>.Instance.Range(0, j + 1);
				List<int> list2 = list;
				int index = j;
				List<int> list3 = list;
				int index2 = num2;
				int value = list[num2];
				int value2 = list[j];
				list2[index] = value;
				list3[index2] = value2;
			}
		}
		return list.GetRange(0, count);
	}

	// Token: 0x0600182A RID: 6186 RVA: 0x0006651C File Offset: 0x0006471C
	public static void RotateArrayDown<T>(T[] array)
	{
		if (array == null || array.Length <= 1)
		{
			return;
		}
		T t = array[array.Length - 1];
		for (int i = array.Length - 1; i > 0; i--)
		{
			array[i] = array[i - 1];
		}
		array[0] = t;
	}

	// Token: 0x0600182B RID: 6187 RVA: 0x00066568 File Offset: 0x00064768
	public static Vector3 NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 pnt)
	{
		Vector3 vector = end - start;
		float magnitude = vector.magnitude;
		vector.Normalize();
		float num = Vector3.Dot(pnt - start, vector);
		num = Mathf.Clamp(num, 0f, magnitude);
		return start + vector * num;
	}

	// Token: 0x0600182C RID: 6188 RVA: 0x000665B4 File Offset: 0x000647B4
	public static Vector3 NearestPointToRayOnLine(Vector3 start, Vector3 end, Vector3 origin, Vector3 direction, out float t, out float s)
	{
		Vector3 vector = end - start;
		Vector3 rhs = start - origin;
		float num = Vector3.Dot(vector, vector);
		float num2 = Vector3.Dot(vector, direction);
		float num3 = Vector3.Dot(direction, direction);
		float num4 = Vector3.Dot(vector, rhs);
		float num5 = Vector3.Dot(direction, rhs);
		float num6 = num * num3 - num2 * num2;
		if (Mathf.Abs(num6) < 1E-06f)
		{
			t = 0f;
			s = -num5 / num3;
		}
		else
		{
			t = (num2 * num5 - num3 * num4) / num6;
			s = (num * num5 - num2 * num4) / num6;
		}
		t = Mathf.Clamp01(t);
		if (s < 0f)
		{
			s = 0f;
		}
		return start + vector * t;
	}

	// Token: 0x0600182D RID: 6189 RVA: 0x00066674 File Offset: 0x00064874
	public static void Teleport(this Rigidbody rb, Vector3 pos, bool resetVelocity = false)
	{
		RigidbodyInterpolation interpolation = rb.interpolation;
		rb.interpolation = RigidbodyInterpolation.None;
		rb.position = pos;
		if (!rb.isKinematic && resetVelocity)
		{
			rb.linearVelocity = Vector3.zero;
		}
		rb.interpolation = interpolation;
	}

	// Token: 0x0600182E RID: 6190 RVA: 0x000666B8 File Offset: 0x000648B8
	public static void Rotate(this Rigidbody rb, Quaternion rot, bool resetVelocity = false)
	{
		RigidbodyInterpolation interpolation = rb.interpolation;
		rb.interpolation = RigidbodyInterpolation.None;
		rb.rotation = rot;
		if (!rb.isKinematic && resetVelocity)
		{
			rb.angularVelocity = Vector3.zero;
		}
		rb.interpolation = interpolation;
	}

	// Token: 0x0600182F RID: 6191 RVA: 0x000666FC File Offset: 0x000648FC
	public static Color PastelizeColor(this Color color, float saturation = 0.7f, float valueBoost = 0.2f)
	{
		float h;
		float num;
		float num2;
		Color.RGBToHSV(color, out h, out num, out num2);
		num *= saturation;
		num2 = Mathf.Lerp(num2, 1f, valueBoost);
		Color result = Color.HSVToRGB(h, num, num2);
		result.a = color.a;
		return result;
	}

	// Token: 0x06001830 RID: 6192 RVA: 0x00066740 File Offset: 0x00064940
	public static Quaternion LookRotationUpPriority(Vector3 forward, Vector3 up)
	{
		if (up.sqrMagnitude < Mathf.Epsilon)
		{
			return Quaternion.identity;
		}
		Vector3 normalized = up.normalized;
		Vector3 vector = Vector3.ProjectOnPlane(forward, normalized);
		if (vector.sqrMagnitude < Mathf.Epsilon)
		{
			vector = Vector3.Cross(normalized, Vector3.right);
			if (vector.sqrMagnitude < Mathf.Epsilon)
			{
				vector = Vector3.Cross(normalized, Vector3.forward);
			}
		}
		vector.Normalize();
		Vector3.Cross(normalized, vector);
		return Quaternion.LookRotation(vector, normalized);
	}
}
