using System;
using System.Collections.Generic;
using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking;
using Mirror;
using UnityEngine;

namespace Dissonance.Integrations.MirrorIgnorance
{
	// Token: 0x02000386 RID: 902
	[HelpURL("https://placeholder-software.co.uk/dissonance/docs/Basics/Quick-Start-MirrorIgnorance/")]
	public class MirrorIgnoranceCommsNetwork : BaseCommsNetwork<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn, Unit, Unit>
	{
		// Token: 0x06001DAA RID: 7594 RVA: 0x0007FE3A File Offset: 0x0007E03A
		protected override MirrorIgnoranceServer CreateServer(Unit details)
		{
			return new MirrorIgnoranceServer(this);
		}

		// Token: 0x06001DAB RID: 7595 RVA: 0x0007FE42 File Offset: 0x0007E042
		protected override MirrorIgnoranceClient CreateClient(Unit details)
		{
			return new MirrorIgnoranceClient(this);
		}

		// Token: 0x06001DAC RID: 7596 RVA: 0x0007FE4C File Offset: 0x0007E04C
		protected override void Update()
		{
			if (base.IsInitialized)
			{
				if (NetworkManager.singleton != null && NetworkManager.singleton.isNetworkActive && (NetworkServer.active || NetworkClient.active) && (!NetworkClient.active || (NetworkClient.connection != null && NetworkClient.connection.isReady)))
				{
					bool active = NetworkServer.active;
					bool active2 = NetworkClient.active;
					if (base.Mode.IsServerEnabled() != active || base.Mode.IsClientEnabled() != active2)
					{
						if (active && active2)
						{
							base.RunAsHost(Unit.None, Unit.None);
						}
						else if (active)
						{
							base.RunAsDedicatedServer(Unit.None);
						}
						else if (active2)
						{
							base.RunAsClient(Unit.None);
						}
					}
				}
				else if (base.Mode != NetworkMode.None)
				{
					base.Stop();
					this._loopbackQueue.Clear();
				}
				for (int i = 0; i < this._loopbackQueue.Count; i++)
				{
					MirrorIgnoranceClient client = base.Client;
					if (client != null)
					{
						client.NetworkReceivedPacket(this._loopbackQueue[i]);
					}
					this._loopbackBuffers.Put(this._loopbackQueue[i].Array);
				}
				this._loopbackQueue.Clear();
			}
			base.Update();
		}

		// Token: 0x06001DAD RID: 7597 RVA: 0x0007FF84 File Offset: 0x0007E184
		protected override void Initialize()
		{
			NetworkServer.ReplaceHandler<DissonanceNetworkMessage>(new Action<DissonanceNetworkMessage>(MirrorIgnoranceCommsNetwork.NullMessageReceivedHandler), true);
			base.Initialize();
		}

		// Token: 0x06001DAE RID: 7598 RVA: 0x0007FFA0 File Offset: 0x0007E1A0
		internal bool PreprocessPacketToClient(ArraySegment<byte> packet, MirrorConn destination)
		{
			if (base.Server == null)
			{
				throw this.Log.CreatePossibleBugException("server packet preprocessing running, but this peer is not a server", "8f9dc0a0-1b48-4a7f-9bb6-f767b2542ab1");
			}
			if (base.Client == null)
			{
				return false;
			}
			if (NetworkClient.connection != destination.Connection)
			{
				return false;
			}
			if (base.Client != null)
			{
				this._loopbackQueue.Add(packet.CopyToSegment(this._loopbackBuffers.Get(), 0));
			}
			return true;
		}

		// Token: 0x06001DAF RID: 7599 RVA: 0x0008000C File Offset: 0x0007E20C
		internal bool PreprocessPacketToServer(ArraySegment<byte> packet)
		{
			if (base.Client == null)
			{
				throw this.Log.CreatePossibleBugException("client packet processing running, but this peer is not a client", "dd75dce4-e85c-4bb3-96ec-3a3636cc4fbe");
			}
			if (base.Server == null)
			{
				return false;
			}
			base.Server.NetworkReceivedPacket(new MirrorConn(NetworkClient.connection), packet);
			return true;
		}

		// Token: 0x06001DB0 RID: 7600 RVA: 0x00080058 File Offset: 0x0007E258
		internal static void NullMessageReceivedHandler(DissonanceNetworkMessage msg)
		{
			if (Logs.GetLogLevel(LogCategory.Network) <= LogLevel.Trace)
			{
				Debug.Log("Discarding Dissonance network message");
			}
			msg.Dispose();
		}

		// Token: 0x04001421 RID: 5153
		internal const byte ReliableSequencedChannel = 0;

		// Token: 0x04001422 RID: 5154
		internal const byte UnreliableChannel = 1;

		// Token: 0x04001423 RID: 5155
		private readonly Dissonance.Datastructures.ConcurrentPool<byte[]> _loopbackBuffers = new Dissonance.Datastructures.ConcurrentPool<byte[]>(8, () => new byte[1024]);

		// Token: 0x04001424 RID: 5156
		private readonly List<ArraySegment<byte>> _loopbackQueue = new List<ArraySegment<byte>>();
	}
}
