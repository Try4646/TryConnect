using System;
using Extensions;
using UnityEngine;

// Token: 0x020001B1 RID: 433
public class SeededRandomManager : NetworkSingleton<SeededRandomManager>
{
	// Token: 0x17000161 RID: 353
	// (get) Token: 0x06000FAC RID: 4012 RVA: 0x00042F88 File Offset: 0x00041188
	public int MysteryBoxCounter
	{
		get
		{
			this._mysteryBoxCounter++;
			return this._mysteryBoxCounter;
		}
	}

	// Token: 0x17000162 RID: 354
	// (get) Token: 0x06000FAD RID: 4013 RVA: 0x00042F9E File Offset: 0x0004119E
	public int AngelsReelCounter
	{
		get
		{
			this._angelsReelCounter++;
			return this._angelsReelCounter;
		}
	}

	// Token: 0x17000163 RID: 355
	// (get) Token: 0x06000FAE RID: 4014 RVA: 0x00042FB4 File Offset: 0x000411B4
	public int DevilsReelCounter
	{
		get
		{
			this._devilsReelCounter++;
			return this._devilsReelCounter;
		}
	}

	// Token: 0x17000164 RID: 356
	// (get) Token: 0x06000FAF RID: 4015 RVA: 0x00042FCA File Offset: 0x000411CA
	public int CurrentSeed
	{
		get
		{
			return this._currentSeed;
		}
	}

	// Token: 0x06000FB0 RID: 4016 RVA: 0x00042FD2 File Offset: 0x000411D2
	protected override void OnAwake()
	{
		base.OnAwake();
	}

	// Token: 0x06000FB1 RID: 4017 RVA: 0x00042FDA File Offset: 0x000411DA
	public void InitializeSeed(int seed)
	{
		this._currentSeed = seed;
		this._random = new Random(seed);
		Debug.Log(string.Format("[SeededRandomManager] Initialized with seed: {0}", seed));
	}

	// Token: 0x06000FB2 RID: 4018 RVA: 0x00043004 File Offset: 0x00041204
	public int Range(int min, int max)
	{
		if (this._random == null)
		{
			Debug.LogError("[SeededRandomManager] Random not initialized! Seed should be loaded from save file. Using Unity Random as fallback.");
			return Random.Range(min, max);
		}
		if (min >= max)
		{
			return min;
		}
		return this._random.Next(min, max);
	}

	// Token: 0x06000FB3 RID: 4019 RVA: 0x00043034 File Offset: 0x00041234
	public float Range(float min, float max)
	{
		if (this._random == null)
		{
			Debug.LogError("[SeededRandomManager] Random not initialized! Seed should be loaded from save file. Using Unity Random as fallback.");
			return Random.Range(min, max);
		}
		if (min >= max)
		{
			return min;
		}
		double num = (double)(max - min);
		double num2 = this._random.NextDouble();
		return (float)((double)min + num2 * num);
	}

	// Token: 0x17000165 RID: 357
	// (get) Token: 0x06000FB4 RID: 4020 RVA: 0x00043079 File Offset: 0x00041279
	public float value
	{
		get
		{
			if (this._random == null)
			{
				Debug.LogError("[SeededRandomManager] Random not initialized! Seed should be loaded from save file. Using Unity Random as fallback.");
				return Random.value;
			}
			return (float)this._random.NextDouble();
		}
	}

	// Token: 0x17000166 RID: 358
	// (get) Token: 0x06000FB5 RID: 4021 RVA: 0x000430A0 File Offset: 0x000412A0
	public Vector2 insideUnitCircle
	{
		get
		{
			float f = this.Range(0f, 6.2831855f);
			float num = Mathf.Sqrt(this.value);
			return new Vector2(num * Mathf.Cos(f), num * Mathf.Sin(f));
		}
	}

	// Token: 0x17000167 RID: 359
	// (get) Token: 0x06000FB6 RID: 4022 RVA: 0x000430E0 File Offset: 0x000412E0
	public Vector3 insideUnitSphere
	{
		get
		{
			float value = this.value;
			float value2 = this.value;
			float f = 6.2831855f * value;
			float f2 = Mathf.Acos(2f * value2 - 1f);
			float num = Mathf.Pow(this.value, 0.33333334f);
			return new Vector3(num * Mathf.Sin(f2) * Mathf.Cos(f), num * Mathf.Sin(f2) * Mathf.Sin(f), num * Mathf.Cos(f2));
		}
	}

