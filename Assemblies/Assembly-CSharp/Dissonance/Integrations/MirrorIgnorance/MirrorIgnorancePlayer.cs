using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

namespace Dissonance.Integrations.MirrorIgnorance
{
	// Token: 0x0200038C RID: 908
	[RequireComponent(typeof(NetworkIdentity))]
	public class MirrorIgnorancePlayer : NetworkBehaviour, IDissonancePlayer
	{
		// Token: 0x170002BA RID: 698
		// (get) Token: 0x06001DC2 RID: 7618 RVA: 0x0008025F File Offset: 0x0007E45F
		// (set) Token: 0x06001DC3 RID: 7619 RVA: 0x00080267 File Offset: 0x0007E467
		public bool IsTracking { get; private set; }

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x06001DC4 RID: 7620 RVA: 0x00080270 File Offset: 0x0007E470
		public string PlayerId
		{
			get
			{
				return this._playerId;
			}
		}

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x06001DC5 RID: 7621 RVA: 0x00080278 File Offset: 0x0007E478
		public Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06001DC6 RID: 7622 RVA: 0x00080285 File Offset: 0x0007E485
		public Quaternion Rotation
		{
			get
			{
				return base.transform.rotation;
			}
		}

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x06001DC7 RID: 7623 RVA: 0x00080292 File Offset: 0x0007E492
		public NetworkPlayerType Type
		{
			get
			{
				if (this._comms == null || this._playerId == null)
				{
					return NetworkPlayerType.Unknown;
				}
				if (!this._comms.LocalPlayerName.Equals(this._playerId))
				{
					return NetworkPlayerType.Remote;
				}
				return NetworkPlayerType.Local;
			}
		}

		// Token: 0x06001DC8 RID: 7624 RVA: 0x000802C7 File Offset: 0x0007E4C7
		public void OnDestroy()
		{
			if (this._comms != null)
			{
				this._comms.LocalPlayerNameChanged -= this.SetPlayerName;
			}
		}

		// Token: 0x06001DC9 RID: 7625 RVA: 0x000802EE File Offset: 0x0007E4EE
		public void OnEnable()
		{
			this._comms = Object.FindFirstObjectByType<DissonanceComms>();
		}

		// Token: 0x06001DCA RID: 7626 RVA: 0x000802FB File Offset: 0x0007E4FB
		public void OnDisable()
		{
			if (this.IsTracking)
			{
				this.StopTracking();
			}
		}

		// Token: 0x06001DCB RID: 7627 RVA: 0x0008030C File Offset: 0x0007E50C
		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();
			DissonanceComms dissonanceComms = Object.FindFirstObjectByType<DissonanceComms>();
			if (dissonanceComms == null)
			{
				throw MirrorIgnorancePlayer.Log.CreateUserErrorException("cannot find DissonanceComms component in scene", "not placing a DissonanceComms component on a game object in the scene", "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-MirrorIgnorance/", "2D90A6C3-5F2B-4859-994C-EBBDDD4A10F4");
			}
			if (dissonanceComms.LocalPlayerName != null)
			{
				this.SetPlayerName(dissonanceComms.LocalPlayerName);
			}
			dissonanceComms.LocalPlayerNameChanged += this.SetPlayerName;
		}

		// Token: 0x06001DCC RID: 7628 RVA: 0x00080373 File Offset: 0x0007E573
		private void SetPlayerName(string playerName)
		{
			if (this.IsTracking)
			{
				this.StopTracking();
			}
			this.Network_playerId = playerName;
			this.StartTracking();
			if (base.isLocalPlayer)
			{
				this.CmdSetPlayerName(playerName);
			}
		}

		// Token: 0x06001DCD RID: 7629 RVA: 0x0008039F File Offset: 0x0007E59F
		public override void OnStartClient()
		{
			base.OnStartClient();
			if (!string.IsNullOrEmpty(this.PlayerId))
			{
				this.StartTracking();
			}
		}

		// Token: 0x06001DCE RID: 7630 RVA: 0x000803BC File Offset: 0x0007E5BC
		[Command]
		private void CmdSetPlayerName(string playerName)
		{
			NetworkWriterPooled writer = NetworkWriterPool.Get();
			writer.WriteString(playerName);
			base.SendCommandInternal("System.Void Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer::CmdSetPlayerName(System.String)", 2094266474, writer, 0, true);
			NetworkWriterPool.Return(writer);
		}

		// Token: 0x06001DCF RID: 7631 RVA: 0x000803F8 File Offset: 0x0007E5F8
		[ClientRpc]
		private void RpcSetPlayerName(string playerName)
		{
			NetworkWriterPooled writer = NetworkWriterPool.Get();
			writer.WriteString(playerName);
			this.SendRPCInternal("System.Void Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer::RpcSetPlayerName(System.String)", -1860767819, writer, 0, true);
			NetworkWriterPool.Return(writer);
		}

