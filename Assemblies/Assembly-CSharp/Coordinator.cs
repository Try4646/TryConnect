using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000DF RID: 223
public class Coordinator : ConsumableItem
{
	// Token: 0x060008D1 RID: 2257 RVA: 0x000235A5 File Offset: 0x000217A5
	private void Start()
	{
		this._renderTexture = new RenderTexture(600, 500, 24);
		this.coordinatorCamera.targetTexture = this._renderTexture;
		this.coordinatorRenderTarget.texture = this._renderTexture;
	}

	// Token: 0x060008D2 RID: 2258 RVA: 0x000235E0 File Offset: 0x000217E0
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		this._lastHolder = playerInventory.GetComponent<PlayerProfile>();
	}

	// Token: 0x060008D3 RID: 2259 RVA: 0x000235F5 File Offset: 0x000217F5
	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this.coordinatorCamera != null)
		{
			this.coordinatorCamera.targetTexture = null;
		}
		if (this._renderTexture != null)
		{
			this._renderTexture.Release();
		}
	}

	// Token: 0x060008D4 RID: 2260 RVA: 0x00023630 File Offset: 0x00021830
	[Server]
	public override void ServerTeleport(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Coordinator::ServerTeleport(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (base.NetworkHolder)
		{
			return;
		}
		this._isBreakable = false;
		this.Rb.Teleport(position, true);
	}

	// Token: 0x060008D5 RID: 2261 RVA: 0x00023669 File Offset: 0x00021869
	private IEnumerator SetUnbreakableRoutine()
	{
		yield return new WaitForSeconds(3f);
		this._isBreakable = true;
		yield break;
	}

	// Token: 0x060008D6 RID: 2262 RVA: 0x00023678 File Offset: 0x00021878
	private void OnCollisionEnter(Collision collision)
	{
		if (!base.isServer)
		{
			return;
		}
		if (base.NetworkHolder)
		{
			return;
		}
		if (!this._isBreakable)
		{
			return;
		}
		if (collision.impulse.magnitude < this.shatterThreshold)
		{
			return;
		}
		if (NetworkSingleton<GameManager>.Instance.state == GameState.Game)
		{
			if (this._bankTotal <= 0L)
			{
				return;
			}
			this.Payout();
		}
		this.sFXLoopComponent.RpcLoopSFX(false);
		this.RpcPlaySFX(2);
		base.DestroyItem();
	}

	// Token: 0x060008D7 RID: 2263 RVA: 0x000236F8 File Offset: 0x000218F8
	public override void ServerThrow(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
	{
		base.ServerThrow(position, rotation, velocity, angularVelocity);
		if (velocity.magnitude < this.throwThreshold)
		{
			this._isBreakable = false;
			return;
		}
		this._isBreakable = true;
		if (this._setUnbreakableRoutine != null)
		{
			base.StopCoroutine(this._setUnbreakableRoutine);
		}
		this._setUnbreakableRoutine = base.StartCoroutine(this.SetUnbreakableRoutine());
	}

	// Token: 0x060008D8 RID: 2264 RVA: 0x00023754 File Offset: 0x00021954
	private void Update()
	{
		if (!this._isCameraRendering)
		{
			return;
		}
		float num = 1f / (float)this.cameraFpsLimit;
		if (Time.time >= this._lastCameraRenderTime + num)
		{
			this._lastCameraRenderTime += num;
			this.coordinatorCamera.Render();
		}
	}

	// Token: 0x060008D9 RID: 2265 RVA: 0x000237A0 File Offset: 0x000219A0
	protected override void SubscribeToEvents(bool isSubscribed)
	{
		base.SubscribeToEvents(isSubscribed);
		if (isSubscribed)
		{
			NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered += this.OnResultRegistered;
			InputEvents.OnZoomEvent = (Action<bool>)Delegate.Combine(InputEvents.OnZoomEvent, new Action<bool>(this.OnZoomEvent));
			return;
		}
		NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered -= this.OnResultRegistered;
		InputEvents.OnZoomEvent = (Action<bool>)Delegate.Remove(InputEvents.OnZoomEvent, new Action<bool>(this.OnZoomEvent));
	}

	// Token: 0x060008DA RID: 2266 RVA: 0x00023824 File Offset: 0x00021A24
	private void OnZoomEvent(bool isPressed)
	{
		if (!base.NetworkHolder)
		{
			return;
		}
		if (!base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		if (this.isInPocket)
		{
			return;
		}
		this.ZoomToCamera(isPressed);
		this.CmdOnZoomEvent(isPressed);
	}

	// Token: 0x060008DB RID: 2267 RVA: 0x0002385C File Offset: 0x00021A5C
	[Command(requiresAuthority = false)]
	private void CmdOnZoomEvent(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		base.SendCommandInternal("System.Void Coordinator::CmdOnZoomEvent(System.Boolean)", 1044819190, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060008DC RID: 2268 RVA: 0x00023898 File Offset: 0x00021A98
	[ClientRpc]
	private void RpcOnZoomEvent(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		this.SendRPCInternal("System.Void Coordinator::RpcOnZoomEvent(System.Boolean)", 2034253727, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060008DD RID: 2269 RVA: 0x000238D4 File Offset: 0x00021AD4
	private void ZoomToCamera(bool isPressed)
	{
		this.onHandFb.StopFeedbacks();
		this.modelTransform.DOKill(false);
		if (!isPressed)
		{
			this.modelTransform.DOLocalMove(Vector3.zero, 0.2f, false).SetEase(Ease.OutQuad);
			return;
		}
		if (base.NetworkHolder.isLocalPlayer)
		{
			this.modelTransform.DOLocalMove(new Vector3(0f, 0.4f, -0.35f), 0.2f, false).SetEase(Ease.OutQuad);
			return;
		}
		this.modelTransform.DOLocalMove(new Vector3(0f, 0.4f, -0.15f), 0.2f, false).SetEase(Ease.OutQuad);
	}

	// Token: 0x060008DE RID: 2270 RVA: 0x00023980 File Offset: 0x00021B80
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
		if (!this._isActive)
		{
			return;
		}
		if (this._lastHolder == playerProfile)
		{
			return;
		}
		Vector3 position2 = playerProfile.GetComponent<PlayerController>().head.transform.position;
		if ((position2 - this.coordinatorCamera.transform.position).sqrMagnitude > this.range * this.range)
		{
			return;
		}
		if (!this.IsInCameraFrustum(position2))
		{
			return;
		}
		if (payout > bet && this._currentCharge < this.chargeRenderers.Count)
		{
			this.IncreaseWinning(payout - bet);
		}
		else if (payout < bet)
		{
			this.ResetWinning();
		}
		this.RpcSetCharges(this._currentCharge);
		this.RpcSetText(this._bankTotal, this.multipliers[this._currentCharge]);
		this.RpcResetCamera();
	}

	// Token: 0x060008DF RID: 2271 RVA: 0x00023A64 File Offset: 0x00021C64
	private void IncreaseWinning(long profit)
	{
		this._bankTotal += (long)Math.Round((double)profit * this.directBankMultiplier * (double)NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._lastHolder.steamId, PlayerUpgradeType.Stakeholder));
		this._currentCharge++;
		this.animator.SetTrigger("Shoot");
		this.RpcPlaySFX(0);
	}

	// Token: 0x060008E0 RID: 2272 RVA: 0x00023ACA File Offset: 0x00021CCA
	private void ResetWinning()
	{
		this._bankTotal = 0L;
		this._currentCharge = 0;
		this.animator.SetTrigger("Shoot");
		this.RpcPlaySFX(1);
	}

	// Token: 0x060008E1 RID: 2273 RVA: 0x00023AF4 File Offset: 0x00021CF4
	protected override void OnUseItem(bool isPressed)
	{
		this._isActive = isPressed;
		this.sFXLoopComponent.LoopSFX(isPressed);
		this.renderingIndicator.material.color = (isPressed ? Color.green : Color.red);
		this.flashLight.material.SetColor("_EmissionColor", isPressed ? (Color.white * 5f) : Color.black);
		this.beamTransform.DOScale(isPressed ? (Vector3.one * this.range) : Vector3.zero, 0.2f).SetEase(Ease.Linear);
		this._isCameraRendering = isPressed;
	}

	// Token: 0x060008E2 RID: 2274 RVA: 0x00023B9C File Offset: 0x00021D9C
	private void Payout()
	{
		long num = (long)Math.Round((double)this._bankTotal * this.multipliers[this._currentCharge]);
		if (num <= 0L)
		{
			return;
		}
		if (!this._lastHolder)
		{
			return;
		}
		NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(num, this._lastHolder, ChangeType.Item);
	}

	// Token: 0x060008E3 RID: 2275 RVA: 0x00023BEC File Offset: 0x00021DEC
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this._isActive = false;
		this.RpcResetCamera();
	}

	// Token: 0x060008E4 RID: 2276 RVA: 0x00023C04 File Offset: 0x00021E04
	[ClientRpc]
	private void RpcSetText(long bankTotal, double mult)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarLong(bankTotal);
		writer.WriteDouble(mult);
		this.SendRPCInternal("System.Void Coordinator::RpcSetText(System.Int64,System.Double)", -641863677, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060008E5 RID: 2277 RVA: 0x00023C48 File Offset: 0x00021E48
	[ClientRpc]
	private void RpcSetCharges(int charge)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(charge);
		this.SendRPCInternal("System.Void Coordinator::RpcSetCharges(System.Int32)", 321818366, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060008E6 RID: 2278 RVA: 0x00023C84 File Offset: 0x00021E84
	[ClientRpc]
	private void RpcResetCamera()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Coordinator::RpcResetCamera()", 1066267376, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060008E7 RID: 2279 RVA: 0x00023CB4 File Offset: 0x00021EB4
	[ClientRpc]
	private void RpcPlaySFX(int i)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(i);
		this.SendRPCInternal("System.Void Coordinator::RpcPlaySFX(System.Int32)", -685521556, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060008E8 RID: 2280 RVA: 0x00023CF0 File Offset: 0x00021EF0
	private bool IsInCameraFrustum(Vector3 worldPos)
	{
		Vector3 vector = this.coordinatorCamera.WorldToViewportPoint(worldPos);
		float num = vector.x;
		if (num >= 0f && num <= 1f)
		{
			num = vector.y;
			if (num >= 0f && num <= 1f)
			{
				return vector.z > 0f;
			}
		}
		return false;
	}

	// Token: 0x060008EA RID: 2282 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060008EB RID: 2283 RVA: 0x00023D6D File Offset: 0x00021F6D
	protected void UserCode_CmdOnZoomEvent__Boolean(bool isPressed)
	{
		this.RpcOnZoomEvent(isPressed);
	}

	// Token: 0x060008EC RID: 2284 RVA: 0x00023D76 File Offset: 0x00021F76
	protected static void InvokeUserCode_CmdOnZoomEvent__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnZoomEvent called on client.");
			return;
		}
		((Coordinator)obj).UserCode_CmdOnZoomEvent__Boolean(reader.ReadBool());
	}

	// Token: 0x060008ED RID: 2285 RVA: 0x00023D9F File Offset: 0x00021F9F
	protected void UserCode_RpcOnZoomEvent__Boolean(bool isPressed)
	{
		if (base.NetworkHolder && base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.ZoomToCamera(isPressed);
	}

	// Token: 0x060008EE RID: 2286 RVA: 0x00023DC3 File Offset: 0x00021FC3
	protected static void InvokeUserCode_RpcOnZoomEvent__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnZoomEvent called on server.");
			return;
		}
		((Coordinator)obj).UserCode_RpcOnZoomEvent__Boolean(reader.ReadBool());
	}

	// Token: 0x060008EF RID: 2287 RVA: 0x00023DEC File Offset: 0x00021FEC
	protected void UserCode_RpcSetText__Int64__Double(long bankTotal, double mult)
	{
		this.bankTotalText.text = MoneyFormatter.FormatWithDollar(bankTotal);
		this.multiplierText.text = mult.ToString("0.#") + "x";
		this.potentialWinningText.text = MoneyFormatter.FormatWithDollar((long)Math.Round((double)bankTotal * mult));
	}

	// Token: 0x060008F0 RID: 2288 RVA: 0x00023E45 File Offset: 0x00022045
	protected static void InvokeUserCode_RpcSetText__Int64__Double(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetText called on server.");
			return;
		}
		((Coordinator)obj).UserCode_RpcSetText__Int64__Double(reader.ReadVarLong(), reader.ReadDouble());
	}

	// Token: 0x060008F1 RID: 2289 RVA: 0x00023E78 File Offset: 0x00022078
	protected void UserCode_RpcSetCharges__Int32(int charge)
	{
		for (int i = 0; i < this.chargeRenderers.Count; i++)
		{
			if (i < charge)
			{
				this.chargeRenderers[i].material.SetColor("_EmissionColor", Color.gold * 2f);
			}
			else
			{
				this.chargeRenderers[i].material.SetColor("_EmissionColor", Color.black);
			}
		}
	}

	// Token: 0x060008F2 RID: 2290 RVA: 0x00023EEB File Offset: 0x000220EB
	protected static void InvokeUserCode_RpcSetCharges__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCharges called on server.");
			return;
		}
		((Coordinator)obj).UserCode_RpcSetCharges__Int32(reader.ReadVarInt());
	}

	// Token: 0x060008F3 RID: 2291 RVA: 0x00023F14 File Offset: 0x00022114
	protected void UserCode_RpcResetCamera()
	{
		this.sFXLoopComponent.LoopSFX(false);
		this._isCameraRendering = false;
		this._isActive = false;
		this.renderingIndicator.material.color = Color.red;
		this.flashLight.material.SetColor("_EmissionColor", Color.black);
		this.modelTransform.DOKill(false);
		this.modelTransform.DOLocalMove(Vector3.zero, 0.2f, false).SetEase(Ease.OutQuad);
		this.beamTransform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.Linear);
	}

	// Token: 0x060008F4 RID: 2292 RVA: 0x00023FB0 File Offset: 0x000221B0
	protected static void InvokeUserCode_RpcResetCamera(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetCamera called on server.");
			return;
		}
		((Coordinator)obj).UserCode_RpcResetCamera();
	}

	// Token: 0x060008F5 RID: 2293 RVA: 0x00023FD3 File Offset: 0x000221D3
	protected void UserCode_RpcPlaySFX__Int32(int i)
	{
		SFXManager.SFXOneShot3DAttached(this.oneShotSFX[i], base.gameObject, true);
	}

	// Token: 0x060008F6 RID: 2294 RVA: 0x00023FED File Offset: 0x000221ED
	protected static void InvokeUserCode_RpcPlaySFX__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaySFX called on server.");
			return;
		}
		((Coordinator)obj).UserCode_RpcPlaySFX__Int32(reader.ReadVarInt());
	}

	// Token: 0x060008F7 RID: 2295 RVA: 0x00024018 File Offset: 0x00022218
	static Coordinator()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Coordinator), "System.Void Coordinator::CmdOnZoomEvent(System.Boolean)", new RemoteCallDelegate(Coordinator.InvokeUserCode_CmdOnZoomEvent__Boolean), false);
		RemoteProcedureCalls.RegisterRpc(typeof(Coordinator), "System.Void Coordinator::RpcOnZoomEvent(System.Boolean)", new RemoteCallDelegate(Coordinator.InvokeUserCode_RpcOnZoomEvent__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(Coordinator), "System.Void Coordinator::RpcSetText(System.Int64,System.Double)", new RemoteCallDelegate(Coordinator.InvokeUserCode_RpcSetText__Int64__Double));
		RemoteProcedureCalls.RegisterRpc(typeof(Coordinator), "System.Void Coordinator::RpcSetCharges(System.Int32)", new RemoteCallDelegate(Coordinator.InvokeUserCode_RpcSetCharges__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(Coordinator), "System.Void Coordinator::RpcResetCamera()", new RemoteCallDelegate(Coordinator.InvokeUserCode_RpcResetCamera));
		RemoteProcedureCalls.RegisterRpc(typeof(Coordinator), "System.Void Coordinator::RpcPlaySFX(System.Int32)", new RemoteCallDelegate(Coordinator.InvokeUserCode_RpcPlaySFX__Int32));
	}

	// Token: 0x04000591 RID: 1425
	[Header("References")]
	[SerializeField]
	private Camera coordinatorCamera;

	// Token: 0x04000592 RID: 1426
	[SerializeField]
	private RawImage coordinatorRenderTarget;

	// Token: 0x04000593 RID: 1427
	[SerializeField]
	private TextMeshPro bankTotalText;

	// Token: 0x04000594 RID: 1428
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x04000595 RID: 1429
	[SerializeField]
	private TextMeshPro potentialWinningText;

	// Token: 0x04000596 RID: 1430
	[SerializeField]
	private Transform beamTransform;

	// Token: 0x04000597 RID: 1431
	[SerializeField]
	private MeshRenderer flashLight;

	// Token: 0x04000598 RID: 1432
	[SerializeField]
	private MeshRenderer renderingIndicator;

	// Token: 0x04000599 RID: 1433
	[SerializeField]
	private List<MeshRenderer> chargeRenderers;

	// Token: 0x0400059A RID: 1434
	[SerializeField]
	private NetworkAnimator animator;

	// Token: 0x0400059B RID: 1435
	[Header("Settings")]
	[SerializeField]
	private float range;

	// Token: 0x0400059C RID: 1436
	[SerializeField]
	private double directBankMultiplier;

	// Token: 0x0400059D RID: 1437
	[SerializeField]
	private double[] multipliers;

	// Token: 0x0400059E RID: 1438
	[SerializeField]
	private int cameraFpsLimit = 30;

	// Token: 0x0400059F RID: 1439
	[SerializeField]
	private float throwThreshold = 5f;

	// Token: 0x040005A0 RID: 1440
	[SerializeField]
	private float shatterThreshold = 0.5f;

	// Token: 0x040005A1 RID: 1441
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent sFXLoopComponent;

	// Token: 0x040005A2 RID: 1442
	[SerializeField]
	private EventReference[] oneShotSFX;

	// Token: 0x040005A3 RID: 1443
	private long _bankTotal;

	// Token: 0x040005A4 RID: 1444
	private int _currentCharge;

	// Token: 0x040005A5 RID: 1445
	private PlayerProfile _lastHolder;

	// Token: 0x040005A6 RID: 1446
	private bool _isActive;

	// Token: 0x040005A7 RID: 1447
	private bool _isCameraRendering;

	// Token: 0x040005A8 RID: 1448
	private float _lastCameraRenderTime;

	// Token: 0x040005A9 RID: 1449
	private bool _isBreakable;

	// Token: 0x040005AA RID: 1450
	private Coroutine _setUnbreakableRoutine;

	// Token: 0x040005AB RID: 1451
	private RenderTexture _renderTexture;
}
