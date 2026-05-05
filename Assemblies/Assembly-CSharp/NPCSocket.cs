using System;
using Extensions;
using UnityEngine;

// Token: 0x020001CB RID: 459
public class NPCSocket : MonoBehaviour
{
	// Token: 0x1700017C RID: 380
	// (get) Token: 0x0600107D RID: 4221 RVA: 0x00046FDD File Offset: 0x000451DD
	public NPCSocketAction Action
	{
		get
		{
			return this.action;
		}
	}

	// Token: 0x1700017D RID: 381
	// (get) Token: 0x0600107E RID: 4222 RVA: 0x00046FE5 File Offset: 0x000451E5
	public Vector3 Position
	{
		get
		{
			if (!(this.socketPosition != null))
			{
				return base.transform.position;
			}
			return this.socketPosition.position;
		}
	}

	// Token: 0x1700017E RID: 382
	// (get) Token: 0x0600107F RID: 4223 RVA: 0x0004700C File Offset: 0x0004520C
	public Vector3 Forward
	{
		get
		{
			if (!(this.socketPosition != null))
			{
				return base.transform.forward;
			}
			return this.socketPosition.forward;
		}
	}

	// Token: 0x1700017F RID: 383
	// (get) Token: 0x06001080 RID: 4224 RVA: 0x00047033 File Offset: 0x00045233
	public float UseRadius
	{
		get
		{
			return this.useRadius;
		}
	}

	// Token: 0x17000180 RID: 384
	// (get) Token: 0x06001081 RID: 4225 RVA: 0x0004703B File Offset: 0x0004523B
	public int MaxNPCs
	{
		get
		{
			return this.maxNPCs;
		}
	}

	// Token: 0x17000181 RID: 385
	// (get) Token: 0x06001082 RID: 4226 RVA: 0x00047043 File Offset: 0x00045243
	// (set) Token: 0x06001083 RID: 4227 RVA: 0x0004704B File Offset: 0x0004524B
	public int CurrentUsers { get; private set; }

	// Token: 0x06001084 RID: 4228 RVA: 0x00047054 File Offset: 0x00045254
	private void Awake()
	{
		if (this.socketPosition == null)
		{
			this.socketPosition = base.transform;
		}
		if (MonoSingleton<NPCSocketManager>.Instance != null)
		{
			MonoSingleton<NPCSocketManager>.Instance.RegisterSocket(this);
		}
	}

	// Token: 0x06001085 RID: 4229 RVA: 0x00047088 File Offset: 0x00045288
	private void OnDestroy()
	{
		if (MonoSingleton<NPCSocketManager>.Instance != null)
		{
			MonoSingleton<NPCSocketManager>.Instance.UnregisterSocket(this);
		}
	}

	// Token: 0x06001086 RID: 4230 RVA: 0x000470A2 File Offset: 0x000452A2
	public bool IsAvailable()
	{
		return this.CurrentUsers < this.maxNPCs;
	}

	// Token: 0x06001087 RID: 4231 RVA: 0x000470B4 File Offset: 0x000452B4
	public void Reserve()
	{
		int currentUsers = this.CurrentUsers;
		this.CurrentUsers = currentUsers + 1;
	}

	// Token: 0x06001088 RID: 4232 RVA: 0x000470D1 File Offset: 0x000452D1
	public void Release()
	{
		this.CurrentUsers = Mathf.Max(0, this.CurrentUsers - 1);
	}

	// Token: 0x06001089 RID: 4233 RVA: 0x000470E7 File Offset: 0x000452E7
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(this.Position, this.useRadius);
	}

	// Token: 0x04000AB2 RID: 2738
	[Header("Socket Settings")]
	[SerializeField]
	private NPCSocketAction action;

	// Token: 0x04000AB3 RID: 2739
	[SerializeField]
	private Transform socketPosition;

	// Token: 0x04000AB4 RID: 2740
	[SerializeField]
	private float useRadius = 1f;

	// Token: 0x04000AB5 RID: 2741
	[SerializeField]
	private int maxNPCs = 1;
}
