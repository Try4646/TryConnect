using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using Shapes;
using TMPro;
using UnityEngine;

// Token: 0x02000043 RID: 67
public class Crash : GameBase
{
	// Token: 0x0600018F RID: 399 RVA: 0x000095BC File Offset: 0x000077BC
	private void OnMultiplierChanged(float oldValue, float newValue)
	{
		this.multiplierText.text = newValue.ToString("0.00") + "x";
		long num = (long)Math.Round((double)((float)this.currentBet * newValue));
		this.potentialWinningText.text = "$" + num.ToString("N0");
		if (this._hasStarted)
		{
			this.currentMultiplierValue = newValue;
			this.UpdateExponentialCurve();
		}
	}

	// Token: 0x06000190 RID: 400 RVA: 0x00009634 File Offset: 0x00007834
	private void OnHasStartedChanged(bool oldValue, bool newValue)
	{
		if (newValue && !oldValue && base.isClient)
		{
			this.InitializeLine();
			this.gameStartTime = Time.time;
			this.currentMultiplierValue = 1f;
			if (this.curveUpdateCoroutine != null)
			{
				base.StopCoroutine(this.curveUpdateCoroutine);
			}
			this.curveUpdateCoroutine = base.StartCoroutine(this.ContinuousCurveUpdate());
		}
	}

	// Token: 0x06000191 RID: 401 RVA: 0x00009691 File Offset: 0x00007891
	private void OnHasCrashedChanged(bool oldValue, bool newValue)
	{
		if (newValue && !oldValue)
		{
			this.UpdateExponentialCurve();
		}
	}

	// Token: 0x06000192 RID: 402 RVA: 0x0000969F File Offset: 0x0000789F
	protected override void StartGame()
	{
		base.StartGame();
		this.InitializeLine();
		base.StartCoroutine(this.StartCountDown());
	}

	// Token: 0x06000193 RID: 403 RVA: 0x000096BA File Offset: 0x000078BA
	private IEnumerator StartCountDown()
	{
		this.RpcShowCountdown();
		float startTime = Time.time;
		while (Time.time - startTime < 1f)
		{
			string value = (3f - (Time.time - startTime) * 3f).ToString("0");
			this.RpcUpdateCountdownText(value);
			this.RpcPlayMysteriousTickingNoise();
			yield return new WaitForSeconds(0.34f);
		}
		this.RpcHideCountdown();
		this.SetCrashPoint();
		yield break;
	}

	// Token: 0x06000194 RID: 404 RVA: 0x000096CC File Offset: 0x000078CC
	private void SetCrashPoint()
	{
		Random seededRandom = base.GetSeededRandom(0);
		float num = (float)seededRandom.NextDouble();
		float crashPoint = 1.01f;
		if (num > this.instantCrashChance)
		{
			crashPoint = this.GetRandomCrashPoint((float)seededRandom.NextDouble());
		}
		base.StartCoroutine(this.RaiseRoutine(crashPoint));
		this.Network_hasStarted = true;
	}

	// Token: 0x06000195 RID: 405 RVA: 0x00009719 File Offset: 0x00007919
	private IEnumerator RaiseRoutine(float crashPoint)
	{
		float t = 0f;
		this.Network_multiplier = 1f;
		this.gameStartTime = Time.time;
		this.RpcStartRiseLoop();
		while (this._multiplier < crashPoint)
		{
			t += Time.deltaTime;
			if (this._hasCrashed)
			{
				t += Time.deltaTime * 4f;
			}
			this.Network_multiplier = Mathf.Exp(this.raiseSpeed * t);
			yield return null;
		}
		this.currentMultiplierValue = crashPoint;
		this.RpcStopRiseLoop();
		this.RpcPlayCrashOverSFX();
		this.Network_multiplier = crashPoint;
		this.CrashOnPoint();
		yield break;
	}

