using System;
using Mirror;

namespace Smooth
{
	// Token: 0x02000366 RID: 870
	public struct NetworkStateMirror : NetworkMessage
	{
		// Token: 0x06001CC9 RID: 7369 RVA: 0x0007BD83 File Offset: 0x00079F83
		public void copyFromSmoothSync(SmoothSyncMirror smoothSyncScript)
		{
			this.smoothSync = smoothSyncScript;
			this.state.copyFromSmoothSync(smoothSyncScript);
		}

		// Token: 0x04001357 RID: 4951
		public SmoothSyncMirror smoothSync;

		// Token: 0x04001358 RID: 4952
		public StateMirror state;
	}
}
