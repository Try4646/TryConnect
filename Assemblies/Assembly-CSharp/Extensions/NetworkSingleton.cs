using System;
using Mirror;
using UnityEngine;

namespace Extensions
{
	// Token: 0x0200039D RID: 925
	public class NetworkSingleton<T> : NetworkBehaviour where T : Component
	{
		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06001E31 RID: 7729 RVA: 0x00082124 File Offset: 0x00080324
		public static T Instance
		{
			get
			{
				if (NetworkSingleton<T>._instance == null && Application.isPlaying && !NetworkSingleton<T>._hadInstance)
				{
					NetworkSingleton<T>._instance = Object.FindAnyObjectByType<T>();
					if (NetworkSingleton<T>._instance == null)
					{
						Debug.LogError("No instance for: " + typeof(T).Name);
					}
					else
					{
						NetworkSingleton<T>._hadInstance = true;
					}
				}
				return NetworkSingleton<T>._instance;
			}
		}

		// Token: 0x06001E32 RID: 7730 RVA: 0x00082198 File Offset: 0x00080398
		protected void Awake()
		{
			if (NetworkSingleton<T>._instance != null && NetworkSingleton<T>._instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				NetworkSingleton<T>._instance = (this as T);
				NetworkSingleton<T>._hadInstance = true;
			}
			this.OnAwake();
		}

		// Token: 0x06001E33 RID: 7731 RVA: 0x000048A7 File Offset: 0x00002AA7
		protected virtual void OnAwake()
		{
		}

		// Token: 0x06001E34 RID: 7732 RVA: 0x000821F2 File Offset: 0x000803F2
		protected virtual void OnDestroy()
		{
			if (NetworkSingleton<T>._instance == this)
			{
				NetworkSingleton<T>._instance = default(T);
			}
		}

		// Token: 0x06001E36 RID: 7734 RVA: 0x00002321 File Offset: 0x00000521
		public override bool Weaved()
		{
			return true;
		}

		// Token: 0x0400144C RID: 5196
		private static T _instance;

		// Token: 0x0400144D RID: 5197
		private static bool _hadInstance;
	}
}
