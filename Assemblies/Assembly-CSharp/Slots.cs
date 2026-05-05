using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000082 RID: 130
public class Slots : GameBase
{
	// Token: 0x060004AA RID: 1194 RVA: 0x00014F69 File Offset: 0x00013169
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Slots::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		base.StartCoroutine(this.SpinReelsCoroutine());
	}

	// Token: 0x060004AB RID: 1195 RVA: 0x00014F94 File Offset: 0x00013194
	[Server]
	private IEnumerator SpinReelsCoroutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator Slots::SpinReelsCoroutine()' called when server was not active");
			return null;
		}
		Slots.<SpinReelsCoroutine>d__14 <SpinReelsCoroutine>d__ = new Slots.<SpinReelsCoroutine>d__14(0);
		<SpinReelsCoroutine>d__.<>4__this = this;
		return <SpinReelsCoroutine>d__;
	}

	// Token: 0x060004AC RID: 1196 RVA: 0x00014FCF File Offset: 0x000131CF
	private IEnumerator EndGameRoutine()
	{
		this.RpcMultiplierAnimation(true, 0f);
		yield return new WaitForSeconds(0.5f);
		List<Slots.SlotsWinData> winningPatterns = this.GetWinningPatterns();
		foreach (Slots.SlotsWinData slotsWinData in winningPatterns)
		{
			this._finalMultiplier += slotsWinData.Pattern.multiplier * slotsWinData.Multiplier;
			this.RpcSetMultiplier(slotsWinData.Pattern.GetPatternIndexes(), this._finalMultiplier);
			yield return new WaitForSeconds(0.5f);
		}
		List<Slots.SlotsWinData>.Enumerator enumerator = default(List<Slots.SlotsWinData>.Enumerator);
		this.RpcMultiplierAnimation(false, this._finalMultiplier);
		yield return new WaitForSeconds(1f);
		this.Payout((double)this._finalMultiplier * base.EstimatedValue, ChangeType.GameResult, null, -1L);
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
		yield break;
	}

	// Token: 0x060004AD RID: 1197 RVA: 0x00014FE0 File Offset: 0x000131E0
	protected override void ResetGame()
	{
		this._finalMultiplier = 0f;
		foreach (SlotReel slotReel in this.reels)
		{
			slotReel.ServerReset();
		}
		base.ResetGame();
	}

	// Token: 0x060004AE RID: 1198 RVA: 0x00015044 File Offset: 0x00013244
	[ClientRpc]
	private void RpcSetMultiplier(List<int> pattern, float multiplier)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_System.Collections.Generic.List`1<System.Int32>(writer, pattern);
		writer.WriteFloat(multiplier);
		this.SendRPCInternal("System.Void Slots::RpcSetMultiplier(System.Collections.Generic.List`1<System.Int32>,System.Single)", -1876413610, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060004AF RID: 1199 RVA: 0x00015088 File Offset: 0x00013288
	[ClientRpc]
	private void RpcMultiplierAnimation(bool isEnabled, float mult)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		writer.WriteFloat(mult);
		this.SendRPCInternal("System.Void Slots::RpcMultiplierAnimation(System.Boolean,System.Single)", -783525406, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060004B0 RID: 1200 RVA: 0x000150CC File Offset: 0x000132CC
	private List<Slots.SlotsWinData> GetWinningPatterns()
	{
		List<Slots.SlotsWinData> list = new List<Slots.SlotsWinData>();
		List<int> list2 = new List<int>();
		foreach (SlotReel slotReel in this.reels)
		{
			list2.AddRange(slotReel.GetResult());
		}
		foreach (SlotPattern slotPattern in this.patterns)
		{
			List<int> patternIndexes = slotPattern.GetPatternIndexes();
			int num = list2[patternIndexes[0]];
			bool flag = true;
			foreach (int index in patternIndexes)
			{
				if (list2[index] != num)
				{
					flag = false;
				}
			}
			if (flag)
			{
				list.Add(new Slots.SlotsWinData(1f - ((float)num - 1.5f) * 0.5f, slotPattern));
				string str = "pattern match: ";
				SlotPattern slotPattern2 = slotPattern;
				Debug.LogWarning(str + ((slotPattern2 != null) ? slotPattern2.ToString() : null));
			}
		}
		return list;
	}

	// Token: 0x060004B1 RID: 1201 RVA: 0x0001521C File Offset: 0x0001341C
	[ClientRpc]
	private void RpcPlaySpinReelSFX(float duration, SlotReel reel)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(duration);
		writer.WriteNetworkBehaviour(reel);
		this.SendRPCInternal("System.Void Slots::RpcPlaySpinReelSFX(System.Single,SlotReel)", 1195037631, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060004B2 RID: 1202 RVA: 0x00015260 File Offset: 0x00013460
	private void PlaySpinDoneSFX()
	{
		SFXManager.SFXOneShot(this.spinDoneSFX, base.transform.position);
	}

	// Token: 0x060004B3 RID: 1203 RVA: 0x00015278 File Offset: 0x00013478
	private void PlayMultiplierSFX(float mult)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("Multiplier", Mathf.Clamp(mult, 0f, 1000f))
		};
		SFXManager.SFXOneShotWithParameters(this.multiplierSFX, sFXParams, base.transform.position, 1f);
	}

	// Token: 0x060004B4 RID: 1204 RVA: 0x000152CC File Offset: 0x000134CC
	private void PlayFinalTextSFX(float mult)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("Multiplier", Mathf.Clamp(mult, 0f, 1000f))
		};
		SFXManager.SFXOneShotWithParameters(this.finaltextSFX, sFXParams, base.transform.position, 1f);
	}

	// Token: 0x060004B8 RID: 1208 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060004B9 RID: 1209 RVA: 0x0001536C File Offset: 0x0001356C
	protected void UserCode_RpcSetMultiplier__List(List<int> pattern, float multiplier)
	{
		foreach (int index in pattern)
		{
			this.symbolLocations[index].transform.DOPunchScale(Vector3.one * 0.05f, 0.4f, 10, 1f).SetEase(Ease.OutCubic);
			this.symbolLocations[index].DOColor(new Color(1f, 0.9f, 0.9f, 0f), 0.5f).From(new Color(1f, 0.9f, 0.9f, 1f), true, false).SetEase(Ease.OutCubic);
		}
		this.payoutText.text = string.Format("x{0}\n", multiplier);
		this.PlayMultiplierSFX(multiplier);
		Color endValue = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
		this.payoutText.DOColor(endValue, 0.3f).SetEase(Ease.OutCubic);
		this.payoutFb.PlayFeedbacks();
	}

	// Token: 0x060004BA RID: 1210 RVA: 0x000154B4 File Offset: 0x000136B4
	protected static void InvokeUserCode_RpcSetMultiplier__List(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMultiplier called on server.");
			return;
		}
		((Slots)obj).UserCode_RpcSetMultiplier__List`1__Single(Mirror.GeneratedNetworkCode._Read_System.Collections.Generic.List`1<System.Int32>(reader), reader.ReadFloat());
	}

	// Token: 0x060004BB RID: 1211 RVA: 0x000154E4 File Offset: 0x000136E4
	protected void UserCode_RpcMultiplierAnimation__Boolean__Single(bool isEnabled, float mult)
	{
		if (isEnabled)
		{
			this.PlayMultiplierSFX(mult);
			this.payoutText.color = Color.white;
			this.payoutText.text = string.Format("x{0}", mult);
			this.payoutText.transform.DOScale(Vector3.one, 0.3f).From(Vector3.zero, true, false);
			return;
		}
		this.PlayFinalTextSFX(mult);
		if (mult < 10f)
		{
			this.payoutText.text = string.Format("${0}", (float)this.currentBet * mult);
			this.payoutText.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, 1, 1f).OnComplete(delegate
			{
				this.payoutText.transform.DOScale(Vector3.zero, 0.3f).From(Vector3.one, true, false);
			});
			return;
		}
		this.payoutText.text = "Jackpot!\n" + string.Format("${0}", (float)this.currentBet * mult);
		this.payoutText.transform.DOShakeScale(1.5f, new Vector3(0.02f, 0.02f, 0f), 20, 0f, false, ShakeRandomnessMode.Harmonic).OnComplete(delegate
		{
			this.payoutText.transform.DOScale(Vector3.zero, 0.3f).From(Vector3.one, true, false);
		});
		this.payoutText.rectTransform.DOShakeAnchorPos(1.5f, new Vector3(100f, 100f, 0f), 50, 90f, false, false, ShakeRandomnessMode.Harmonic);
	}

	// Token: 0x060004BC RID: 1212 RVA: 0x00015667 File Offset: 0x00013867
	protected static void InvokeUserCode_RpcMultiplierAnimation__Boolean__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMultiplierAnimation called on server.");
			return;
		}
		((Slots)obj).UserCode_RpcMultiplierAnimation__Boolean__Single(reader.ReadBool(), reader.ReadFloat());
	}

	// Token: 0x060004BD RID: 1213 RVA: 0x00015698 File Offset: 0x00013898
	protected void UserCode_RpcPlaySpinReelSFX__Single__SlotReel(float duration, SlotReel reel)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("spinDuration", duration * 1000f)
		};
		SFXManager.SFXOneShotWithParameters(this.spinReelSFX, sFXParams, reel.transform.position, 1f);
		base.Invoke("PlaySpinDoneSFX", duration);
	}

	// Token: 0x060004BE RID: 1214 RVA: 0x000156EC File Offset: 0x000138EC
	protected static void InvokeUserCode_RpcPlaySpinReelSFX__Single__SlotReel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaySpinReelSFX called on server.");
			return;
		}
		((Slots)obj).UserCode_RpcPlaySpinReelSFX__Single__SlotReel(reader.ReadFloat(), reader.ReadNetworkBehaviour<SlotReel>());
	}

	// Token: 0x060004BF RID: 1215 RVA: 0x0001571C File Offset: 0x0001391C
	static Slots()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Slots), "System.Void Slots::RpcSetMultiplier(System.Collections.Generic.List`1<System.Int32>,System.Single)", new RemoteCallDelegate(Slots.InvokeUserCode_RpcSetMultiplier__List`1__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(Slots), "System.Void Slots::RpcMultiplierAnimation(System.Boolean,System.Single)", new RemoteCallDelegate(Slots.InvokeUserCode_RpcMultiplierAnimation__Boolean__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(Slots), "System.Void Slots::RpcPlaySpinReelSFX(System.Single,SlotReel)", new RemoteCallDelegate(Slots.InvokeUserCode_RpcPlaySpinReelSFX__Single__SlotReel));
	}

	// Token: 0x04000334 RID: 820
	[Header("References")]
	[SerializeField]
	private TextMeshPro payoutText;

	// Token: 0x04000335 RID: 821
	[SerializeField]
	private MMF_Player payoutFb;

	// Token: 0x04000336 RID: 822
	[SerializeField]
	private List<SlotReel> reels;

	// Token: 0x04000337 RID: 823
	[SerializeField]
	private List<Image> symbolLocations;

	// Token: 0x04000338 RID: 824
	[SerializeField]
	private List<SlotPattern> patterns;

	// Token: 0x04000339 RID: 825
	[Header("Slots Settings")]
	[SerializeField]
	private float spinDuration = 5f;

	// Token: 0x0400033A RID: 826
	[SerializeField]
	private int spinCount = 15;

	// Token: 0x0400033B RID: 827
	[SerializeField]
	private float delayBetweenReels = 1f;

	// Token: 0x0400033C RID: 828
	[Header("SFX")]
	[SerializeField]
	private EventReference spinReelSFX;

	// Token: 0x0400033D RID: 829
	[SerializeField]
	private EventReference spinDoneSFX;

	// Token: 0x0400033E RID: 830
	[SerializeField]
	private EventReference multiplierSFX;

	// Token: 0x0400033F RID: 831
	[SerializeField]
	private EventReference finaltextSFX;

	// Token: 0x04000340 RID: 832
	private float _finalMultiplier;

	// Token: 0x02000083 RID: 131
	private class SlotsWinData
	{
		// Token: 0x060004C0 RID: 1216 RVA: 0x00015789 File Offset: 0x00013989
		public SlotsWinData(float multiplier, SlotPattern pattern)
		{
			this.Multiplier = multiplier;
			this.Pattern = pattern;
		}

		// Token: 0x04000341 RID: 833
		public float Multiplier;

		// Token: 0x04000342 RID: 834
		public SlotPattern Pattern;
	}
}
