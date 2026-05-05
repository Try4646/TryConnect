using System;
using UnityEngine;

namespace Extensions
{
	// Token: 0x0200039C RID: 924
	public class MonoSingleton<T> : MonoBehaviour where T : Component
	{
		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06001E2C RID: 7724 RVA: 0x000820A4 File Offset: 0x000802A4
		// (set) Token: 0x06001E2D RID: 7725 RVA: 0x000820C7 File Offset: 0x000802C7
		public static T Instance
		{
			get
			{
				if (MonoSingleton<T>._instance == null)
				{
					MonoSingleton<T>._instance = Object.FindAnyObjectByType<T>();
				}
				return MonoSingleton<T>._instance;
			}
			protected set
			{
				MonoSingleton<T>._instance = value;
			}
		}

		// Token: 0x06001E2E RID: 7726 RVA: 0x000820D0 File Offset: 0x000802D0
		protected void Awake()
		{
			if (MonoSingleton<T>._instance != null && MonoSingleton<T>._instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				MonoSingleton<T>._instance = (this as T);
			}
			this.OnAwake();
		}

		// Token: 0x06001E2F RID: 7727 RVA: 0x000048A7 File Offset: 0x00002AA7
		protected virtual void OnAwake()
		{
		}

		// Token: 0x0400144B RID: 5195
		private static T _instance;
	}
}
