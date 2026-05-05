using System;
using System.Collections.Generic;
using Dissonance.Audio.Playback;
using Dissonance.Extensions;
using Dissonance.Networking;
using JetBrains.Annotations;
using UnityEngine;

namespace Dissonance.Integrations.Offline
{
	// Token: 0x02000384 RID: 900
	public class OfflineCommsNetwork : MonoBehaviour, ICommsNetwork
	{
		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06001D83 RID: 7555 RVA: 0x0007F6D8 File Offset: 0x0007D8D8
		// (set) Token: 0x06001D84 RID: 7556 RVA: 0x0007F6E0 File Offset: 0x0007D8E0
		public int LoopbackPacketCount { get; private set; }

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06001D85 RID: 7557 RVA: 0x0006A586 File Offset: 0x00068786
		public ConnectionStatus Status
		{
			get
			{
				return ConnectionStatus.Connected;
			}
		}

		// Token: 0x06001D86 RID: 7558 RVA: 0x0007F6EC File Offset: 0x0007D8EC
		public void Initialize(string playerName, Rooms rooms, PlayerChannels playerChannels, RoomChannels roomChannels, CodecSettings codecSettings)
		{
			this._codecSettings = new CodecSettings?(codecSettings);
			this._loopbackChannels.Add(new RemoteChannel("Loopback", ChannelType.Room, new PlaybackOptions(false, 1f, ChannelPriority.Default)));
			roomChannels.OpenedChannel += this.BeginLoopback;
			roomChannels.ClosedChannel += this.EndLoopback;
		}

		// Token: 0x06001D87 RID: 7559 RVA: 0x0007F74E File Offset: 0x0007D94E
		private void BeginLoopback(RoomName channel, ChannelProperties props)
		{
			this._loopbackActive = true;
		}

		// Token: 0x06001D88 RID: 7560 RVA: 0x0007F757 File Offset: 0x0007D957
		private void EndLoopback(RoomName channel, ChannelProperties props)
		{
			if (this._sentStartedSpeakingEvent)
			{
				Action<string> playerStoppedSpeaking = this.PlayerStoppedSpeaking;
				if (playerStoppedSpeaking != null)
				{
					playerStoppedSpeaking("Loopback");
				}
			}
			this._loopbackQueue.Clear();
			this._sentStartedSpeakingEvent = false;
			this._loopbackActive = false;
			this._loopbackSequenceNumber = 0U;
		}

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x06001D89 RID: 7561 RVA: 0x0006A586 File Offset: 0x00068786
		public NetworkMode Mode
		{
			get
			{
				return NetworkMode.Client;
			}
		}

		// Token: 0x1400002E RID: 46
		// (add) Token: 0x06001D8A RID: 7562 RVA: 0x0007F798 File Offset: 0x0007D998
		// (remove) Token: 0x06001D8B RID: 7563 RVA: 0x0007F7D0 File Offset: 0x0007D9D0
		public event Action<string, CodecSettings> PlayerJoined;

		// Token: 0x1400002F RID: 47
		// (add) Token: 0x06001D8C RID: 7564 RVA: 0x0007F808 File Offset: 0x0007DA08
		// (remove) Token: 0x06001D8D RID: 7565 RVA: 0x0007F840 File Offset: 0x0007DA40
		public event Action<VoicePacket> VoicePacketReceived;

		// Token: 0x14000030 RID: 48
		// (add) Token: 0x06001D8E RID: 7566 RVA: 0x0007F878 File Offset: 0x0007DA78
		// (remove) Token: 0x06001D8F RID: 7567 RVA: 0x0007F8B0 File Offset: 0x0007DAB0
		public event Action<string> PlayerStartedSpeaking;

		// Token: 0x14000031 RID: 49
		// (add) Token: 0x06001D90 RID: 7568 RVA: 0x0007F8E8 File Offset: 0x0007DAE8
		// (remove) Token: 0x06001D91 RID: 7569 RVA: 0x0007F920 File Offset: 0x0007DB20
		public event Action<string> PlayerStoppedSpeaking;

		// Token: 0x14000032 RID: 50
		// (add) Token: 0x06001D92 RID: 7570 RVA: 0x0007F958 File Offset: 0x0007DB58
		// (remove) Token: 0x06001D93 RID: 7571 RVA: 0x0007F990 File Offset: 0x0007DB90
		public event Action<NetworkMode> ModeChanged;

		// Token: 0x14000033 RID: 51
		// (add) Token: 0x06001D94 RID: 7572 RVA: 0x0007F9C8 File Offset: 0x0007DBC8
		// (remove) Token: 0x06001D95 RID: 7573 RVA: 0x0007FA00 File Offset: 0x0007DC00
		public event Action<string> PlayerLeft;

		// Token: 0x14000034 RID: 52
		// (add) Token: 0x06001D96 RID: 7574 RVA: 0x0007FA38 File Offset: 0x0007DC38
		// (remove) Token: 0x06001D97 RID: 7575 RVA: 0x0007FA70 File Offset: 0x0007DC70
		public event Action<TextMessage> TextPacketReceived;

