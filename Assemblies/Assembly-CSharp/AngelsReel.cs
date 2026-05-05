using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x020000CA RID: 202
public class AngelsReel : ConsumableItem
{
	// Token: 0x060007CA RID: 1994 RVA: 0x0001F5CC File Offset: 0x0001D7CC
	protected override void SubscribeToEvents(bool isSubscribed)
	{
		base.SubscribeToEvents(isSubscribed);
		if (isSubscribed)
		{
			NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered += this.OnResultRegistered;
			return;
		}
		NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered -= this.OnResultRegistered;
	}

	// Token: 0x060007CB RID: 1995 RVA: 0x0001F608 File Offset: 0x0001D808
	private void OnResultRegistered(long bet, long payout, PlayerProfile playerProfile, CasinoGameType gameType, Vector3 position, bool hadTipsyFortune, bool hadInspiringMelody, bool hadImmunity, Dictionary<string, object> gameSpecificData)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!base.NetworkHolder)
		{
			return;
		}
		if (this._holderProfile != playerProfile)
		{
			return;
		}
		PlayerResultData playerResultData;
		if (NetworkSingleton<GameResultsManager>.Instance.lastResults.TryGetValue(this._holderProfile, out playerResultData))
		{
			this.Network_lastProfit = playerResultData.NetProfit;
			string str = MoneyFormatter.FormatWithDollar((long)((double)(-(double)this._lastProfit) * (double)NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._holderProfile.steamId, PlayerUpgradeType.Stakeholder)));
			if (this._lastProfit < 0L)
			{
				this.RpcSetText("0 | +" + str, false);
				return;
			}
			this.RpcSetText("-", false);
		}
	}

	// Token: 0x060007CC RID: 1996 RVA: 0x0001F6B0 File Offset: 0x0001D8B0
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		if (this._isSpinning)
		{
			return;
		}
		this._holderProfile = playerInventory.GetComponent<PlayerProfile>();
		PlayerResultData playerResultData;
		if (NetworkSingleton<GameResultsManager>.Instance.lastResults.TryGetValue(this._holderProfile, out playerResultData))
		{
			this.Network_lastProfit = playerResultData.NetProfit;
			string str = MoneyFormatter.FormatWithDollar((long)((double)(-(double)this._lastProfit) * (double)NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._holderProfile.steamId, PlayerUpgradeType.Stakeholder)));
			if (this._lastProfit < 0L)
			{
				this.RpcSetText("$0 | +" + str, false);
				return;
			}
			this.RpcSetText("-", false);
		}
	}

	// Token: 0x060007CD RID: 1997 RVA: 0x0001F74E File Offset: 0x0001D94E
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		if (this._isSpinning)
		{
			return;
		}
		this._holderProfile = null;
		this.Network_lastProfit = 0L;
		this.RpcSetText("-", false);
	}

	// Token: 0x060007CE RID: 1998 RVA: 0x0001F77C File Offset: 0x0001D97C
	protected override void OnUseItem(bool isPressed)
	{
		base.OnUseItem(isPressed);
		if (!isPressed)
		{
			return;
		}
		if (this._isSpinning)
		{
			return;
		}
		if (this._lastProfit >= 0L)
		{
			this.PlayUnableToUseFeedback();
			return;
		}
		this._isSpinning = true;
		if (base.isServer)
		{
			base.StartCoroutine(this.SpinRoutine());
		}
	}

	// Token: 0x060007CF RID: 1999 RVA: 0x0001F7CA File Offset: 0x0001D9CA
	private IEnumerator SpinRoutine()
	{
		this.anim.SetTrigger("Spin");
		yield return new WaitForSeconds(1f);
		this.spinSfx.RpcPlayOneShotAttached();
		float totalDuration = 3f;
		float minInterval = 0.1f;
		float maxInterval = 0.35f;
		PlayerProfile holder = this._holderProfile;
		long win = (long)((double)(-(double)this._lastProfit) * (double)NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(holder.steamId, PlayerUpgradeType.Stakeholder));
		float interval;
		for (float elapsed = 0f; elapsed < totalDuration; elapsed += interval)
		{
			float num = elapsed / totalDuration;
			float t = 1f - Mathf.Pow(1f - num, 3f);
			interval = Mathf.Lerp(minInterval, maxInterval, t);
			bool flag = Random.value > 0.5f;
			long amount = flag ? win : 0L;
			this.RpcSetText(MoneyFormatter.FormatWithDollar(amount), true);
			if (flag)
			{
				this.RpcPlayVFX(true, false);
			}
			else
			{
				this.RpcPlayVFX(false, false);
			}
			this.numChangeSfx.RpcPlayOneShotAttached();
			yield return new WaitForSeconds(interval);
		}
		bool flag2 = (float)this.GetSeededRandom().NextDouble() < this.chanceOfWin;
		long finalValue = flag2 ? win : 0L;
		this.RpcSetText(MoneyFormatter.FormatWithDollar(finalValue), true);
		if (flag2)
		{
			this.winSfx.RpcPlayOneShotAttached();
			this.RpcPlayVFX(true, true);
		}
		else
		{
			this.loseSfx.RpcPlayOneShotAttached();
			this.RpcPlayVFX(false, true);
		}
		yield return new WaitForSeconds(1f);
		NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(finalValue, holder, ChangeType.Item);
		this.destroySfx.RpcPlayOneShotWith3DPos();
		base.DestroyItem();
		yield break;
	}

	// Token: 0x060007D0 RID: 2000 RVA: 0x0001F7DC File Offset: 0x0001D9DC
	[ClientRpc]
	private void RpcSetText(string text, bool punch)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(text);
		writer.WriteBool(punch);
		this.SendRPCInternal("System.Void AngelsReel::RpcSetText(System.String,System.Boolean)", 664303138, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007D1 RID: 2001 RVA: 0x0001F820 File Offset: 0x0001DA20
	[ClientRpc]
	private void RpcPlayVFX(bool isWin, bool isEnd)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isWin);
		writer.WriteBool(isEnd);
		this.SendRPCInternal("System.Void AngelsReel::RpcPlayVFX(System.Boolean,System.Boolean)", -1906851754, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007D2 RID: 2002 RVA: 0x0001F864 File Offset: 0x0001DA64
	private void PlayUnableToUseFeedback()
	{
		this.invalidSfx.PlayOneShotAttached();
		this.screenText.transform.DOPunchScale(this.screenText.transform.localScale * 0.5f, 0.1f, 1, 1f);
	}

	// Token: 0x060007D3 RID: 2003 RVA: 0x0001F8B4 File Offset: 0x0001DAB4
	private Random GetSeededRandom()
	{
		if (!NetworkSingleton<SeededRandomManager>.Instance || !NetworkSingleton<GameManager>.Instance)
		{
			return new Random(Random.Range(int.MinValue, int.MaxValue));
		}
		long currentSeed = (long)NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
		int daysPassed = NetworkSingleton<GameManager>.Instance.daysPassed;
		int angelsReelCounter = NetworkSingleton<SeededRandomManager>.Instance.AngelsReelCounter;
		long num = ((currentSeed * (long)((ulong)-1640531535) + (long)daysPassed) * (long)((ulong)-1640531535) + (long)angelsReelCounter) * (long)((ulong)-1640531535) ^ (long)((long)angelsReelCounter << 13) ^ (long)(angelsReelCounter >> 7);
		long num2 = (num ^ num >> 32) * (long)((ulong)-2048144789);
		long num3 = (num2 ^ num2 >> 16) * (long)((ulong)-1028477379);
		return new Random((int)(num3 ^ num3 >> 13));
	}

	// Token: 0x060007D5 RID: 2005 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000B7 RID: 183
	// (get) Token: 0x060007D6 RID: 2006 RVA: 0x0001F960 File Offset: 0x0001DB60
	// (set) Token: 0x060007D7 RID: 2007 RVA: 0x0001F973 File Offset: 0x0001DB73
	public long Network_lastProfit
	{
		get
		{
			return this._lastProfit;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this._lastProfit, 2UL, null);
		}
	}

	// Token: 0x060007D8 RID: 2008 RVA: 0x0001F990 File Offset: 0x0001DB90
	protected void UserCode_RpcSetText__String__Boolean(string text, bool punch)
	{
		this.screenText.text = text;
		if (punch)
		{
			this.screenText.transform.DOPunchScale(this.screenText.transform.localScale * 0.2f, 0.1f, 1, 0f);
		}
	}

	// Token: 0x060007D9 RID: 2009 RVA: 0x0001F9E2 File Offset: 0x0001DBE2
	protected static void InvokeUserCode_RpcSetText__String__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetText called on server.");
			return;
		}
		((AngelsReel)obj).UserCode_RpcSetText__String__Boolean(reader.ReadString(), reader.ReadBool());
	}

	// Token: 0x060007DA RID: 2010 RVA: 0x0001FA14 File Offset: 0x0001DC14
	protected void UserCode_RpcPlayVFX__Boolean__Boolean(bool isWin, bool isEnd)
	{
		ParticleSystem particleSystem = this.spinVfxGood;
		if (!isWin)
		{
			particleSystem = this.spinVfxBad;
		}
		if (isEnd)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			main.duration = 1f;
			main.startLifetime = 1f;
			particleSystem.transform.GetChild(0).GetComponent<ParticleSystem>().main.duration = 1f;
		}
		particleSystem.Play();
	}

	// Token: 0x060007DB RID: 2011 RVA: 0x0001FA82 File Offset: 0x0001DC82
	protected static void InvokeUserCode_RpcPlayVFX__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayVFX called on server.");
			return;
		}
		((AngelsReel)obj).UserCode_RpcPlayVFX__Boolean__Boolean(reader.ReadBool(), reader.ReadBool());
	}

	// Token: 0x060007DC RID: 2012 RVA: 0x0001FAB4 File Offset: 0x0001DCB4
	static AngelsReel()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(AngelsReel), "System.Void AngelsReel::RpcSetText(System.String,System.Boolean)", new RemoteCallDelegate(AngelsReel.InvokeUserCode_RpcSetText__String__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(AngelsReel), "System.Void AngelsReel::RpcPlayVFX(System.Boolean,System.Boolean)", new RemoteCallDelegate(AngelsReel.InvokeUserCode_RpcPlayVFX__Boolean__Boolean));
	}

	// Token: 0x060007DD RID: 2013 RVA: 0x0001FB04 File Offset: 0x0001DD04
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarLong(this._lastProfit);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarLong(this._lastProfit);
		}
	}

	// Token: 0x060007DE RID: 2014 RVA: 0x0001FB5C File Offset: 0x0001DD5C
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this._lastProfit, null, reader.ReadVarLong());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this._lastProfit, null, reader.ReadVarLong());
		}
	}

	// Token: 0x0400050C RID: 1292
	[SerializeField]
	private TextMeshPro screenText;

	// Token: 0x0400050D RID: 1293
	[SerializeField]
	private float chanceOfWin;

	// Token: 0x0400050E RID: 1294
	[SerializeField]
	private NetworkAnimator anim;

	// Token: 0x0400050F RID: 1295
	[SerializeField]
	private ParticleSystem spinVfxGood;

	// Token: 0x04000510 RID: 1296
	[SerializeField]
	private ParticleSystem spinVfxBad;

	// Token: 0x04000511 RID: 1297
	private bool _isSpinning;

	// Token: 0x04000512 RID: 1298
	[SyncVar]
	private long _lastProfit;

	// Token: 0x04000513 RID: 1299
	private PlayerProfile _holderProfile;

	// Token: 0x04000514 RID: 1300
	[Header("SFX")]
	[SerializeField]
	private SFXComponent spinSfx;

	// Token: 0x04000515 RID: 1301
	[SerializeField]
	private SFXComponent numChangeSfx;

	// Token: 0x04000516 RID: 1302
	[SerializeField]
	private SFXComponent winSfx;

	// Token: 0x04000517 RID: 1303
	[SerializeField]
	private SFXComponent loseSfx;

	// Token: 0x04000518 RID: 1304
	[SerializeField]
	private SFXComponent destroySfx;

	// Token: 0x04000519 RID: 1305
	[SerializeField]
	private SFXComponent invalidSfx;
}
