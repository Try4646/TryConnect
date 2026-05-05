using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMODUnity;
using JetBrains.Annotations;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

// Token: 0x02000102 RID: 258
public class BodyShreddingMachine : NetworkBehaviour
{
	// Token: 0x06000A92 RID: 2706 RVA: 0x0002A2AC File Offset: 0x000284AC
	private void Awake()
	{
		this._ls = Resources.Load<LobbySettings>("LobbySettings");
		this._gs = Resources.Load<GameSettings>("GameSettings");
	}

	// Token: 0x06000A93 RID: 2707 RVA: 0x0002A2CE File Offset: 0x000284CE
	private void Start()
	{
		if (!base.isServer)
		{
			return;
		}
		if (this.GetSeededRandom().NextDouble() >= (double)this.spawnChance)
		{
			NetworkServer.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000A94 RID: 2708 RVA: 0x0002A2F8 File Offset: 0x000284F8
	public void OnQuotaChanged()
	{
		this.ServerSetPrices();
	}

	// Token: 0x06000A95 RID: 2709 RVA: 0x0002A300 File Offset: 0x00028500
	public override void OnStartClient()
	{
		base.OnStartClient();
		this.CmdSetPrices();
	}

	// Token: 0x06000A96 RID: 2710 RVA: 0x0002A310 File Offset: 0x00028510
	[Command(requiresAuthority = false)]
	private void CmdSetPrices()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void BodyShreddingMachine::CmdSetPrices()", -19453666, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A97 RID: 2711 RVA: 0x0002A340 File Offset: 0x00028540
	[Server]
	private void ServerSetPrices()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BodyShreddingMachine::ServerSetPrices()' called when server was not active");
			return;
		}
		GameSettings.CasinoFloorData currentFloorData = this._gs.GetCurrentFloorData();
		this._eyePrice = Mathf.RoundToInt(currentFloorData.shreddingEyePrice * this.GetPriceMultiplier());
		this._mouthPrice = Mathf.RoundToInt(currentFloorData.shreddingMouthPrice * this.GetPriceMultiplier());
		this._bodyPrice = Mathf.RoundToInt(currentFloorData.shreddingBodyPrice * this.GetPriceMultiplier());
		this.RpcSetPrices(this._bodyPrice, this._eyePrice, this._mouthPrice);
	}