		// Token: 0x06001DD0 RID: 7632 RVA: 0x00080432 File Offset: 0x0007E632
		private void StartTracking()
		{
			if (this.IsTracking)
			{
				throw MirrorIgnorancePlayer.Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "31971B1F-52FD-4FCF-89E9-67A17A917921");
			}
			if (this._comms != null)
			{
				this._comms.TrackPlayerPosition(this);
				this.IsTracking = true;
			}
		}

		// Token: 0x06001DD1 RID: 7633 RVA: 0x00080472 File Offset: 0x0007E672
		private void StopTracking()
		{
			if (!this.IsTracking)
			{
				throw MirrorIgnorancePlayer.Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "C7CF0174-0667-4F07-88E3-800ED652142D");
			}
			if (this._comms != null)
			{
				this._comms.StopTracking(this);
				this.IsTracking = false;
			}
		}

		// Token: 0x06001DD3 RID: 7635 RVA: 0x000804B4 File Offset: 0x0007E6B4
		static MirrorIgnorancePlayer()
		{
			RemoteProcedureCalls.RegisterCommand(typeof(MirrorIgnorancePlayer), "System.Void Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer::CmdSetPlayerName(System.String)", new RemoteCallDelegate(MirrorIgnorancePlayer.InvokeUserCode_CmdSetPlayerName__String), true);
			RemoteProcedureCalls.RegisterRpc(typeof(MirrorIgnorancePlayer), "System.Void Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer::RpcSetPlayerName(System.String)", new RemoteCallDelegate(MirrorIgnorancePlayer.InvokeUserCode_RpcSetPlayerName__String));
		}

		// Token: 0x06001DD4 RID: 7636 RVA: 0x00002321 File Offset: 0x00000521
		public override bool Weaved()
		{
			return true;
		}

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x06001DD5 RID: 7637 RVA: 0x00080514 File Offset: 0x0007E714
		// (set) Token: 0x06001DD6 RID: 7638 RVA: 0x00080527 File Offset: 0x0007E727
		public string Network_playerId
		{
			get
			{
				return this._playerId;
			}
			[param: In]
			set
			{
				base.GeneratedSyncVarSetter<string>(value, ref this._playerId, 1UL, null);
			}
		}

		// Token: 0x06001DD7 RID: 7639 RVA: 0x00080541 File Offset: 0x0007E741
		protected void UserCode_CmdSetPlayerName__String(string playerName)
		{
			this.Network_playerId = playerName;
			this.RpcSetPlayerName(playerName);
		}

		// Token: 0x06001DD8 RID: 7640 RVA: 0x00080551 File Offset: 0x0007E751
		protected static void InvokeUserCode_CmdSetPlayerName__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSetPlayerName called on client.");
				return;
			}
			((MirrorIgnorancePlayer)obj).UserCode_CmdSetPlayerName__String(reader.ReadString());
		}

		// Token: 0x06001DD9 RID: 7641 RVA: 0x0008057A File Offset: 0x0007E77A
		protected void UserCode_RpcSetPlayerName__String(string playerName)
		{
			if (!base.isLocalPlayer)
			{
				this.SetPlayerName(playerName);
			}
		}

		// Token: 0x06001DDA RID: 7642 RVA: 0x0008058B File Offset: 0x0007E78B
		protected static void InvokeUserCode_RpcSetPlayerName__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcSetPlayerName called on server.");
				return;
			}
			((MirrorIgnorancePlayer)obj).UserCode_RpcSetPlayerName__String(reader.ReadString());
		}

		// Token: 0x06001DDB RID: 7643 RVA: 0x000805B4 File Offset: 0x0007E7B4
		public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
		{
			base.SerializeSyncVars(writer, forceAll);
			if (forceAll)
			{
				writer.WriteString(this._playerId);
				return;
			}
			writer.WriteVarULong(this.syncVarDirtyBits);
			if ((this.syncVarDirtyBits & 1UL) != 0UL)
			{
				writer.WriteString(this._playerId);
			}
		}

		// Token: 0x06001DDC RID: 7644 RVA: 0x0008060C File Offset: 0x0007E80C
		public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
		{
			base.DeserializeSyncVars(reader, initialState);
			if (initialState)
			{
				base.GeneratedSyncVarDeserialize<string>(ref this._playerId, null, reader.ReadString());
				return;
			}
			long num = (long)reader.ReadVarULong();
			if ((num & 1L) != 0L)
			{
				base.GeneratedSyncVarDeserialize<string>(ref this._playerId, null, reader.ReadString());
			}
		}

		// Token: 0x0400142C RID: 5164
		private static readonly Log Log = Logs.Create(LogCategory.Network, "Mirror Player Component");

		// Token: 0x0400142D RID: 5165
		private DissonanceComms _comms;

		// Token: 0x0400142F RID: 5167
		[SyncVar]
		private string _playerId;
	}
}
