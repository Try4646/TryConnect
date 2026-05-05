using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000255 RID: 597
public class SkipUI : NetworkBehaviour
{
	// Token: 0x0600154A RID: 5450 RVA: 0x0005B583 File Offset: 0x00059783
	private void Awake()
	{
		this._colorPalette = Resources.Load<UIColorPalette>("ColorSettings");
		this._lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
	}

	// Token: 0x0600154B RID: 5451 RVA: 0x0005B5A5 File Offset: 0x000597A5
	private void OnEnable()
	{
		InputEvents.OnSkipUIEvent = (Action<bool>)Delegate.Combine(InputEvents.OnSkipUIEvent, new Action<bool>(this.SkipCredits));
	}

	// Token: 0x0600154C RID: 5452 RVA: 0x0005B5C7 File Offset: 0x000597C7
	private void OnDisable()
	{
		InputEvents.OnSkipUIEvent = (Action<bool>)Delegate.Remove(InputEvents.OnSkipUIEvent, new Action<bool>(this.SkipCredits));
	}

	// Token: 0x0600154D RID: 5453 RVA: 0x0005B5EC File Offset: 0x000597EC
	private void SkipCredits(bool isPressed)
	{
		if (this._hasSkipped)
		{
			return;
		}
		if (!this._isSkippableLocal)
		{
			return;
		}
		if (this._skipRoutine != null)
		{
			base.StopCoroutine(this._skipRoutine);
		}
		if (isPressed)
		{
			this._skipRoutine = base.StartCoroutine(this.SkipRoutine());
			return;
		}
		this.skipFillImage.fillAmount = 0f;
		if (this.skipBarSfx != null)
		{
			this.skipBarSfx.LoopSFX(false);
		}
	}

	// Token: 0x0600154E RID: 5454 RVA: 0x0005B65F File Offset: 0x0005985F
	private IEnumerator SkipRoutine()
	{
		if (this.skipBarSfx != null)
		{
			this.skipBarSfx.LoopSFX(true);
		}
		float t = 0f;
		while (t < this.skipTime)
		{
			yield return null;
			t += Time.deltaTime;
			this.skipFillImage.fillAmount = t / this.skipTime;
		}
		this.OnSkipFilled();
		yield break;
	}

	// Token: 0x0600154F RID: 5455 RVA: 0x0005B66E File Offset: 0x0005986E
	private void OnSkipFilled()
	{
		this._hasSkipped = true;
		if (this.skipBarSfx != null)
		{
			this.skipBarSfx.LoopSFX(false);
		}
		this.RequestSkip(null);
	}

