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

// Token: 0x020000E3 RID: 227
public class DevilsReel : ConsumableItem
{
	// Token: 0x06000915 RID: 2325 RVA: 0x000246B5 File Offset: 0x000228B5
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

	// Token: 0x06000916 RID: 2326 RVA: 0x000246F0 File Offset: 0x000228F0
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
			string str = MoneyFormatter.FormatWithDollar((long)((double)(this._lastProfit * 2L) * (double)NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._holderProfile.steamId, PlayerUpgradeType.Stakeholder)));
			string str2 = MoneyFormatter.FormatWithDollar(this._lastProfit);
			if (this._lastProfit > 0L)
			{
				this.RpcSetText("-" + str2 + " | +" + str, false);
				return;
			}
			this.RpcSetText("-", false);
		}
	}

	// Token: 0x06000917 RID: 2327 RVA: 0x000247AC File Offset: 0x000229AC
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
			string str = MoneyFormatter.FormatWithDollar((long)((double)(this._lastProfit * 2L) * (double)NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._holderProfile.steamId, PlayerUpgradeType.Stakeholder)));
			string str2 = MoneyFormatter.FormatWithDollar(this._lastProfit);
			if (this._lastProfit > 0L)
			{
				this.RpcSetText("-" + str2 + " | +" + str, false);
				return;
			}
			this.RpcSetText("-", false);
		}
	}

	// Token: 0x06000918 RID: 2328 RVA: 0x0002485E File Offset: 0x00022A5E
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

	// Token: 0x06000919 RID: 2329 RVA: 0x0002488C File Offset: 0x00022A8C
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
		if (this._lastProfit <= 0L)
		{
			this.PlayUnableToUseFeedback();
			return;
		}
		if (NetworkSingleton<MoneyManager>.Instance.balance < this._lastProfit)
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

	// Token: 0x0600091A RID: 2330 RVA: 0x000248F3 File Offset: 0x00022AF3
	private IEnumerator SpinRoutine()
	{
		this.anim.SetTrigger("Spin");
		yield return new WaitForSeconds(1f);
		this.spinSfx.RpcPlayOneShotAttached();
		float totalDuration = 3f;
		float minInterval = 0.1f;
		float maxInterval = 0.35f;
		PlayerProfile holder = this._holderProfile;
		long lose = -this._lastProfit;
		long win = (long)((double)(this._lastProfit * 2L) * (double)NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(holder.steamId, PlayerUpgradeType.Stakeholder));
		float interval;
		for (float elapsed = 0f; elapsed < totalDuration; elapsed += interval)
		{
			float num = elapsed / totalDuration;
			float t = 1f - Mathf.Pow(1f - num, 3f);
			interval = Mathf.Lerp(minInterval, maxInterval, t);
			bool flag = Random.value > 0.5f;
			long amount = flag ? win : lose;
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
		long finalValue = flag2 ? win : lose;
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
		NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(finalValue, holder, ChangeType.Misc);
		this.destroySfx.RpcPlayOneShotWith3DPos();
		base.DestroyItem();
		yield break;
	}

	// Token: 0x0600091B RID: 2331 RVA: 0x00024904 File Offset: 0x00022B04
	[ClientRpc]
	private void RpcSetText(string text, bool punch)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(text);
		writer.WriteBool(punch);
		this.SendRPCInternal("System.Void DevilsReel::RpcSetText(System.String,System.Boolean)", -1982567001, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600091C RID: 2332 RVA: 0x00024948 File Offset: 0x00022B48
	[ClientRpc]
	private void RpcPlayVFX(bool isWin, bool isEnd)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isWin);
		writer.WriteBool(isEnd);
		this.SendRPCInternal("System.Void DevilsReel::RpcPlayVFX(System.Boolean,System.Boolean)", 751801301, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600091D RID: 2333 RVA: 0x0002498C File Offset: 0x00022B8C
	private void PlayUnableToUseFeedback()
	{
		this.invalidSfx.PlayOneShotAttached();
		this.screenText.transform.DOPunchScale(this.screenText.transform.localScale * 0.5f, 0.1f, 1, 1f);
	}

	// Token: 0x0600091E RID: 2334 RVA: 0x000249DC File Offset: 0x00022BDC
	private Random GetSeededRandom()
	{
		if (!NetworkSingleton<SeededRandomManager>.Instance || !NetworkSingleton<GameManager>.Instance)
		{
			return new Random(Random.Range(int.MinValue, int.MaxValue));
		}
		long currentSeed = (long)NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
		int daysPassed = NetworkSingleton<GameManager>.Instance.daysPassed;
		int devilsReelCounter = NetworkSingleton<SeededRandomManager>.Instance.DevilsReelCounter;
		long num = ((currentSeed * (long)((ulong)-1640531535) + (long)daysPassed) * (long)((ulong)-1640531535) + (long)devilsReelCounter) * (long)((ulong)-1640531535) ^ (long)((long)devilsReelCounter << 13) ^ (long)(devilsReelCounter >> 7);
		long num2 = (num ^ num >> 32) * (long)((ulong)-2048144789);
		long num3 = (num2 ^ num2 >> 16) * (long)((ulong)-1028477379);
		return new Random((int)(num3 ^ num3 >> 13));
	}

	// Token: 0x06000920 RID: 2336 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000CD RID: 205
	// (get) Token: 0x06000921 RID: 2337 RVA: 0x00024A80 File Offset: 0x00022C80
	// (set) Token: 0x06000922 RID: 2338 RVA: 0x00024A93 File Offset: 0x00022C93
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

	// Token: 0x06000923 RID: 2339 RVA: 0x00024AB0 File Offset: 0x00022CB0
	protected void UserCode_RpcSetText__String__Boolean(string text, bool punch)
	{
		this.screenText.text = text;
		if (punch)
		{
			this.screenText.transform.DOPunchScale(this.screenText.transform.localScale * 0.2f, 0.1f, 1, 0f);
		}
	}

	// Token: 0x06000924 RID: 2340 RVA: 0x00024B02 File Offset: 0x00022D02
	protected static void InvokeUserCode_RpcSetText__String__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetText called on server.");
			return;
		}
		((DevilsReel)obj).UserCode_RpcSetText__String__Boolean(reader.ReadString(), reader.ReadBool());
	}

	// Token: 0x06000925 RID: 2341 RVA: 0x00024B34 File Offset: 0x00022D34
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

	// Token: 0x06000926 RID: 2342 RVA: 0x00024BA2 File Offset: 0x00022DA2
	protected static void InvokeUserCode_RpcPlayVFX__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayVFX called on server.");
			return;
		}
		((DevilsReel)obj).UserCode_RpcPlayVFX__Boolean__Boolean(reader.ReadBool(), reader.ReadBool());
	}

	// Token: 0x06000927 RID: 2343 RVA: 0x00024BD4 File Offset: 0x00022DD4
	static DevilsReel()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DevilsReel), "System.Void DevilsReel::RpcSetText(System.String,System.Boolean)", new RemoteCallDelegate(DevilsReel.InvokeUserCode_RpcSetText__String__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(DevilsReel), "System.Void DevilsReel::RpcPlayVFX(System.Boolean,System.Boolean)", new RemoteCallDelegate(DevilsReel.InvokeUserCode_RpcPlayVFX__Boolean__Boolean));
	}

	// Token: 0x06000928 RID: 2344 RVA: 0x00024C24 File Offset: 0x00022E24
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

	// Token: 0x06000929 RID: 2345 RVA: 0x00024C7C File Offset: 0x00022E7C
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

	// Token: 0x040005C0 RID: 1472
	[SerializeField]
	private TextMeshPro screenText;

	// Token: 0x040005C1 RID: 1473
	[SerializeField]
	private float chanceOfWin;

	// Token: 0x040005C2 RID: 1474
	[SerializeField]
	private NetworkAnimator anim;

	// Token: 0x040005C3 RID: 1475
	[SerializeField]
	private ParticleSystem spinVfxGood;

	// Token: 0x040005C4 RID: 1476
	[SerializeField]
	private ParticleSystem spinVfxBad;

	// Token: 0x040005C5 RID: 1477
	private bool _isSpinning;

	// Token: 0x040005C6 RID: 1478
	[SyncVar]
	private long _lastProfit;

	// Token: 0x040005C7 RID: 1479
	private PlayerProfile _holderProfile;

	// Token: 0x040005C8 RID: 1480
	[Header("SFX")]
	[SerializeField]
	private SFXComponent spinSfx;

	// Token: 0x040005C9 RID: 1481
	[SerializeField]
	private SFXComponent numChangeSfx;

	// Token: 0x040005CA RID: 1482
	[SerializeField]
	private SFXComponent winSfx;

	// Token: 0x040005CB RID: 1483
	[SerializeField]
	private SFXComponent loseSfx;

	// Token: 0x040005CC RID: 1484
	[SerializeField]
	private SFXComponent destroySfx;

	// Token: 0x040005CD RID: 1485
	[SerializeField]
	private SFXComponent invalidSfx;
}