		// Token: 0x14000035 RID: 53
		// (add) Token: 0x06001D98 RID: 7576 RVA: 0x0007FAA8 File Offset: 0x0007DCA8
		// (remove) Token: 0x06001D99 RID: 7577 RVA: 0x0007FAE0 File Offset: 0x0007DCE0
		public event Action<RoomEvent> PlayerEnteredRoom;

		// Token: 0x14000036 RID: 54
		// (add) Token: 0x06001D9A RID: 7578 RVA: 0x0007FB18 File Offset: 0x0007DD18
		// (remove) Token: 0x06001D9B RID: 7579 RVA: 0x0007FB50 File Offset: 0x0007DD50
		public event Action<RoomEvent> PlayerExitedRoom;

		// Token: 0x06001D9C RID: 7580 RVA: 0x0007FB88 File Offset: 0x0007DD88
		public void SendVoice(ArraySegment<byte> data)
		{
			if (!this._loopbackActive)
			{
				return;
			}
			ArraySegment<byte> arraySegment = data.CopyToSegment((this._bufferPool.Count > 0) ? this._bufferPool.Dequeue() : new byte[1024], 0);
			int loopbackPacketCount = this.LoopbackPacketCount;
			this.LoopbackPacketCount = loopbackPacketCount + 1;
			Queue<VoicePacket> loopbackQueue = this._loopbackQueue;
			string senderPlayerId = "Loopback";
			ChannelPriority priority = ChannelPriority.Default;
			float ampMul = 1f;
			bool positional = false;
			ArraySegment<byte> encodedAudioFrame = arraySegment;
			uint loopbackSequenceNumber = this._loopbackSequenceNumber;
			this._loopbackSequenceNumber = loopbackSequenceNumber + 1U;
			loopbackQueue.Enqueue(new VoicePacket(senderPlayerId, priority, ampMul, positional, encodedAudioFrame, loopbackSequenceNumber, this._loopbackChannels));
		}

		// Token: 0x06001D9D RID: 7581 RVA: 0x000048A7 File Offset: 0x00002AA7
		public void SendText([CanBeNull] string data, ChannelType recipientType, string recipientId)
		{
		}

		// Token: 0x06001D9E RID: 7582 RVA: 0x0007FC0F File Offset: 0x0007DE0F
		private void Update()
		{
			this.JoinFakePlayer();
			if (this._playerJoined)
			{
				this.PumpLoopback();
			}
		}

		// Token: 0x06001D9F RID: 7583 RVA: 0x0007FC28 File Offset: 0x0007DE28
		private void JoinFakePlayer()
		{
			if (this._playerJoined)
			{
				return;
			}
			if (this._codecSettings == null)
			{
				return;
			}
			Action<string, CodecSettings> playerJoined = this.PlayerJoined;
			if (playerJoined != null)
			{
				playerJoined("Loopback", this._codecSettings.Value);
			}
			this._playerJoined = true;
		}

		// Token: 0x06001DA0 RID: 7584 RVA: 0x0007FC74 File Offset: 0x0007DE74
		private void PumpLoopback()
		{
			if (!this._loopbackActive)
			{
				return;
			}
			if (!this._sentStartedSpeakingEvent && this._loopbackQueue.Count < 5)
			{
				return;
			}
			if (!this._sentStartedSpeakingEvent)
			{
				Action<string> playerStartedSpeaking = this.PlayerStartedSpeaking;
				if (playerStartedSpeaking != null)
				{
					playerStartedSpeaking("Loopback");
				}
				this._sentStartedSpeakingEvent = true;
			}
			while (this._loopbackQueue.Count > 0)
			{
				VoicePacket obj = this._loopbackQueue.Dequeue();
				Action<VoicePacket> voicePacketReceived = this.VoicePacketReceived;
				if (voicePacketReceived != null)
				{
					voicePacketReceived(obj);
				}
				this._bufferPool.Enqueue(obj.EncodedAudioFrame.Array);
			}
		}

		// Token: 0x0400140E RID: 5134
		private bool _loopbackActive;

		// Token: 0x0400140F RID: 5135
		private bool _sentStartedSpeakingEvent;

		// Token: 0x04001410 RID: 5136
		private uint _loopbackSequenceNumber;

		// Token: 0x04001411 RID: 5137
		private readonly List<RemoteChannel> _loopbackChannels = new List<RemoteChannel>();

		// Token: 0x04001412 RID: 5138
		private readonly Queue<byte[]> _bufferPool = new Queue<byte[]>();

		// Token: 0x04001413 RID: 5139
		private readonly Queue<VoicePacket> _loopbackQueue = new Queue<VoicePacket>(128);

		// Token: 0x04001414 RID: 5140
		private bool _playerJoined;

		// Token: 0x04001415 RID: 5141
		private CodecSettings? _codecSettings;
	}
}