	// Token: 0x06000196 RID: 406 RVA: 0x0000972F File Offset: 0x0000792F
	private void CrashOnPoint()
	{
		if (!this._hasEnded)
		{
			this.Payout(0.0, ChangeType.GameResult, null, -1L);
		}
		this._hasEnded = true;
		this.Network_hasCrashed = true;
		this.RpcSetCrashColors();
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000197 RID: 407 RVA: 0x00009770 File Offset: 0x00007970
	[Server]
	public void Cashout()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Crash::Cashout()' called when server was not active");
			return;
		}
		if (!this.isPlaying)
		{
			return;
		}
		if (!this._hasStarted)
		{
			return;
		}
		if (this._hasEnded)
		{
			return;
		}
		this._hasEnded = true;
		this.RpcCashout(this._multiplier.ToString("0.00") + "x $" + ((long)Math.Round((double)this.currentBet * (double)this._multiplier)).ToString());
		this.Payout((double)this._multiplier, ChangeType.GameResult, null, -1L);
	}

	// Token: 0x06000198 RID: 408 RVA: 0x00009804 File Offset: 0x00007A04
	[ClientRpc]
	private void RpcCashout(string text)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(text);
		this.SendRPCInternal("System.Void Crash::RpcCashout(System.String)", 812465518, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000199 RID: 409 RVA: 0x0000983E File Offset: 0x00007A3E
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x0600019A RID: 410 RVA: 0x0000984D File Offset: 0x00007A4D
	protected override void ResetGame()
	{
		this.Network_multiplier = 0f;
		this.Network_hasStarted = false;
		this._hasEnded = false;
		this.Network_hasCrashed = false;
		this.RpcCashout("");
		this.ClearLine();
		base.ResetGame();
	}

	// Token: 0x0600019B RID: 411 RVA: 0x00009888 File Offset: 0x00007A88
	[ClientRpc]
	private void RpcShowCountdown()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Crash::RpcShowCountdown()", 939937333, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600019C RID: 412 RVA: 0x000098B8 File Offset: 0x00007AB8
	[ClientRpc]
	private void RpcHideCountdown()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Crash::RpcHideCountdown()", -922614198, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600019D RID: 413 RVA: 0x000098E8 File Offset: 0x00007AE8
	[ClientRpc]
	private void RpcUpdateCountdownText(string value)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(value);
		this.SendRPCInternal("System.Void Crash::RpcUpdateCountdownText(System.String)", 1974833200, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600019E RID: 414 RVA: 0x00009922 File Offset: 0x00007B22
	private float GetRandomCrashPoint(float r)
	{
		r = Mathf.Max(r, 0.001f);
		return Mathf.Clamp(1f / r, 1.001f, this.maxPoint);
	}

	// Token: 0x0600019F RID: 415 RVA: 0x00009948 File Offset: 0x00007B48
	[ClientRpc]
	private void RpcPlayMysteriousTickingNoise()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Crash::RpcPlayMysteriousTickingNoise()", -1397534610, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001A0 RID: 416 RVA: 0x00009978 File Offset: 0x00007B78
	[ClientRpc]
	private void RpcPlayCrashOverSFX()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Crash::RpcPlayCrashOverSFX()", 1728586995, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001A1 RID: 417 RVA: 0x000099A8 File Offset: 0x00007BA8
	[ClientRpc]
	private void RpcSetCrashColors()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Crash::RpcSetCrashColors()", 2139037266, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001A2 RID: 418 RVA: 0x000099D8 File Offset: 0x00007BD8
	[ClientRpc]
	private void RpcStartRiseLoop()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Crash::RpcStartRiseLoop()", 497445318, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001A3 RID: 419 RVA: 0x00009A08 File Offset: 0x00007C08
	[ClientRpc]
	private void RpcStopRiseLoop()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Crash::RpcStopRiseLoop()", -1665586290, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001A4 RID: 420 RVA: 0x00009A38 File Offset: 0x00007C38
	private IEnumerator ContinuousCurveUpdate()
	{
		while (this._hasStarted && !this._hasCrashed)
		{
			if (Mathf.Abs(this._multiplier - this.lastUpdateMultiplier) > 0.01f)
			{
				this.UpdateExponentialCurve();
				this.lastUpdateMultiplier = this._multiplier;
			}
			yield return new WaitForSeconds(0.05f);
		}
		if (this._hasCrashed)
		{
			this.UpdateExponentialCurve();
		}
		this.curveUpdateCoroutine = null;
		yield break;
	}

	// Token: 0x060001A5 RID: 421 RVA: 0x00009A47 File Offset: 0x00007C47
	private void Update()
	{
		if (base.isClient && this._hasStarted && !this._hasCrashed && this.curveUpdateCoroutine == null)
		{
			this.curveUpdateCoroutine = base.StartCoroutine(this.ContinuousCurveUpdate());
		}
	}

	// Token: 0x060001A6 RID: 422 RVA: 0x00009A7C File Offset: 0x00007C7C
	private void InitializeLine()
	{
		if (this.multiplierLine == null)
		{
			GameObject gameObject = new GameObject("MultiplierLine");
			Transform parent = (this.lineGraphParent != null) ? this.lineGraphParent : base.transform;
			gameObject.transform.SetParent(parent);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			this.multiplierLine = gameObject.AddComponent<Polyline>();
			this.multiplierLine.Thickness = this.lineThickness;
			this.multiplierLine.ThicknessSpace = ThicknessSpace.Meters;
			this.multiplierLine.Geometry = PolylineGeometry.Billboard;
			this.multiplierLine.Joins = PolylineJoins.Round;
			this.multiplierLine.BlendMode = ShapesBlendMode.Opaque;
			this.multiplierLine.Closed = false;
			this.multiplierLine.Color = this.lineColor;
			this.multiplierLine.gameObject.SetActive(false);
		}
	}

	// Token: 0x060001A7 RID: 423 RVA: 0x00009B7C File Offset: 0x00007D7C
	private void UpdateExponentialCurve()
	{
		if (this.multiplierLine == null)
		{
			return;
		}
		if (!this._hasStarted)
		{
			return;
		}
		float num = this._hasCrashed ? this.currentMultiplierValue : this._multiplier;
		if (num <= 0f)
		{
			num = 1f;
		}
		float num2 = Mathf.Clamp01(this.CalculateElapsedTimeFromMultiplier(num) / this.curveDrawDuration);
		if (num2 <= 0f)
		{
			if (this.multiplierLine != null)
			{
				this.multiplierLine.gameObject.SetActive(false);
			}
			if (this.multiplierText != null)
			{
				this.multiplierText.gameObject.SetActive(false);
			}
			return;
		}
		float a = 0f;
		float a2 = 0f;
		float b = this.timeAxisLength * (1f - this.endPointOffsetX);
		float b2 = this.multiplierAxisHeight * (1f - this.endPointOffsetY);
		float num3 = this.minCurvePower + (num - 1f) * this.curvePowerMultiplier;
		num3 = Mathf.Clamp(num3, this.minCurvePower, this.maxCurvePower);
		int num4 = Mathf.Clamp(Mathf.CeilToInt((float)this.curveResolution * num2), 2, 50);
		List<Vector3> list = new List<Vector3>(num4);
		Color color = this._hasCrashed ? Color.red : this.lineColor;
		for (int i = 0; i < num4; i++)
		{
			float num5 = (float)i / (float)(num4 - 1) * num2;
			float x = Mathf.Lerp(a, b, num5);
			float t = Mathf.Pow(num5, num3);
			float y = Mathf.Lerp(a2, b2, t);
			list.Add(new Vector3(x, y, 0f));
		}
		this.multiplierLine.SetPoints(list, null);
		this.multiplierLine.Color = color;
		this.multiplierLine.gameObject.SetActive(true);
		if (this.rocketImage != null && list.Count >= 2)
		{
			Transform transform = (this.lineGraphParent != null) ? this.lineGraphParent : base.transform;
			Transform transform2 = this.rocketImage;
			Transform transform3 = transform;
			List<Vector3> list2 = list;
			transform2.position = transform3.TransformPoint(list2[list2.Count - 1]);
			List<Vector3> list3 = list;
			Vector3 a3 = list3[list3.Count - 1];
			List<Vector3> list4 = list;
			Vector3 b3 = list4[list4.Count - 2];
			Vector3 vector = a3 - b3;
			float num6 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			this.rocketImage.localRotation = Quaternion.Euler(0f, 0f, -num6 + 90f);
			this.rocketImage.gameObject.SetActive(true);
		}
		this.multiplierText.gameObject.SetActive(true);
		this.potentialWinningText.gameObject.SetActive(true);
		if (this.showBorders && !this.bordersInitialized)
		{
			this.UpdateBorders();
			this.bordersInitialized = true;
		}
	}

	// Token: 0x060001A8 RID: 424 RVA: 0x00009E54 File Offset: 0x00008054
	private void ClearLine()
	{
		if (this.curveUpdateCoroutine != null)
		{
			base.StopCoroutine(this.curveUpdateCoroutine);
			this.curveUpdateCoroutine = null;
		}
		if (this.multiplierLine != null)
		{
			this.multiplierLine.gameObject.SetActive(false);
		}
		if (this.multiplierText != null)
		{
			this.multiplierText.gameObject.SetActive(false);
		}
		this.potentialWinningText.gameObject.SetActive(false);
		this.rocketImage.gameObject.SetActive(false);
		this.currentMultiplierValue = 1f;
		this.lastUpdateMultiplier = 0f;
		this.bordersInitialized = false;
		this.DestroyBorders();
	}

	// Token: 0x060001A9 RID: 425 RVA: 0x00009F00 File Offset: 0x00008100
	private void UpdateBorders()
	{
		if (this.bordersInitialized)
		{
			return;
		}
		Transform parent = (this.lineGraphParent != null) ? this.lineGraphParent : base.transform;
		if (this.xAxisBorder == null)
		{
			GameObject gameObject = new GameObject("XAxisBorder");
			gameObject.transform.SetParent(parent);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			this.xAxisBorder = gameObject.AddComponent<Polyline>();
			this.xAxisBorder.SetPoints(new List<Vector3>
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(this.timeAxisLength, 0f, 0f)
			}, null);
			this.xAxisBorder.Thickness = this.borderThickness;
			this.xAxisBorder.ThicknessSpace = ThicknessSpace.Meters;
			this.xAxisBorder.Geometry = PolylineGeometry.Billboard;
			this.xAxisBorder.Joins = PolylineJoins.Round;
			this.xAxisBorder.BlendMode = ShapesBlendMode.Opaque;
			this.xAxisBorder.Closed = false;
			this.xAxisBorder.Color = this.borderColor;
			this.xAxisBorder.gameObject.SetActive(true);
		}
		if (this.yAxisBorder == null)
		{
			GameObject gameObject2 = new GameObject("YAxisBorder");
			gameObject2.transform.SetParent(parent);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			this.yAxisBorder = gameObject2.AddComponent<Polyline>();
			this.yAxisBorder.SetPoints(new List<Vector3>
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, this.multiplierAxisHeight, 0f)
			}, null);
			this.yAxisBorder.Thickness = this.borderThickness;
			this.yAxisBorder.ThicknessSpace = ThicknessSpace.Meters;
			this.yAxisBorder.Geometry = PolylineGeometry.Billboard;
			this.yAxisBorder.Joins = PolylineJoins.Round;
			this.yAxisBorder.BlendMode = ShapesBlendMode.Opaque;
			this.yAxisBorder.Closed = false;
			this.yAxisBorder.Color = this.borderColor;
			this.yAxisBorder.gameObject.SetActive(true);
		}
		if (this.topBorder == null)
		{
			GameObject gameObject3 = new GameObject("TopBorder");
			gameObject3.transform.SetParent(parent);
			gameObject3.transform.localPosition = Vector3.zero;
			gameObject3.transform.localRotation = Quaternion.identity;
			gameObject3.transform.localScale = Vector3.one;
			this.topBorder = gameObject3.AddComponent<Polyline>();
			this.topBorder.SetPoints(new List<Vector3>
			{
				new Vector3(0f, this.multiplierAxisHeight, 0f),
				new Vector3(this.timeAxisLength, this.multiplierAxisHeight, 0f)
			}, null);
			this.topBorder.Thickness = this.borderThickness;
			this.topBorder.ThicknessSpace = ThicknessSpace.Meters;
			this.topBorder.Geometry = PolylineGeometry.Billboard;
			this.topBorder.Joins = PolylineJoins.Round;
			this.topBorder.BlendMode = ShapesBlendMode.Opaque;
			this.topBorder.Closed = false;
			this.topBorder.Color = this.borderColor;
			this.topBorder.gameObject.SetActive(true);
		}
		if (this.rightBorder == null)
		{
			GameObject gameObject4 = new GameObject("RightBorder");
			gameObject4.transform.SetParent(parent);
			gameObject4.transform.localPosition = Vector3.zero;
			gameObject4.transform.localRotation = Quaternion.identity;
			gameObject4.transform.localScale = Vector3.one;
			this.rightBorder = gameObject4.AddComponent<Polyline>();
			this.rightBorder.SetPoints(new List<Vector3>
			{
				new Vector3(this.timeAxisLength, 0f, 0f),
				new Vector3(this.timeAxisLength, this.multiplierAxisHeight, 0f)
			}, null);
			this.rightBorder.Thickness = this.borderThickness;
			this.rightBorder.ThicknessSpace = ThicknessSpace.Meters;
			this.rightBorder.Geometry = PolylineGeometry.Billboard;
			this.rightBorder.Joins = PolylineJoins.Round;
			this.rightBorder.BlendMode = ShapesBlendMode.Opaque;
			this.rightBorder.Closed = false;
			this.rightBorder.Color = this.borderColor;
			this.rightBorder.gameObject.SetActive(true);
		}
	}

	// Token: 0x060001AA RID: 426 RVA: 0x0000A3A4 File Offset: 0x000085A4
	private void DestroyBorders()
	{
		if (this.xAxisBorder != null && this.xAxisBorder.gameObject != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.xAxisBorder.gameObject);
			}
			else
			{
				Object.DestroyImmediate(this.xAxisBorder.gameObject);
			}
			this.xAxisBorder = null;
		}
		if (this.yAxisBorder != null && this.yAxisBorder.gameObject != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.yAxisBorder.gameObject);
			}
			else
			{
				Object.DestroyImmediate(this.yAxisBorder.gameObject);
			}
			this.yAxisBorder = null;
		}
		if (this.topBorder != null && this.topBorder.gameObject != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.topBorder.gameObject);
			}
			else
			{
				Object.DestroyImmediate(this.topBorder.gameObject);
			}
			this.topBorder = null;
		}
		if (this.rightBorder != null && this.rightBorder.gameObject != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(this.rightBorder.gameObject);
			}
			else
			{
				Object.DestroyImmediate(this.rightBorder.gameObject);
			}
			this.rightBorder = null;
		}
	}

	// Token: 0x060001AB RID: 427 RVA: 0x0000A4F5 File Offset: 0x000086F5
	private float CalculateElapsedTimeFromMultiplier(float multiplier)
	{
		if (multiplier <= 1f)
		{
			return 0f;
		}
		return Mathf.Log(multiplier) / this.raiseSpeed;
	}

	// Token: 0x060001AC RID: 428 RVA: 0x0000A514 File Offset: 0x00008714
	public Crash()
	{
		this._Mirror_SyncVarHookDelegate__multiplier = new Action<float, float>(this.OnMultiplierChanged);
		this._Mirror_SyncVarHookDelegate__hasStarted = new Action<bool, bool>(this.OnHasStartedChanged);
		this._Mirror_SyncVarHookDelegate__hasCrashed = new Action<bool, bool>(this.OnHasCrashedChanged);
	}

	// Token: 0x060001AD RID: 429 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700002F RID: 47
	// (get) Token: 0x060001AE RID: 430 RVA: 0x0000A630 File Offset: 0x00008830
	// (set) Token: 0x060001AF RID: 431 RVA: 0x0000A643 File Offset: 0x00008843
	public float Network_multiplier
	{
		get
		{
			return this._multiplier;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this._multiplier, 8UL, this._Mirror_SyncVarHookDelegate__multiplier);
		}
	}

	// Token: 0x17000030 RID: 48
	// (get) Token: 0x060001B0 RID: 432 RVA: 0x0000A664 File Offset: 0x00008864
	// (set) Token: 0x060001B1 RID: 433 RVA: 0x0000A677 File Offset: 0x00008877
	public bool Network_hasStarted
	{
		get
		{
			return this._hasStarted;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this._hasStarted, 16UL, this._Mirror_SyncVarHookDelegate__hasStarted);
		}
	}

	// Token: 0x17000031 RID: 49
	// (get) Token: 0x060001B2 RID: 434 RVA: 0x0000A698 File Offset: 0x00008898
	// (set) Token: 0x060001B3 RID: 435 RVA: 0x0000A6AB File Offset: 0x000088AB
	public bool Network_hasCrashed
	{
		get
		{
			return this._hasCrashed;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this._hasCrashed, 32UL, this._Mirror_SyncVarHookDelegate__hasCrashed);
		}
	}

	// Token: 0x060001B4 RID: 436 RVA: 0x0000A6CA File Offset: 0x000088CA
	protected void UserCode_RpcCashout__String(string text)
	{
		this.cashoutText.text = text;
	}

	// Token: 0x060001B5 RID: 437 RVA: 0x0000A6D8 File Offset: 0x000088D8
	protected static void InvokeUserCode_RpcCashout__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCashout called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcCashout__String(reader.ReadString());
	}

	// Token: 0x060001B6 RID: 438 RVA: 0x0000A701 File Offset: 0x00008901
	protected void UserCode_RpcShowCountdown()
	{
		if (this.countdownText != null)
		{
			this.countdownText.gameObject.SetActive(true);
		}
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x0000A722 File Offset: 0x00008922
	protected static void InvokeUserCode_RpcShowCountdown(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowCountdown called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcShowCountdown();
	}

	// Token: 0x060001B8 RID: 440 RVA: 0x0000A745 File Offset: 0x00008945
	protected void UserCode_RpcHideCountdown()
	{
		if (this.countdownText != null)
		{
			this.countdownText.gameObject.SetActive(false);
		}
	}

	// Token: 0x060001B9 RID: 441 RVA: 0x0000A766 File Offset: 0x00008966
	protected static void InvokeUserCode_RpcHideCountdown(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHideCountdown called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcHideCountdown();
	}

	// Token: 0x060001BA RID: 442 RVA: 0x0000A789 File Offset: 0x00008989
	protected void UserCode_RpcUpdateCountdownText__String(string value)
	{
		if (this.countdownText != null)
		{
			this.countdownText.text = value;
		}
	}

	// Token: 0x060001BB RID: 443 RVA: 0x0000A7A5 File Offset: 0x000089A5
	protected static void InvokeUserCode_RpcUpdateCountdownText__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateCountdownText called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcUpdateCountdownText__String(reader.ReadString());
	}

	// Token: 0x060001BC RID: 444 RVA: 0x0000A7CE File Offset: 0x000089CE
	protected void UserCode_RpcPlayMysteriousTickingNoise()
	{
		SFXManager.SFXOneShot(this.sfxTick, base.transform.position);
	}

	// Token: 0x060001BD RID: 445 RVA: 0x0000A7E6 File Offset: 0x000089E6
	protected static void InvokeUserCode_RpcPlayMysteriousTickingNoise(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayMysteriousTickingNoise called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcPlayMysteriousTickingNoise();
	}

	// Token: 0x060001BE RID: 446 RVA: 0x0000A809 File Offset: 0x00008A09
	protected void UserCode_RpcPlayCrashOverSFX()
	{
		SFXManager.SFXOneShot(this.sfxCrashOver, base.transform.position);
	}

	// Token: 0x060001BF RID: 447 RVA: 0x0000A821 File Offset: 0x00008A21
	protected static void InvokeUserCode_RpcPlayCrashOverSFX(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayCrashOverSFX called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcPlayCrashOverSFX();
	}

	// Token: 0x060001C0 RID: 448 RVA: 0x0000A844 File Offset: 0x00008A44
	protected void UserCode_RpcSetCrashColors()
	{
		if (this.multiplierText != null)
		{
			this.multiplierText.color = Color.red;
		}
		if (this.multiplierLine != null)
		{
			this.multiplierLine.Color = Color.red;
		}
	}

	// Token: 0x060001C1 RID: 449 RVA: 0x0000A882 File Offset: 0x00008A82
	protected static void InvokeUserCode_RpcSetCrashColors(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCrashColors called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcSetCrashColors();
	}

	// Token: 0x060001C2 RID: 450 RVA: 0x0000A8A8 File Offset: 0x00008AA8
	protected void UserCode_RpcStartRiseLoop()
	{
		this.InitializeLine();
		if (this.multiplierText != null)
		{
			this.multiplierText.color = Color.white;
		}
		if (this.multiplierLine != null)
		{
			this.multiplierLine.Color = this.lineColor;
		}
		this.sfxLoop.LoopSFX(true);
		if (this.showBorders)
		{
			this.UpdateBorders();
		}
	}

	// Token: 0x060001C3 RID: 451 RVA: 0x0000A912 File Offset: 0x00008B12
	protected static void InvokeUserCode_RpcStartRiseLoop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartRiseLoop called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcStartRiseLoop();
	}

	// Token: 0x060001C4 RID: 452 RVA: 0x0000A935 File Offset: 0x00008B35
	protected void UserCode_RpcStopRiseLoop()
	{
		this.sfxLoop.LoopSFX(false);
		if (this.curveUpdateCoroutine != null)
		{
			base.StopCoroutine(this.curveUpdateCoroutine);
			this.curveUpdateCoroutine = null;
		}
	}

	// Token: 0x060001C5 RID: 453 RVA: 0x0000A95E File Offset: 0x00008B5E
	protected static void InvokeUserCode_RpcStopRiseLoop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopRiseLoop called on server.");
			return;
		}
		((Crash)obj).UserCode_RpcStopRiseLoop();
	}

	// Token: 0x060001C6 RID: 454 RVA: 0x0000A984 File Offset: 0x00008B84
	static Crash()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcCashout(System.String)", new RemoteCallDelegate(Crash.InvokeUserCode_RpcCashout__String));
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcShowCountdown()", new RemoteCallDelegate(Crash.InvokeUserCode_RpcShowCountdown));
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcHideCountdown()", new RemoteCallDelegate(Crash.InvokeUserCode_RpcHideCountdown));
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcUpdateCountdownText(System.String)", new RemoteCallDelegate(Crash.InvokeUserCode_RpcUpdateCountdownText__String));
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcPlayMysteriousTickingNoise()", new RemoteCallDelegate(Crash.InvokeUserCode_RpcPlayMysteriousTickingNoise));
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcPlayCrashOverSFX()", new RemoteCallDelegate(Crash.InvokeUserCode_RpcPlayCrashOverSFX));
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcSetCrashColors()", new RemoteCallDelegate(Crash.InvokeUserCode_RpcSetCrashColors));
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcStartRiseLoop()", new RemoteCallDelegate(Crash.InvokeUserCode_RpcStartRiseLoop));
		RemoteProcedureCalls.RegisterRpc(typeof(Crash), "System.Void Crash::RpcStopRiseLoop()", new RemoteCallDelegate(Crash.InvokeUserCode_RpcStopRiseLoop));
	}

	// Token: 0x060001C7 RID: 455 RVA: 0x0000AAB4 File Offset: 0x00008CB4
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(this._multiplier);
			writer.WriteBool(this._hasStarted);
			writer.WriteBool(this._hasCrashed);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteFloat(this._multiplier);
		}
		if ((this.syncVarDirtyBits & 16UL) != 0UL)
		{
			writer.WriteBool(this._hasStarted);
		}
		if ((this.syncVarDirtyBits & 32UL) != 0UL)
		{
			writer.WriteBool(this._hasCrashed);
		}
	}

	// Token: 0x060001C8 RID: 456 RVA: 0x0000AB68 File Offset: 0x00008D68
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this._multiplier, this._Mirror_SyncVarHookDelegate__multiplier, reader.ReadFloat());
			base.GeneratedSyncVarDeserialize<bool>(ref this._hasStarted, this._Mirror_SyncVarHookDelegate__hasStarted, reader.ReadBool());
			base.GeneratedSyncVarDeserialize<bool>(ref this._hasCrashed, this._Mirror_SyncVarHookDelegate__hasCrashed, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this._multiplier, this._Mirror_SyncVarHookDelegate__multiplier, reader.ReadFloat());
		}
		if ((num & 16L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._hasStarted, this._Mirror_SyncVarHookDelegate__hasStarted, reader.ReadBool());
		}
		if ((num & 32L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._hasCrashed, this._Mirror_SyncVarHookDelegate__hasCrashed, reader.ReadBool());
		}
	}

	// Token: 0x04000157 RID: 343
	[Header("References")]
	[SerializeField]
	private Transform rocketImage;

	// Token: 0x04000158 RID: 344
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x04000159 RID: 345
	[SerializeField]
	private TextMeshPro potentialWinningText;

	// Token: 0x0400015A RID: 346
	[SerializeField]
	private TextMeshPro cashoutText;

	// Token: 0x0400015B RID: 347
	[SerializeField]
	private TextMeshPro countdownText;

	// Token: 0x0400015C RID: 348
	[Header("Settings")]
	[SerializeField]
	private float instantCrashChance = 0.1f;

	// Token: 0x0400015D RID: 349
	[SerializeField]
	private float maxPoint = 100f;

	// Token: 0x0400015E RID: 350
	[SerializeField]
	private float raiseSpeed = 0.15f;

	// Token: 0x0400015F RID: 351
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent sfxLoop;

	// Token: 0x04000160 RID: 352
	[SerializeField]
	private EventReference sfxCrashOver;

	// Token: 0x04000161 RID: 353
	[SerializeField]
	private EventReference sfxTick;

	// Token: 0x04000162 RID: 354
	[Header("Line Graph Settings")]
	[SerializeField]
	private float lineThickness = 0.05f;

	// Token: 0x04000163 RID: 355
	[SerializeField]
	private Color lineColor = Color.white;

	// Token: 0x04000164 RID: 356
	[SerializeField]
	private float timeAxisLength = 10f;

	// Token: 0x04000165 RID: 357
	[SerializeField]
	private float multiplierAxisHeight = 5f;

	// Token: 0x04000166 RID: 358
	[SerializeField]
	private int curveResolution = 100;

	// Token: 0x04000167 RID: 359
	[SerializeField]
	private Transform lineGraphParent;

	// Token: 0x04000168 RID: 360
	[Header("Curve Steepness Control")]
	[SerializeField]
	private float minCurvePower = 1f;

	// Token: 0x04000169 RID: 361
	[SerializeField]
	private float maxCurvePower = 5f;

	// Token: 0x0400016A RID: 362
	[SerializeField]
	private float curvePowerMultiplier = 0.1f;

	// Token: 0x0400016B RID: 363
	[SerializeField]
	private float endPointOffsetX = 0.1f;

	// Token: 0x0400016C RID: 364
	[SerializeField]
	private float endPointOffsetY = 0.1f;

	// Token: 0x0400016D RID: 365
	[SerializeField]
	private float curveDrawDuration = 10f;

	// Token: 0x0400016E RID: 366
	[SerializeField]
	private float multiplierTextOffsetX = 0.5f;

	// Token: 0x0400016F RID: 367
	[Header("Border Settings")]
	[SerializeField]
	private bool showBorders = true;

	// Token: 0x04000170 RID: 368
	[SerializeField]
	private float borderThickness = 0.02f;

	// Token: 0x04000171 RID: 369
	[SerializeField]
	private Color borderColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

	// Token: 0x04000172 RID: 370
	[SyncVar(hook = "OnMultiplierChanged")]
	private float _multiplier;

	// Token: 0x04000173 RID: 371
	[SyncVar(hook = "OnHasStartedChanged")]
	private bool _hasStarted;

	// Token: 0x04000174 RID: 372
	[SyncVar(hook = "OnHasCrashedChanged")]
	private bool _hasCrashed;

	// Token: 0x04000175 RID: 373
	private bool _hasEnded;

	// Token: 0x04000176 RID: 374
	private Polyline multiplierLine;

	// Token: 0x04000177 RID: 375
	private float gameStartTime;

	// Token: 0x04000178 RID: 376
	private float currentMultiplierValue;

	// Token: 0x04000179 RID: 377
	private Coroutine curveUpdateCoroutine;

	// Token: 0x0400017A RID: 378
	private float lastUpdateMultiplier;

	// Token: 0x0400017B RID: 379
	private const float UPDATE_THRESHOLD = 0.01f;

	// Token: 0x0400017C RID: 380
	private Polyline xAxisBorder;

	// Token: 0x0400017D RID: 381
	private Polyline yAxisBorder;

	// Token: 0x0400017E RID: 382
	private Polyline topBorder;

	// Token: 0x0400017F RID: 383
	private Polyline rightBorder;

	// Token: 0x04000180 RID: 384
	private bool bordersInitialized;

	// Token: 0x04000181 RID: 385
	public Action<float, float> _Mirror_SyncVarHookDelegate__multiplier;

	// Token: 0x04000182 RID: 386
	public Action<bool, bool> _Mirror_SyncVarHookDelegate__hasStarted;

	// Token: 0x04000183 RID: 387
	public Action<bool, bool> _Mirror_SyncVarHookDelegate__hasCrashed;
}
