using System;
using Mirror;

namespace Dissonance.Integrations.MirrorIgnorance
{
	// Token: 0x02000388 RID: 904
	public readonly struct MirrorConn : IEquatable<MirrorConn>
	{
		// Token: 0x06001DB5 RID: 7605 RVA: 0x000800CA File Offset: 0x0007E2CA
		public MirrorConn(NetworkConnection connection)
		{
			this = default(MirrorConn);
			this.Connection = connection;
		}

		// Token: 0x06001DB6 RID: 7606 RVA: 0x000800DA File Offset: 0x0007E2DA
		public override int GetHashCode()
		{
			return this.Connection.GetHashCode();
		}

		// Token: 0x06001DB7 RID: 7607 RVA: 0x000800E7 File Offset: 0x0007E2E7
		public override string ToString()
		{
			return this.Connection.ToString();
		}

		// Token: 0x06001DB8 RID: 7608 RVA: 0x000800F4 File Offset: 0x0007E2F4
		public override bool Equals(object obj)
		{
			return obj != null && obj is MirrorConn && this.Equals((MirrorConn)obj);
		}

		// Token: 0x06001DB9 RID: 7609 RVA: 0x00080111 File Offset: 0x0007E311
		public bool Equals(MirrorConn other)
		{
			if (this.Connection == null)
			{
				return other.Connection == null;
			}
			return this.Connection.Equals(other.Connection);
		}

		// Token: 0x04001427 RID: 5159
		public readonly NetworkConnection Connection;
	}
}
