using System;
using Dissonance.Extensions;
using Mirror;

namespace Dissonance.Integrations.MirrorIgnorance
{
	// Token: 0x0200038B RID: 907
	internal struct DissonanceNetworkMessage : NetworkMessage, IDisposable
	{
		// Token: 0x06001DC0 RID: 7616 RVA: 0x00080202 File Offset: 0x0007E402
		public DissonanceNetworkMessage(ArraySegment<byte> packet)
		{
			this.Data = packet.CopyToSegment(DissonanceNetworkMessageExtensions.SerializationBuffers.Get(), 0);
		}

		// Token: 0x06001DC1 RID: 7617 RVA: 0x0008021C File Offset: 0x0007E41C
		public void Dispose()
		{
			byte[] array = this.Data.Array;
			if (array != null && array.Length == 1024)
			{
				DissonanceNetworkMessageExtensions.SerializationBuffers.Put(array);
				this.Data = new ArraySegment<byte>(Array.Empty<byte>(), 0, 0);
			}
		}

		// Token: 0x0400142B RID: 5163
		public ArraySegment<byte> Data;
	}
}
