using System;
using System.Collections.Generic;
using Dissonance.Networking;
using Dissonance.Networking.Server;
using JetBrains.Annotations;
using Mirror;

namespace Dissonance.Integrations.MirrorIgnorance
{
	// Token: 0x0200038D RID: 909
	public class MirrorIgnoranceServer : BaseServer<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn>
	{
		// Token: 0x06001DDD RID: 7645 RVA: 0x00080667 File Offset: 0x0007E867
		public MirrorIgnoranceServer([NotNull] MirrorIgnoranceCommsNetwork network)
		{
			if (network == null)
			{
				throw new ArgumentNullException("network");
			}
			this._network = network;
		}

		// Token: 0x06001DDE RID: 7646 RVA: 0x00080695 File Offset: 0x0007E895
		public override void Connect()
		{
			NetworkServer.ReplaceHandler<DissonanceNetworkMessage>(new Action<NetworkConnectionToClient, DissonanceNetworkMessage>(this.OnMessageReceived), true);
			base.Connect();
		}

		// Token: 0x06001DDF RID: 7647 RVA: 0x000806B0 File Offset: 0x0007E8B0
		private void OnMessageReceived(NetworkConnection source, DissonanceNetworkMessage msg)
		{
			using (msg)
			{
				base.NetworkReceivedPacket(new MirrorConn(source), msg.Data);
			}
		}

		// Token: 0x06001DE0 RID: 7648 RVA: 0x000806F4 File Offset: 0x0007E8F4
		protected override void AddClient([NotNull] ClientInfo<MirrorConn> client)
		{
			base.AddClient(client);
			if (client.PlayerName != this._network.PlayerName)
			{
				this._addedConnections.Add(client.Connection.Connection);
			}
		}

		// Token: 0x06001DE1 RID: 7649 RVA: 0x0008072B File Offset: 0x0007E92B
		public override void Disconnect()
		{
			base.Disconnect();
			NetworkServer.ReplaceHandler<DissonanceNetworkMessage>(new Action<DissonanceNetworkMessage>(MirrorIgnoranceCommsNetwork.NullMessageReceivedHandler), true);
		}

		// Token: 0x06001DE2 RID: 7650 RVA: 0x000048A7 File Offset: 0x00002AA7
		protected override void ReadMessages()
		{
		}

		// Token: 0x06001DE3 RID: 7651 RVA: 0x00080748 File Offset: 0x0007E948
		public override ServerState Update()
		{
			for (int i = this._addedConnections.Count - 1; i >= 0; i--)
			{
				if (!MirrorIgnoranceServer.IsConnected(this._addedConnections[i]))
				{
					base.ClientDisconnected(new MirrorConn(this._addedConnections[i]));
					this._addedConnections.RemoveAt(i);
				}
			}
			return base.Update();
		}

		// Token: 0x06001DE4 RID: 7652 RVA: 0x000807AC File Offset: 0x0007E9AC
		private static bool IsConnected([NotNull] NetworkConnection conn)
		{
			NetworkConnectionToClient networkConnectionToClient = (NetworkConnectionToClient)conn;
			return conn.isReady && NetworkServer.connections.ContainsKey(networkConnectionToClient.connectionId);
		}

		// Token: 0x06001DE5 RID: 7653 RVA: 0x000807DA File Offset: 0x0007E9DA
		protected override void SendReliable(MirrorConn connection, ArraySegment<byte> packet)
		{
			if (!this.Send(packet, connection, 0))
			{
				base.FatalError("Failed to send reliable packet (unknown Mirror error)");
			}
		}

		// Token: 0x06001DE6 RID: 7654 RVA: 0x000807F2 File Offset: 0x0007E9F2
		protected override void SendUnreliable(MirrorConn connection, ArraySegment<byte> packet)
		{
			this.Send(packet, connection, 1);
		}

		// Token: 0x06001DE7 RID: 7655 RVA: 0x00080800 File Offset: 0x0007EA00
		private bool Send(ArraySegment<byte> packet, MirrorConn connection, byte channel)
		{
			if (this._network.PreprocessPacketToClient(packet, connection))
			{
				return true;
			}
			if (!MirrorIgnoranceServer.IsConnected(connection.Connection))
			{
				return true;
			}
			if (connection.Connection == null)
			{
				this.Log.Error("Cannot send to a null destination");
				return false;
			}
			connection.Connection.Send<DissonanceNetworkMessage>(new DissonanceNetworkMessage(packet), (int)channel);
			return true;
		}

		// Token: 0x04001430 RID: 5168
		[NotNull]
		private readonly MirrorIgnoranceCommsNetwork _network;

		// Token: 0x04001431 RID: 5169
		private readonly List<NetworkConnection> _addedConnections = new List<NetworkConnection>();
	}
}