	// Token: 0x06001550 RID: 5456 RVA: 0x0005B698 File Offset: 0x00059898
	[Server]
	public void ServerRegisterSkipFromConnection(NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkipUI::ServerRegisterSkipFromConnection(Mirror.NetworkConnectionToClient)' called when server was not active");
			return;
		}
		if (!this._isSkippable)
		{
			return;
		}
		if (!this._skippedPlayers.Add(conn))
		{
			return;
		}
		this.RpcMarkPlayerSkipped(conn.identity.netId);
		if (this._skippedPlayers.Count >= MonoSingleton<LocalManager>.Instance.players.Count)
		{
			this.ServerSkip();
		}
	}

	// Token: 0x06001551 RID: 5457 RVA: 0x0005B708 File Offset: 0x00059908
	[Command(requiresAuthority = false)]
	private void RequestSkip(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void SkipUI::RequestSkip(Mirror.NetworkConnectionToClient)", -1245224911, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001552 RID: 5458 RVA: 0x0005B738 File Offset: 0x00059938
	private void ServerSkip()
	{
		this._isSkippable = false;
		NetworkSingleton<GameManager>.Instance.ProgressGame();
		this.RpcSkip();
	}

	// Token: 0x06001553 RID: 5459 RVA: 0x0005B754 File Offset: 0x00059954
	[ClientRpc]
	private void RpcSkip()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void SkipUI::RpcSkip()", 1253806432, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001554 RID: 5460 RVA: 0x0005B784 File Offset: 0x00059984
	[ClientRpc]
	private void RpcMarkPlayerSkipped(uint id)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarUInt(id);
		this.SendRPCInternal("System.Void SkipUI::RpcMarkPlayerSkipped(System.UInt32)", 977291795, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001555 RID: 5461 RVA: 0x0005B7C0 File Offset: 0x000599C0
	public void Reset()
	{
		this._isSkippable = false;
		this._isSkippableLocal = false;
		this._hasSkipped = false;
		this._skippedPlayers.Clear();
		if (this.skippedHeadsRoot)
		{
			for (int i = this.skippedHeadsRoot.childCount - 1; i >= 0; i--)
			{
				Object.Destroy(this.skippedHeadsRoot.GetChild(i).gameObject);
			}
		}
		this._headIconViews.Clear();
		this.skipFillImage.fillAmount = 0f;
		this.canvasGroup.alpha = 0f;
	}

	// Token: 0x06001556 RID: 5462 RVA: 0x0005B853 File Offset: 0x00059A53
	public void SetSkippableServer()
	{
		if (!base.isServer)
		{
			return;
		}
		if (this._isSkippable)
		{
			return;
		}
		this._isSkippable = true;
		this.RpcSetSkippableUI();
	}

	// Token: 0x06001557 RID: 5463 RVA: 0x0005B874 File Offset: 0x00059A74
	[ClientRpc]
	private void RpcSetSkippableUI()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void SkipUI::RpcSetSkippableUI()", 1580893612, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001558 RID: 5464 RVA: 0x0005B8A4 File Offset: 0x00059AA4
	public void SetSkippableForLocal()
	{
		this.canvasGroup.DOFade(1f, this.fadeTime);
		this._isSkippableLocal = true;
	}

	// Token: 0x06001559 RID: 5465 RVA: 0x0005B8C4 File Offset: 0x00059AC4
	private void BuildHeadIcons()
	{
		for (int i = this.skippedHeadsRoot.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(this.skippedHeadsRoot.GetChild(i).gameObject);
		}
		this._headIconViews.Clear();
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			if (!(((playerReferences != null) ? playerReferences.identity : null) == null))
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.headIconPrefab, this.skippedHeadsRoot);
				Image component = gameObject.GetComponent<Image>();
				CanvasGroup componentInChildren = gameObject.GetComponentInChildren<CanvasGroup>(true);
				SkipUI.HeadIconView headIconView = new SkipUI.HeadIconView
				{
					Image = component,
					CanvasGroup = componentInChildren
				};
				if (headIconView.Image)
				{
					Color playerColor = this.GetPlayerColor(playerReferences);
					playerColor.a = 1f;
					headIconView.Image.color = playerColor;
				}
				headIconView.SetSkipped(false, this.fadedAlpha);
				this._headIconViews[playerReferences.identity.netId] = headIconView;
			}
		}
	}

	// Token: 0x0600155A RID: 5466 RVA: 0x0005B9F8 File Offset: 0x00059BF8
	private Color GetPlayerColor(PlayerReferences player)
	{
		if (!((player != null) ? player.profile : null))
		{
			if (!this._colorPalette)
			{
				return Color.white;
			}
			return this._colorPalette.playerColor;
		}
		else
		{
			PlayerInfo playerInfo = this._lobbySettings ? this._lobbySettings.GetPlayerBySteamId(player.profile.steamId) : null;
			if (playerInfo != null)
			{
				return playerInfo.playerColor;
			}
			if (!this._colorPalette)
			{
				return Color.white;
			}
			return this._colorPalette.playerColor;
		}
	}

	// Token: 0x0600155C RID: 5468 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600155D RID: 5469 RVA: 0x0005BAC4 File Offset: 0x00059CC4
	protected void UserCode_RequestSkip__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (!this._isSkippable)
		{
			return;
		}
		if (!this._skippedPlayers.Add(sender))
		{
			return;
		}
		this.RpcMarkPlayerSkipped(sender.identity.netId);
		if (this._skippedPlayers.Count >= MonoSingleton<LocalManager>.Instance.players.Count)
		{
			this.ServerSkip();
		}
	}

	// Token: 0x0600155E RID: 5470 RVA: 0x0005BB1C File Offset: 0x00059D1C
	protected static void InvokeUserCode_RequestSkip__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command RequestSkip called on client.");
			return;
		}
		((SkipUI)obj).UserCode_RequestSkip__NetworkConnectionToClient(senderConnection);
	}

	// Token: 0x0600155F RID: 5471 RVA: 0x0005BB40 File Offset: 0x00059D40
	protected void UserCode_RpcSkip()
	{
		UnityAction onSkipped = this.OnSkipped;
		if (onSkipped != null)
		{
			onSkipped();
		}
		this.canvasGroup.DOFade(0f, this.fadeTime);
	}

	// Token: 0x06001560 RID: 5472 RVA: 0x0005BB6A File Offset: 0x00059D6A
	protected static void InvokeUserCode_RpcSkip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSkip called on server.");
			return;
		}
		((SkipUI)obj).UserCode_RpcSkip();
	}

	// Token: 0x06001561 RID: 5473 RVA: 0x0005BB90 File Offset: 0x00059D90
	protected void UserCode_RpcMarkPlayerSkipped__UInt32(uint id)
	{
		SkipUI.HeadIconView headIconView;
		if (this._headIconViews.TryGetValue(id, out headIconView))
		{
			headIconView.SetSkipped(true, this.fadedAlpha);
			if (this.headAppearSfx != null)
			{
				this.headAppearSfx.PlayOneShotWith3DPos();
			}
		}
	}

	// Token: 0x06001562 RID: 5474 RVA: 0x0005BBD3 File Offset: 0x00059DD3
	protected static void InvokeUserCode_RpcMarkPlayerSkipped__UInt32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMarkPlayerSkipped called on server.");
			return;
		}
		((SkipUI)obj).UserCode_RpcMarkPlayerSkipped__UInt32(reader.ReadVarUInt());
	}

	// Token: 0x06001563 RID: 5475 RVA: 0x0005BBFC File Offset: 0x00059DFC
	protected void UserCode_RpcSetSkippableUI()
	{
		this.BuildHeadIcons();
	}

	// Token: 0x06001564 RID: 5476 RVA: 0x0005BC04 File Offset: 0x00059E04
	protected static void InvokeUserCode_RpcSetSkippableUI(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSkippableUI called on server.");
			return;
		}
		((SkipUI)obj).UserCode_RpcSetSkippableUI();
	}

	// Token: 0x06001565 RID: 5477 RVA: 0x0005BC28 File Offset: 0x00059E28
	static SkipUI()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(SkipUI), "System.Void SkipUI::RequestSkip(Mirror.NetworkConnectionToClient)", new RemoteCallDelegate(SkipUI.InvokeUserCode_RequestSkip__NetworkConnectionToClient), false);
		RemoteProcedureCalls.RegisterRpc(typeof(SkipUI), "System.Void SkipUI::RpcSkip()", new RemoteCallDelegate(SkipUI.InvokeUserCode_RpcSkip));
		RemoteProcedureCalls.RegisterRpc(typeof(SkipUI), "System.Void SkipUI::RpcMarkPlayerSkipped(System.UInt32)", new RemoteCallDelegate(SkipUI.InvokeUserCode_RpcMarkPlayerSkipped__UInt32));
		RemoteProcedureCalls.RegisterRpc(typeof(SkipUI), "System.Void SkipUI::RpcSetSkippableUI()", new RemoteCallDelegate(SkipUI.InvokeUserCode_RpcSetSkippableUI));
	}

	// Token: 0x04000DA0 RID: 3488
	[Header("Settings")]
	[SerializeField]
	private float fadeTime = 1f;

	// Token: 0x04000DA1 RID: 3489
	[SerializeField]
	private float skipTime = 1f;

	// Token: 0x04000DA2 RID: 3490
	[Header("References")]
	[SerializeField]
	private CanvasGroup canvasGroup;

	// Token: 0x04000DA3 RID: 3491
	[SerializeField]
	private Image skipFillImage;

	// Token: 0x04000DA4 RID: 3492
	[Header("Skipped Heads")]
	[SerializeField]
	private Transform skippedHeadsRoot;

	// Token: 0x04000DA5 RID: 3493
	[SerializeField]
	private GameObject headIconPrefab;

	// Token: 0x04000DA6 RID: 3494
	[SerializeField]
	private float fadedAlpha = 0.25f;

	// Token: 0x04000DA7 RID: 3495
	[Header("SFX")]
	[SerializeField]
	private SFXLocalLoopComponent skipBarSfx;

	// Token: 0x04000DA8 RID: 3496
	[SerializeField]
	private SFXLocalPlayer headAppearSfx;

	// Token: 0x04000DA9 RID: 3497
	private UIColorPalette _colorPalette;

	// Token: 0x04000DAA RID: 3498
	private LobbySettings _lobbySettings;

	// Token: 0x04000DAB RID: 3499
	private bool _isSkippable;

	// Token: 0x04000DAC RID: 3500
	private bool _isSkippableLocal;

	// Token: 0x04000DAD RID: 3501
	private bool _hasSkipped;

	// Token: 0x04000DAE RID: 3502
	private Coroutine _skipRoutine;

	// Token: 0x04000DAF RID: 3503
	private readonly HashSet<NetworkConnectionToClient> _skippedPlayers = new HashSet<NetworkConnectionToClient>();

	// Token: 0x04000DB0 RID: 3504
	private readonly Dictionary<uint, SkipUI.HeadIconView> _headIconViews = new Dictionary<uint, SkipUI.HeadIconView>();

	// Token: 0x04000DB1 RID: 3505
	public UnityAction OnSkipped;

	// Token: 0x02000256 RID: 598
	private class HeadIconView
	{
		// Token: 0x06001566 RID: 5478 RVA: 0x0005BCB6 File Offset: 0x00059EB6
		public void SetSkipped(bool skipped, float fadedAlpha)
		{
			if (!this.CanvasGroup)
			{
				return;
			}
			this.CanvasGroup.alpha = (skipped ? 1f : fadedAlpha);
		}

		// Token: 0x04000DB2 RID: 3506
		public Image Image;

		// Token: 0x04000DB3 RID: 3507
		public CanvasGroup CanvasGroup;
	}
}