	// Token: 0x06000A98 RID: 2712 RVA: 0x0002A3D0 File Offset: 0x000285D0
	[ClientRpc]
	private void RpcSetPrices(int bodyPrice, int eyePrice, int mouthPrice)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(bodyPrice);
		writer.WriteVarInt(eyePrice);
		writer.WriteVarInt(mouthPrice);
		this.SendRPCInternal("System.Void BodyShreddingMachine::RpcSetPrices(System.Int32,System.Int32,System.Int32)", 801687128, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A99 RID: 2713 RVA: 0x0002A41E File Offset: 0x0002861E
	[Server]
	public void ToggleLid()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BodyShreddingMachine::ToggleLid()' called when server was not active");
			return;
		}
		this._isLidOpen = !this._isLidOpen;
		this.RpcToggleLid(this._isLidOpen);
	}

	// Token: 0x06000A9A RID: 2714 RVA: 0x0002A450 File Offset: 0x00028650
	[ClientRpc]
	private void RpcToggleLid(bool isOpen)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isOpen);
		this.SendRPCInternal("System.Void BodyShreddingMachine::RpcToggleLid(System.Boolean)", -1431490903, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A9B RID: 2715 RVA: 0x0002A48A File Offset: 0x0002868A
	[Server]
	public void ServerToggleLever()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BodyShreddingMachine::ServerToggleLever()' called when server was not active");
			return;
		}
		this._isLeverBuy = !this._isLeverBuy;
		this.RpcToggleLever(this._isLeverBuy);
	}

	// Token: 0x06000A9C RID: 2716 RVA: 0x0002A4BC File Offset: 0x000286BC
	[ClientRpc]
	private void RpcToggleLever(bool isBuy)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isBuy);
		this.SendRPCInternal("System.Void BodyShreddingMachine::RpcToggleLever(System.Boolean)", 408643580, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A9D RID: 2717 RVA: 0x0002A4F8 File Offset: 0x000286F8
	[Server]
	public void OnEyeButtonClicked()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BodyShreddingMachine::OnEyeButtonClicked()' called when server was not active");
			return;
		}
		if (this._isLidOpen)
		{
			return;
		}
		if (this._isProcessing)
		{
			return;
		}
		PlayerOrgans playersOrgans = this.GetPlayersOrgans();
		if (!playersOrgans)
		{
			return;
		}
		PlayerOrganData playerOrganData = NetworkSingleton<OrganManager>.Instance.OrganData[playersOrgans.connectionToClient.connectionId];
		if (playerOrganData == null)
		{
			return;
		}
		if (this._isLeverBuy)
		{
			if (this.TryBuyEyeBack(playersOrgans, playerOrganData))
			{
				base.StartCoroutine(this.ProcessRoutine());
				return;
			}
		}
		else if (this.TryShredEye(playersOrgans, playerOrganData))
		{
			base.StartCoroutine(this.ProcessRoutine());
		}
	}

	// Token: 0x06000A9E RID: 2718 RVA: 0x0002A594 File Offset: 0x00028794
	[Server]
	private bool TryShredEye(PlayerOrgans po, PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean BodyShreddingMachine::TryShredEye(PlayerOrgans,PlayerOrganData)' called when server was not active");
			return default(bool);
		}
		if (!data.leftEye || !data.rightEye)
		{
			return false;
		}
		if (Random.value > 0.5f)
		{
			NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(po, OrganType.RightEye, false);
		}
		else
		{
			NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(po, OrganType.LeftEye, false);
		}
		NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)this._eyePrice);
		this.RpcOnEyeShredded();
		return true;
	}

	// Token: 0x06000A9F RID: 2719 RVA: 0x0002A618 File Offset: 0x00028818
	[Server]
	private bool TryBuyEyeBack(PlayerOrgans po, PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean BodyShreddingMachine::TryBuyEyeBack(PlayerOrgans,PlayerOrganData)' called when server was not active");
			return default(bool);
		}
		if (data.leftEye && data.rightEye)
		{
			return false;
		}
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)(-(long)Mathf.RoundToInt((float)this._eyePrice))))
		{
			return false;
		}
		if (!data.leftEye)
		{
			NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(po, OrganType.LeftEye, true);
		}
		else if (!data.rightEye)
		{
			NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(po, OrganType.RightEye, true);
		}
		this.buyBackSfx.RpcPlayOneShotWith3DPos();
		return true;
	}

	// Token: 0x06000AA0 RID: 2720 RVA: 0x0002A6AC File Offset: 0x000288AC
	[Server]
	public void OnMouthButtonClicked()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BodyShreddingMachine::OnMouthButtonClicked()' called when server was not active");
			return;
		}
		if (this._isLidOpen)
		{
			return;
		}
		if (this._isProcessing)
		{
			return;
		}
		PlayerOrgans playersOrgans = this.GetPlayersOrgans();
		if (!playersOrgans)
		{
			return;
		}
		PlayerOrganData playerOrganData = NetworkSingleton<OrganManager>.Instance.OrganData[playersOrgans.connectionToClient.connectionId];
		if (playerOrganData == null)
		{
			return;
		}
		if (this._isLeverBuy)
		{
			if (this.TryBuyMouthBack(playersOrgans, playerOrganData))
			{
				base.StartCoroutine(this.ProcessRoutine());
				return;
			}
		}
		else if (this.TryShredMouth(playersOrgans, playerOrganData))
		{
			base.StartCoroutine(this.ProcessRoutine());
		}
	}

	// Token: 0x06000AA1 RID: 2721 RVA: 0x0002A748 File Offset: 0x00028948
	[Server]
	private bool TryShredMouth(PlayerOrgans po, PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean BodyShreddingMachine::TryShredMouth(PlayerOrgans,PlayerOrganData)' called when server was not active");
			return default(bool);
		}
		if (!data.mouth)
		{
			return false;
		}
		NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(po, OrganType.Mouth, false);
		NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)this._mouthPrice);
		this.RpcOnMouthShredded();
		return true;
	}

	// Token: 0x06000AA2 RID: 2722 RVA: 0x0002A7A8 File Offset: 0x000289A8
	[Server]
	private bool TryBuyMouthBack(PlayerOrgans po, PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean BodyShreddingMachine::TryBuyMouthBack(PlayerOrgans,PlayerOrganData)' called when server was not active");
			return default(bool);
		}
		if (data.mouth)
		{
			return false;
		}
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)(-(long)Mathf.RoundToInt((float)this._mouthPrice))))
		{
			return false;
		}
		NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(po, OrganType.Mouth, true);
		this.buyBackSfx.RpcPlayOneShotWith3DPos();
		return true;
	}

	// Token: 0x06000AA3 RID: 2723 RVA: 0x0002A818 File Offset: 0x00028A18
	[Server]
	public void OnBodyButtonClicked()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BodyShreddingMachine::OnBodyButtonClicked()' called when server was not active");
			return;
		}
		if (this._isLidOpen)
		{
			return;
		}
		if (this._isProcessing)
		{
			return;
		}
		PlayerOrgans playersOrgans = this.GetPlayersOrgans();
		if (!playersOrgans)
		{
			return;
		}
		PlayerOrganData playerOrganData = NetworkSingleton<OrganManager>.Instance.OrganData[playersOrgans.connectionToClient.connectionId];
		if (playerOrganData == null)
		{
			return;
		}
		if (this._isLeverBuy)
		{
			if (this.TryBuyBodyBack(playersOrgans, playerOrganData))
			{
				base.StartCoroutine(this.ProcessRoutine());
				return;
			}
		}
		else if (this.TryShredBody(playersOrgans, playerOrganData))
		{
			base.StartCoroutine(this.ProcessRoutine());
		}
	}

	// Token: 0x06000AA4 RID: 2724 RVA: 0x0002A8B4 File Offset: 0x00028AB4
	[Server]
	private bool TryShredBody(PlayerOrgans po, PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean BodyShreddingMachine::TryShredBody(PlayerOrgans,PlayerOrganData)' called when server was not active");
			return default(bool);
		}
		if (!data.body)
		{
			return false;
		}
		NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(po, OrganType.Body, false);
		NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)this._bodyPrice);
		this.RpcOnBodyShredded();
		return true;
	}

	// Token: 0x06000AA5 RID: 2725 RVA: 0x0002A914 File Offset: 0x00028B14
	[Server]
	private bool TryBuyBodyBack(PlayerOrgans po, PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean BodyShreddingMachine::TryBuyBodyBack(PlayerOrgans,PlayerOrganData)' called when server was not active");
			return default(bool);
		}
		if (data.body)
		{
			return false;
		}
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)(-(long)Mathf.RoundToInt((float)this._bodyPrice))))
		{
			return false;
		}
		NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(po, OrganType.Body, true);
		this.buyBackSfx.RpcPlayOneShotWith3DPos();
		return true;
	}

	// Token: 0x06000AA6 RID: 2726 RVA: 0x0002A981 File Offset: 0x00028B81
	private IEnumerator ProcessRoutine()
	{
		this._isProcessing = true;
		this.animator.SetTrigger("Shred");
		this.RpcOnProcess();
		yield return new WaitForSeconds(this.processDuration);
		this._isProcessing = false;
		yield break;
	}

	// Token: 0x06000AA7 RID: 2727 RVA: 0x0002A990 File Offset: 0x00028B90
	[ClientRpc]
	private void RpcOnEyeShredded()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BodyShreddingMachine::RpcOnEyeShredded()", 923359576, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000AA8 RID: 2728 RVA: 0x0002A9C0 File Offset: 0x00028BC0
	[ClientRpc]
	private void RpcOnMouthShredded()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BodyShreddingMachine::RpcOnMouthShredded()", 1991753518, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000AA9 RID: 2729 RVA: 0x0002A9F0 File Offset: 0x00028BF0
	[ClientRpc]
	private void RpcOnBodyShredded()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BodyShreddingMachine::RpcOnBodyShredded()", -125625849, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000AAA RID: 2730 RVA: 0x0002AA20 File Offset: 0x00028C20
	[ClientRpc]
	private void RpcOnProcess()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BodyShreddingMachine::RpcOnProcess()", -1087308049, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000AAB RID: 2731 RVA: 0x0002AA50 File Offset: 0x00028C50
	[CanBeNull]
	private PlayerOrgans GetPlayersOrgans()
	{
		PlayerOrgans result = null;
		float num = float.MaxValue;
		Vector3 center = this.checkCollider.bounds.center;
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			Vector3 position = playerReferences.transform.position;
			if (this.checkCollider.bounds.Contains(position))
			{
				float sqrMagnitude = (position - center).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = playerReferences.organs;
				}
			}
		}
		return result;
	}

	// Token: 0x06000AAC RID: 2732 RVA: 0x0002AB08 File Offset: 0x00028D08
	private float GetPriceMultiplier()
	{
		int currentPlayerCount = this._ls.currentPlayerCount;
		return 1f;
	}

	// Token: 0x06000AAD RID: 2733 RVA: 0x0002AB1C File Offset: 0x00028D1C
	private Random GetSeededRandom()
	{
		if (NetworkSingleton<SeededRandomManager>.Instance == null || NetworkSingleton<GameManager>.Instance == null)
		{
			return new Random(Random.Range(int.MinValue, int.MaxValue));
		}
		int currentSeed = NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
		int daysPassed = NetworkSingleton<GameManager>.Instance.daysPassed;
		return new Random(currentSeed * 31 + daysPassed);
	}

	// Token: 0x06000AAF RID: 2735 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000AB0 RID: 2736 RVA: 0x0002A2F8 File Offset: 0x000284F8
	protected void UserCode_CmdSetPrices()
	{
		this.ServerSetPrices();
	}

	// Token: 0x06000AB1 RID: 2737 RVA: 0x0002AB95 File Offset: 0x00028D95
	protected static void InvokeUserCode_CmdSetPrices(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPrices called on client.");
			return;
		}
		((BodyShreddingMachine)obj).UserCode_CmdSetPrices();
	}

	// Token: 0x06000AB2 RID: 2738 RVA: 0x0002ABB8 File Offset: 0x00028DB8
	protected void UserCode_RpcSetPrices__Int32__Int32__Int32(int bodyPrice, int eyePrice, int mouthPrice)
	{
		this.priceTag.text = string.Concat(new string[]
		{
			"Eye\n",
			string.Format("<color=yellow>{0} Ticket</color>\n", eyePrice),
			"Mouth\n",
			string.Format("<color=yellow>{0} Ticket</color>\n", mouthPrice),
			"Body\n",
			string.Format("<color=yellow>{0} Ticket</color>", bodyPrice)
		});
	}

	// Token: 0x06000AB3 RID: 2739 RVA: 0x0002AC2C File Offset: 0x00028E2C
	protected static void InvokeUserCode_RpcSetPrices__Int32__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPrices called on server.");
			return;
		}
		((BodyShreddingMachine)obj).UserCode_RpcSetPrices__Int32__Int32__Int32(reader.ReadVarInt(), reader.ReadVarInt(), reader.ReadVarInt());
	}

	// Token: 0x06000AB4 RID: 2740 RVA: 0x0002AC64 File Offset: 0x00028E64
	protected void UserCode_RpcToggleLid__Boolean(bool isOpen)
	{
		this.lidTransform.DOLocalRotate(isOpen ? (Vector3.up * -120f) : Vector3.zero, 0.3f, RotateMode.Fast).SetEase(Ease.OutBounce);
		SFXManager.SFXOneShot(isOpen ? this.doorOpenSfx : this.doorCloseSfx, this.lidTransform.position);
	}

	// Token: 0x06000AB5 RID: 2741 RVA: 0x0002ACC4 File Offset: 0x00028EC4
	protected static void InvokeUserCode_RpcToggleLid__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcToggleLid called on server.");
			return;
		}
		((BodyShreddingMachine)obj).UserCode_RpcToggleLid__Boolean(reader.ReadBool());
	}

	// Token: 0x06000AB6 RID: 2742 RVA: 0x0002ACF0 File Offset: 0x00028EF0
	protected void UserCode_RpcToggleLever__Boolean(bool isBuy)
	{
		foreach (Transform target in this.leverTransforms)
		{
			target.DOKill(false);
			target.DOLocalRotate(isBuy ? (Vector3.forward * 75f) : Vector3.zero, 0.2f, RotateMode.Fast).SetEase(Ease.OutBounce);
		}
		MeshRenderer[] array2 = this.buttonMeshes;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].material.SetColor("_Color", isBuy ? this.buttonAddColor : this.buttonRemoveColor);
		}
		SFXManager.SFXOneShot(this.leverSwitchSfx, this.leverTransforms[0].position);
		foreach (TextMeshPro textMeshPro in this.modeTexts)
		{
			textMeshPro.text = (isBuy ? "Buy" : "Sell");
			textMeshPro.color = (isBuy ? Color.forestGreen : Color.crimson);
		}
	}

	// Token: 0x06000AB7 RID: 2743 RVA: 0x0002ADDA File Offset: 0x00028FDA
	protected static void InvokeUserCode_RpcToggleLever__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcToggleLever called on server.");
			return;
		}
		((BodyShreddingMachine)obj).UserCode_RpcToggleLever__Boolean(reader.ReadBool());
	}

	// Token: 0x06000AB8 RID: 2744 RVA: 0x0002AE03 File Offset: 0x00029003
	protected void UserCode_RpcOnEyeShredded()
	{
		SFXManager.SFXOneShot(this.eyeShredSfx, base.transform.position);
	}

	// Token: 0x06000AB9 RID: 2745 RVA: 0x0002AE1B File Offset: 0x0002901B
	protected static void InvokeUserCode_RpcOnEyeShredded(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnEyeShredded called on server.");
			return;
		}
		((BodyShreddingMachine)obj).UserCode_RpcOnEyeShredded();
	}

	// Token: 0x06000ABA RID: 2746 RVA: 0x0002AE3E File Offset: 0x0002903E
	protected void UserCode_RpcOnMouthShredded()
	{
		SFXManager.SFXOneShot(this.mouthShredSfx, base.transform.position);
	}

	// Token: 0x06000ABB RID: 2747 RVA: 0x0002AE56 File Offset: 0x00029056
	protected static void InvokeUserCode_RpcOnMouthShredded(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnMouthShredded called on server.");
			return;
		}
		((BodyShreddingMachine)obj).UserCode_RpcOnMouthShredded();
	}

	// Token: 0x06000ABC RID: 2748 RVA: 0x0002AE79 File Offset: 0x00029079
	protected void UserCode_RpcOnBodyShredded()
	{
		SFXManager.SFXOneShot(this.bodyShredSfx, base.transform.position);
	}

	// Token: 0x06000ABD RID: 2749 RVA: 0x0002AE91 File Offset: 0x00029091
	protected static void InvokeUserCode_RpcOnBodyShredded(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnBodyShredded called on server.");
			return;
		}
		((BodyShreddingMachine)obj).UserCode_RpcOnBodyShredded();
	}

	// Token: 0x06000ABE RID: 2750 RVA: 0x0002AEB4 File Offset: 0x000290B4
	protected void UserCode_RpcOnProcess()
	{
		this.wiggle.WiggleRotation(1f);
	}

	// Token: 0x06000ABF RID: 2751 RVA: 0x0002AEC6 File Offset: 0x000290C6
	protected static void InvokeUserCode_RpcOnProcess(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnProcess called on server.");
			return;
		}
		((BodyShreddingMachine)obj).UserCode_RpcOnProcess();
	}

	// Token: 0x06000AC0 RID: 2752 RVA: 0x0002AEEC File Offset: 0x000290EC
	static BodyShreddingMachine()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(BodyShreddingMachine), "System.Void BodyShreddingMachine::CmdSetPrices()", new RemoteCallDelegate(BodyShreddingMachine.InvokeUserCode_CmdSetPrices), false);
		RemoteProcedureCalls.RegisterRpc(typeof(BodyShreddingMachine), "System.Void BodyShreddingMachine::RpcSetPrices(System.Int32,System.Int32,System.Int32)", new RemoteCallDelegate(BodyShreddingMachine.InvokeUserCode_RpcSetPrices__Int32__Int32__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(BodyShreddingMachine), "System.Void BodyShreddingMachine::RpcToggleLid(System.Boolean)", new RemoteCallDelegate(BodyShreddingMachine.InvokeUserCode_RpcToggleLid__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(BodyShreddingMachine), "System.Void BodyShreddingMachine::RpcToggleLever(System.Boolean)", new RemoteCallDelegate(BodyShreddingMachine.InvokeUserCode_RpcToggleLever__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(BodyShreddingMachine), "System.Void BodyShreddingMachine::RpcOnEyeShredded()", new RemoteCallDelegate(BodyShreddingMachine.InvokeUserCode_RpcOnEyeShredded));
		RemoteProcedureCalls.RegisterRpc(typeof(BodyShreddingMachine), "System.Void BodyShreddingMachine::RpcOnMouthShredded()", new RemoteCallDelegate(BodyShreddingMachine.InvokeUserCode_RpcOnMouthShredded));
		RemoteProcedureCalls.RegisterRpc(typeof(BodyShreddingMachine), "System.Void BodyShreddingMachine::RpcOnBodyShredded()", new RemoteCallDelegate(BodyShreddingMachine.InvokeUserCode_RpcOnBodyShredded));
		RemoteProcedureCalls.RegisterRpc(typeof(BodyShreddingMachine), "System.Void BodyShreddingMachine::RpcOnProcess()", new RemoteCallDelegate(BodyShreddingMachine.InvokeUserCode_RpcOnProcess));
	}

	// Token: 0x040006A3 RID: 1699
	[Header("Settings")]
	[SerializeField]
	[Range(0f, 1f)]
	private float spawnChance = 0.5f;

	// Token: 0x040006A4 RID: 1700
	[SerializeField]
	private float processDuration = 0.1f;

	// Token: 0x040006A5 RID: 1701
	[SerializeField]
	private Color buttonRemoveColor;

	// Token: 0x040006A6 RID: 1702
	[SerializeField]
	private Color buttonAddColor;

	// Token: 0x040006A7 RID: 1703
	[Header("References")]
	[SerializeField]
	private Collider checkCollider;

	// Token: 0x040006A8 RID: 1704
	[SerializeField]
	private TextMeshPro priceTag;

	// Token: 0x040006A9 RID: 1705
	[SerializeField]
	private TextMeshPro[] modeTexts;

	// Token: 0x040006AA RID: 1706
	[SerializeField]
	private NetworkAnimator animator;

	// Token: 0x040006AB RID: 1707
	[SerializeField]
	private MMWiggle wiggle;

	// Token: 0x040006AC RID: 1708
	[SerializeField]
	private Transform lidTransform;

	// Token: 0x040006AD RID: 1709
	[SerializeField]
	private MeshRenderer[] buttonMeshes;

	// Token: 0x040006AE RID: 1710
	[SerializeField]
	private Transform[] leverTransforms;

	// Token: 0x040006AF RID: 1711
	[Header("SFX")]
	[SerializeField]
	private EventReference bodyShredSfx;

	// Token: 0x040006B0 RID: 1712
	[SerializeField]
	private EventReference eyeShredSfx;

	// Token: 0x040006B1 RID: 1713
	[SerializeField]
	private EventReference mouthShredSfx;

	// Token: 0x040006B2 RID: 1714
	[SerializeField]
	private EventReference leverSwitchSfx;

	// Token: 0x040006B3 RID: 1715
	[SerializeField]
	private EventReference doorOpenSfx;

	// Token: 0x040006B4 RID: 1716
	[SerializeField]
	private EventReference doorCloseSfx;

	// Token: 0x040006B5 RID: 1717
	[SerializeField]
	private SFXComponent buyBackSfx;

	// Token: 0x040006B6 RID: 1718
	private LobbySettings _ls;

	// Token: 0x040006B7 RID: 1719
	private GameSettings _gs;

	// Token: 0x040006B8 RID: 1720
	private int _bodyPrice;

	// Token: 0x040006B9 RID: 1721
	private int _eyePrice;

	// Token: 0x040006BA RID: 1722
	private int _mouthPrice;

	// Token: 0x040006BB RID: 1723
	private bool _isLidOpen;

	// Token: 0x040006BC RID: 1724
	private bool _isLeverBuy;

	// Token: 0x040006BD RID: 1725
	private bool _isProcessing;
}
