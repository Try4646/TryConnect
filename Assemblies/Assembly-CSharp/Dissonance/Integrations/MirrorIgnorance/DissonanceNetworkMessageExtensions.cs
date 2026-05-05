using System;
using Dissonance.Datastructures;
using JetBrains.Annotations;
using Mirror;

namespace Dissonance.Integrations.MirrorIgnorance
{
	// Token: 0x02000389 RID: 905
	internal static class DissonanceNetworkMessageExtensions
	{
		// Token: 0x06001DBA RID: 7610 RVA: 0x00080138 File Offset: 0x0007E338
		public static void Serialize([NotNull] this NetworkWriter writer, DissonanceNetworkMessage value)
		{
			writer.WriteUShort((ushort)value.Data.Count);
			writer.WriteBytes(value.Data.Array, value.Data.Offset, value.Data.Count);
			DissonanceNetworkMessageExtensions.SerializationBuffers.Put(value.Data.Array);
		}

		// Token: 0x06001DBB RID: 7611 RVA: 0x00080198 File Offset: 0x0007E398
		public static DissonanceNetworkMessage Deserialize([NotNull] this NetworkReader reader)
		{
			byte[] array = DissonanceNetworkMessageExtensions.SerializationBuffers.Get();
			ushort num = reader.ReadUShort();
			for (int i = 0; i < (int)num; i++)
			{
				array[i] = reader.ReadByte();
			}
			return new DissonanceNetworkMessage(new ArraySegment<byte>(array, 0, (int)num));
		}

		// Token: 0x04001428 RID: 5160
		internal const int BufferLength = 1024;

		// Token: 0x04001429 RID: 5161
		internal static readonly Dissonance.Datastructures.ConcurrentPool<byte[]> SerializationBuffers = new Dissonance.Datastructures.ConcurrentPool<byte[]>(8, () => new byte[1024]);
	}
}
