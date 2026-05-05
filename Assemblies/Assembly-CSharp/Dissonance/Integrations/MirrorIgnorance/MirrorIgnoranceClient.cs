using System;
using Dissonance.Networking;
using JetBrains.Annotations;
using Mirror;

namespace Dissonance.Integrations.MirrorIgnorance
{
	// Token: 0x02000385 RID: 901
	public class MirrorIgnoranceClient : BaseClient<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn>
	{
		// Token: 0x06001DA2 RID: 7586 RVA: 0x0007FD39 File Offset: 0x0007DF39
		public MirrorIgnoranceClient([NotNull] MirrorIgnoranceCommsNetwork network) : base(network)
		{
			if (network == null)
			{
				throw new ArgumentNullException("network");
			}
			this._network = network;
		}

		// Token: 0x06001DA3 RID: 7587 RVA: 0x0007FD5D File Offset: 0x0007DF5D
		public override void Connect()
		{
			if (!this._network.Mode.IsServerEnabled())
			{
				NetworkClient.ReplaceHandler<DissonanceNetworkMessage>(new Action<DissonanceNetworkMessage>(this.OnMessageReceived), true);
			}
			base.Connected();
		}

		// Token: 0x06001DA4 RID: 7588 RVA: 0x0007FD89 File Offset: 0x0007DF89
		public override void Disconnect()
		{
			if (!this._network.Mode.IsServerEnabled())
			{
				NetworkClient.ReplaceHandler<DissonanceNetworkMessage>(new Action<DissonanceNetworkMessage>(MirrorIgnoranceCommsNetwork.NullMessageReceivedHandler), true);
			}
			base.Disconnect();
		}

		// Token: 0x06001DA5 RID: 7589 RVA: 0x0007FDB8 File Offset: 0x0007DFB8
		private void OnMessageReceived(DissonanceNetworkMessage msg)
		{
			using (msg)
			{
				base.NetworkReceivedPacket(msg.Data);
			}
		}

		// Token: 0x06001DA6 RID: 7590 RVA: 0x000048A7 File Offset: 0x00002AA7
		protected override void ReadMessages()
		{
		}

		// Token: 0x06001DA7 RID: 7591 RVA: 0x0007FDF4 File Offset: 0x0007DFF4
		protected override void SendReliable(ArraySegment<byte> packet)
		{
			if (!this.Send(packet, 0))
			{
				base.FatalError("Failed to send reliable packet (unknown Mirror error)");
			}
		}

		// Token: 0x06001DA8 RID: 7592 RVA: 0x0007FE0B File Offset: 0x0007E00B
		protected override void SendUnreliable(ArraySegment<byte> packet)
		{
			this.Send(packet, 1);
		}

		// Token: 0x06001DA9 RID: 7593 RVA: 0x0007FE16 File Offset: 0x0007E016
		private bool Send(ArraySegment<byte> packet, byte channel)
		{
			if (this._network.PreprocessPacketToServer(packet))
			{
				return true;
			}
			NetworkClient.connection.Send<DissonanceNetworkMessage>(new DissonanceNetworkMessage(packet), (int)channel);
			return true;
		}

		// Token: 0x04001420 RID: 5152
		private readonly MirrorIgnoranceCommsNetwork _network;
	}
}
