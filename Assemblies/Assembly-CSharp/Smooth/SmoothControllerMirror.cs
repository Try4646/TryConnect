using System;
using Mirror;
using UnityEngine;

namespace Smooth
{
	// Token: 0x0200035E RID: 862
	public class SmoothControllerMirror : MonoBehaviour
	{
		// Token: 0x06001C69 RID: 7273 RVA: 0x00078FA3 File Offset: 0x000771A3
		private void Awake()
		{
			SmoothControllerMirror.RegisterHandlers();
		}

		// Token: 0x06001C6A RID: 7274 RVA: 0x00078FAA File Offset: 0x000771AA
		private void Update()
		{
			if ((NetworkServer.active || NetworkClient.active) && !SmoothControllerMirror.isHandlerRegistered)
			{
				SmoothControllerMirror.RegisterHandlers();
			}
			if (!NetworkServer.active && !NetworkClient.active && SmoothControllerMirror.isHandlerRegistered)
			{
				SmoothControllerMirror.isHandlerRegistered = false;
			}
		}

		// Token: 0x06001C6B RID: 7275 RVA: 0x00078FE1 File Offset: 0x000771E1
		public static void RegisterHandlers()
		{
			NetworkServer.ReplaceHandler<NetworkStateMirror>(new Action<NetworkConnectionToClient, NetworkStateMirror>(SmoothSyncMirror.HandleSyncServer), true);
			NetworkClient.ReplaceHandler<NetworkStateMirror>(new Action<NetworkStateMirror>(SmoothSyncMirror.HandleSyncClient), true);
			SmoothControllerMirror.isHandlerRegistered = true;
		}

		// Token: 0x040012CD RID: 4813
		public static bool isHandlerRegistered;
	}
}
