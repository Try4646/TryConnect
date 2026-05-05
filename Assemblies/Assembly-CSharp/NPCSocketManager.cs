using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

// Token: 0x020001CD RID: 461
public class NPCSocketManager : MonoSingleton<NPCSocketManager>
{
	// Token: 0x06001092 RID: 4242 RVA: 0x00047171 File Offset: 0x00045371
	public void RegisterSocket(NPCSocket socket)
	{
		if (socket != null && !this.allSockets.Contains(socket))
		{
			this.allSockets.Add(socket);
		}
	}

	// Token: 0x06001093 RID: 4243 RVA: 0x00047196 File Offset: 0x00045396
	public void UnregisterSocket(NPCSocket socket)
	{
		if (socket != null)
		{
			this.allSockets.Remove(socket);
		}
	}

	// Token: 0x06001094 RID: 4244 RVA: 0x000471B0 File Offset: 0x000453B0
	public NPCSocket FindAvailableSocket(Vector3 npcPosition, float searchRadius, NPCSocketAction preferredAction = null)
	{
		NPCSocket result = null;
		float num = float.MinValue;
		foreach (NPCSocket npcsocket in this.allSockets)
		{
			if (!(npcsocket == null) && npcsocket.gameObject.activeInHierarchy && npcsocket.IsAvailable())
			{
				float num2 = Vector3.Distance(npcPosition, npcsocket.Position);
				if (num2 <= searchRadius)
				{
					float num3 = 1f / (num2 + 1f);
					if (preferredAction != null && npcsocket.Action == preferredAction)
					{
						num3 *= 2f;
					}
					if (num3 > num)
					{
						num = num3;
						result = npcsocket;
					}
				}
			}
		}
		return result;
	}

	// Token: 0x06001095 RID: 4245 RVA: 0x00047274 File Offset: 0x00045474
	public List<NPCSocket> GetSocketsByAction(NPCSocketAction action)
	{
		return (from s in this.allSockets
		where s != null && s.gameObject.activeInHierarchy && s.Action == action
		select s).ToList<NPCSocket>();
	}

	// Token: 0x04000ABC RID: 2748
	private List<NPCSocket> allSockets = new List<NPCSocket>();
}
