using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMOD.Studio;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000050 RID: 80
public class DuckRace : GameBase
{
	// Token: 0x06000238 RID: 568 RVA: 0x0000C3F4 File Offset: 0x0000A5F4
	protected override void OnAwake()
	{
		base.OnAwake();
		foreach (Renderer renderer in this.selectedDuckLights)
		{
			this._sdmpbs.Add(new MaterialPropertyBlock());
		}
		this.PlaceBetFeedback(0);
	}

	// Token: 0x06000239 RID: 569 RVA: 0x0000C460 File Offset: 0x0000A660
	[Command(requiresAuthority = false)]
	public void ServerPlaceBet(int duckIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(duckIndex);
		base.SendCommandInternal("System.Void DuckRace::ServerPlaceBet(System.Int32)", 1419084891, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600023A RID: 570 RVA: 0x0000C49C File Offset: 0x0000A69C
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DuckRace::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		for (int i = 0; i < this.ducks.Count; i++)
		{
			this.ducks[i].ServerStartRace(base.GetSeededRandom(i * 999));
		}
		this.RpcSetCrown(true);
		this.RpcDuckMusic(true);
	}

	// Token: 0x0600023B RID: 571 RVA: 0x0000C508 File Offset: 0x0000A708
	private void Update()
	{
		if (this._winningDuckIndex >= 0)
		{
			this.SetCrownPosition();
		}
		if (!this.isPlaying && this.hasEnded)
		{
			return;
		}
		if (base.isServer)
		{
			for (int i = 0; i < this.ducks.Count; i++)
			{
				if (this._winningDuckIndex < 0 || this.ducks[i].transform.localPosition.z > this.ducks[this._winningDuckIndex].transform.localPosition.z)
				{
					this.Network_winningDuckIndex = i;
				}
			}
		}
	}

	// Token: 0x0600023C RID: 572 RVA: 0x0000C5A0 File Offset: 0x0000A7A0
	[Server]
	public bool OnDuckFinish(DuckRaceDuck duck)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean DuckRace::OnDuckFinish(DuckRaceDuck)' called when server was not active");
			return default(bool);
		}
		if (!this.isPlaying)
		{
			return false;
		}
		if (this.hasEnded)
		{
			return false;
		}
		this.hasEnded = true;
		this.Network_winningDuckIndex = this.ducks.IndexOf(duck);
		double multiplier = (this._placedBetIndex == this._winningDuckIndex) ? (4.0 * base.EstimatedValue) : 0.0;
		Dictionary<string, object> gameSpecificData = new Dictionary<string, object>
		{
			{
				"winningDuckIndex",
				this._winningDuckIndex
			},
			{
				"betDuckIndex",
				this._placedBetIndex
			}
		};
		this.Payout(multiplier, ChangeType.GameResult, gameSpecificData, -1L);
		this.RpcDuckMusic(false);
		base.StartCoroutine(this.ResetGameRoutine());
		return true;
	}

	// Token: 0x0600023D RID: 573 RVA: 0x0000C679 File Offset: 0x0000A879
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		foreach (DuckRaceDuck duckRaceDuck in this.ducks)
		{
			duckRaceDuck.ServerReturn();
		}
		yield return new WaitForSeconds(0.7f);
		this.RpcSetCrown(false);
		yield return new WaitForSeconds(0.3f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x0600023E RID: 574 RVA: 0x0000C688 File Offset: 0x0000A888
	protected override void ResetGame()
	{
		base.ResetGame();
		this.Network_winningDuckIndex = -1;
		this.hasEnded = false;
		this.RpcSetCrown(false);
		foreach (DuckRaceDuck duckRaceDuck in this.ducks)
		{
			duckRaceDuck.ResetDuck();
		}
	}

	// Token: 0x0600023F RID: 575 RVA: 0x0000C6F4 File Offset: 0x0000A8F4
	[ClientRpc]
	private void RpcPlaceBetFeedback(int duckIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(duckIndex);
		this.SendRPCInternal("System.Void DuckRace::RpcPlaceBetFeedback(System.Int32)", 638818872, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000240 RID: 576 RVA: 0x0000C730 File Offset: 0x0000A930
	private void PlaceBetFeedback(int duckIndex)
	{
		int num = duckIndex * 2;
		int num2 = num + 3;
		for (int i = 0; i < this.selectedDuckLights.Count; i++)
		{
			Renderer renderer = this.selectedDuckLights[i];
			MaterialPropertyBlock materialPropertyBlock = this._sdmpbs[i];
			renderer.GetPropertyBlock(materialPropertyBlock, 1);
			if (i >= num && i <= num2)
			{
				materialPropertyBlock.SetColor("_EmissionColor", Color.greenYellow * 3f);
			}
			else
			{
				materialPropertyBlock.SetColor("_EmissionColor", Color.black);
			}
			renderer.SetPropertyBlock(materialPropertyBlock, 1);
		}
	}

	// Token: 0x06000241 RID: 577 RVA: 0x0000C7B8 File Offset: 0x0000A9B8
	[ClientRpc]
	private void RpcSetCrown(bool active)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(active);
		this.SendRPCInternal("System.Void DuckRace::RpcSetCrown(System.Boolean)", -820960290, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000242 RID: 578 RVA: 0x0000C7F4 File Offset: 0x0000A9F4
	private void SetCrownPosition()
	{
		this.crown.position = Vector3.Lerp(this.crown.position, this.ducks[this._winningDuckIndex].transform.position, 20f * Time.deltaTime);
	}

	// Token: 0x06000243 RID: 579 RVA: 0x0000C844 File Offset: 0x0000AA44
	[ClientRpc]
	private void RpcDuckMusic(bool play)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(play);
		this.SendRPCInternal("System.Void DuckRace::RpcDuckMusic(System.Boolean)", -923557929, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000245 RID: 581 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000041 RID: 65
	// (get) Token: 0x06000246 RID: 582 RVA: 0x0000C898 File Offset: 0x0000AA98
	// (set) Token: 0x06000247 RID: 583 RVA: 0x0000C8AB File Offset: 0x0000AAAB
	public int Network_winningDuckIndex
	{
		get
		{
			return this._winningDuckIndex;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this._winningDuckIndex, 8UL, null);
		}
	}

	// Token: 0x06000248 RID: 584 RVA: 0x0000C8C5 File Offset: 0x0000AAC5
	protected void UserCode_ServerPlaceBet__Int32(int duckIndex)
	{
		if (this.isPlaying)
		{
			return;
		}
		this._placedBetIndex = duckIndex;
		this.RpcPlaceBetFeedback(duckIndex);
	}

	// Token: 0x06000249 RID: 585 RVA: 0x0000C8DE File Offset: 0x0000AADE
	protected static void InvokeUserCode_ServerPlaceBet__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command ServerPlaceBet called on client.");
			return;
		}
		((DuckRace)obj).UserCode_ServerPlaceBet__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600024A RID: 586 RVA: 0x0000C907 File Offset: 0x0000AB07
	protected void UserCode_RpcPlaceBetFeedback__Int32(int duckIndex)
	{
		this.PlaceBetFeedback(duckIndex);
	}

	// Token: 0x0600024B RID: 587 RVA: 0x0000C910 File Offset: 0x0000AB10
	protected static void InvokeUserCode_RpcPlaceBetFeedback__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceBetFeedback called on server.");
			return;
		}
		((DuckRace)obj).UserCode_RpcPlaceBetFeedback__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600024C RID: 588 RVA: 0x0000C939 File Offset: 0x0000AB39
	protected void UserCode_RpcSetCrown__Boolean(bool active)
	{
		this.crown.DOScale(active ? Vector3.one : Vector3.zero, 0.3f).SetEase(Ease.InBack);
	}

	// Token: 0x0600024D RID: 589 RVA: 0x0000C962 File Offset: 0x0000AB62
	protected static void InvokeUserCode_RpcSetCrown__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCrown called on server.");
			return;
		}
		((DuckRace)obj).UserCode_RpcSetCrown__Boolean(reader.ReadBool());
	}

	// Token: 0x0600024E RID: 590 RVA: 0x0000C98C File Offset: 0x0000AB8C
	protected void UserCode_RpcDuckMusic__Boolean(bool play)
	{
		if (play)
		{
			this._duckMusicInstance = RuntimeManager.CreateInstance(this.duckMusic);
			this._duckMusicInstance.set3DAttributes(base.transform.position.To3DAttributes());
			this._duckMusicInstance.start();
			return;
		}
		this._duckMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		this._duckMusicInstance.release();
	}

	// Token: 0x0600024F RID: 591 RVA: 0x0000C9EF File Offset: 0x0000ABEF
	protected static void InvokeUserCode_RpcDuckMusic__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDuckMusic called on server.");
			return;
		}
		((DuckRace)obj).UserCode_RpcDuckMusic__Boolean(reader.ReadBool());
	}

	// Token: 0x06000250 RID: 592 RVA: 0x0000CA18 File Offset: 0x0000AC18
	static DuckRace()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(DuckRace), "System.Void DuckRace::ServerPlaceBet(System.Int32)", new RemoteCallDelegate(DuckRace.InvokeUserCode_ServerPlaceBet__Int32), false);
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRace), "System.Void DuckRace::RpcPlaceBetFeedback(System.Int32)", new RemoteCallDelegate(DuckRace.InvokeUserCode_RpcPlaceBetFeedback__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRace), "System.Void DuckRace::RpcSetCrown(System.Boolean)", new RemoteCallDelegate(DuckRace.InvokeUserCode_RpcSetCrown__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRace), "System.Void DuckRace::RpcDuckMusic(System.Boolean)", new RemoteCallDelegate(DuckRace.InvokeUserCode_RpcDuckMusic__Boolean));
	}

	// Token: 0x06000251 RID: 593 RVA: 0x0000CAA8 File Offset: 0x0000ACA8
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this._winningDuckIndex);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteVarInt(this._winningDuckIndex);
		}
	}

	// Token: 0x06000252 RID: 594 RVA: 0x0000CB00 File Offset: 0x0000AD00
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._winningDuckIndex, null, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._winningDuckIndex, null, reader.ReadVarInt());
		}
	}

	// Token: 0x040001D0 RID: 464
	[Header("References")]
	public Transform startPoint;

	// Token: 0x040001D1 RID: 465
	public Transform endPoint;

	// Token: 0x040001D2 RID: 466
	[SerializeField]
	private Transform crown;

	// Token: 0x040001D3 RID: 467
	[SerializeField]
	private List<DuckRaceDuck> ducks;

	// Token: 0x040001D4 RID: 468
	[SerializeField]
	private List<Renderer> selectedDuckLights;

	// Token: 0x040001D5 RID: 469
	[Header("SFX")]
	[SerializeField]
	private EventReference duckMusic;

	// Token: 0x040001D6 RID: 470
	private EventInstance _duckMusicInstance;

	// Token: 0x040001D7 RID: 471
	private List<MaterialPropertyBlock> _sdmpbs = new List<MaterialPropertyBlock>();

	// Token: 0x040001D8 RID: 472
	public bool hasEnded;

	// Token: 0x040001D9 RID: 473
	private int _placedBetIndex;

	// Token: 0x040001DA RID: 474
	[SyncVar]
	[SerializeField]
	private int _winningDuckIndex = -1;
}