	// Token: 0x17000168 RID: 360
	// (get) Token: 0x06000FB7 RID: 4023 RVA: 0x00043156 File Offset: 0x00041356
	public Quaternion rotation
	{
		get
		{
			return Quaternion.Euler(this.Range(0f, 360f), this.Range(0f, 360f), this.Range(0f, 360f));
		}
	}

	// Token: 0x17000169 RID: 361
	// (get) Token: 0x06000FB8 RID: 4024 RVA: 0x0004318D File Offset: 0x0004138D
	public Color color
	{
		get
		{
			return new Color(this.value, this.value, this.value, 1f);
		}
	}

	// Token: 0x1700016A RID: 362
	// (get) Token: 0x06000FB9 RID: 4025 RVA: 0x000431AB File Offset: 0x000413AB
	public Color colorWithAlpha
	{
		get
		{
			return new Color(this.value, this.value, this.value, this.value);
		}
	}

	// Token: 0x06000FBA RID: 4026 RVA: 0x000431CA File Offset: 0x000413CA
	public Random GetRandomInstance()
	{
		if (this._random == null)
		{
			Debug.LogWarning("[SeededRandomManager] Random not initialized, creating new instance");
			this.InitializeSeed(Random.Range(int.MinValue, int.MaxValue));
		}
		return this._random;
	}

	// Token: 0x06000FBB RID: 4027 RVA: 0x000431F9 File Offset: 0x000413F9
	private int GetContextualSeed(int context)
	{
		return this._currentSeed * 31 + context;
	}

	// Token: 0x06000FBC RID: 4028 RVA: 0x00043206 File Offset: 0x00041406
	public int RangeWithContext(int min, int max, int context)
	{
		if (this._random == null)
		{
			Debug.LogError("[SeededRandomManager] Random not initialized! Seed should be loaded from save file. Using Unity Random as fallback.");
			return Random.Range(min, max);
		}
		if (min >= max)
		{
			return min;
		}
		return new Random(this.GetContextualSeed(context)).Next(min, max);
	}

	// Token: 0x06000FBD RID: 4029 RVA: 0x0004323C File Offset: 0x0004143C
	public float RangeWithContext(float min, float max, int context)
	{
		if (this._random == null)
		{
			Debug.LogError("[SeededRandomManager] Random not initialized! Seed should be loaded from save file. Using Unity Random as fallback.");
			return Random.Range(min, max);
		}
		if (min >= max)
		{
			return min;
		}
		Random random = new Random(this.GetContextualSeed(context));
		double num = (double)(max - min);
		double num2 = random.NextDouble();
		return (float)((double)min + num2 * num);
	}

	// Token: 0x06000FBE RID: 4030 RVA: 0x00043287 File Offset: 0x00041487
	public float ValueWithContext(int context)
	{
		if (this._random == null)
		{
			Debug.LogError("[SeededRandomManager] Random not initialized! Seed should be loaded from save file. Using Unity Random as fallback.");
			return Random.value;
		}
		return (float)new Random(this.GetContextualSeed(context)).NextDouble();
	}

	// Token: 0x06000FBF RID: 4031 RVA: 0x000432B3 File Offset: 0x000414B3
	public Random GetContextualRandomInstance(int context)
	{
		if (this._random == null)
		{
			Debug.LogError("[SeededRandomManager] Random not initialized! Seed should be loaded from save file. Cannot create contextual random.");
			return new Random(Random.Range(int.MinValue, int.MaxValue));
		}
		return new Random(this.GetContextualSeed(context));
	}

	// Token: 0x06000FC1 RID: 4033 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000A36 RID: 2614
	private Random _random;

	// Token: 0x04000A37 RID: 2615
	private int _currentSeed;

	// Token: 0x04000A38 RID: 2616
	private int _mysteryBoxCounter;

	// Token: 0x04000A39 RID: 2617
	private int _angelsReelCounter;

	// Token: 0x04000A3A RID: 2618
	private int _devilsReelCounter;
}
