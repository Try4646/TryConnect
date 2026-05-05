using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Token: 0x020002A8 RID: 680
public class MultiColliderTracker<T> : NetworkBehaviour where T : Component
{
	// Token: 0x17000223 RID: 547
	// (get) Token: 0x060017F8 RID: 6136 RVA: 0x0006598D File Offset: 0x00063B8D
	public IReadOnlyCollection<T> InsideObjects
	{
		get
		{
			return this._insideCounts.Keys;
		}
	}

	// Token: 0x060017F9 RID: 6137 RVA: 0x0006599C File Offset: 0x00063B9C
	protected virtual void OnTriggerEnter(Collider other)
	{
		if (!other.attachedRigidbody)
		{
			return;
		}
		T t;
		if (!other.attachedRigidbody.TryGetComponent<T>(out t))
		{
			return;
		}
		if (!this._insideCounts.ContainsKey(t))
		{
			this._insideCounts[t] = 0;
			this.OnObjectEntered(t);
		}
		Dictionary<T, int> insideCounts = this._insideCounts;
		T key = t;
		int num = insideCounts[key];
		insideCounts[key] = num + 1;
	}

	// Token: 0x060017FA RID: 6138 RVA: 0x00065A04 File Offset: 0x00063C04
	protected virtual void OnTriggerExit(Collider other)
	{
		if (!other.attachedRigidbody)
		{
			return;
		}
		T t;
		if (!other.attachedRigidbody.TryGetComponent<T>(out t))
		{
			return;
		}
		if (this._insideCounts.ContainsKey(t))
		{
			Dictionary<T, int> insideCounts = this._insideCounts;
			T key = t;
			int num = insideCounts[key];
			insideCounts[key] = num - 1;
			if (this._insideCounts[t] <= 0)
			{
				this._insideCounts.Remove(t);
				this.OnObjectExited(t);
			}
		}
	}

	// Token: 0x060017FB RID: 6139 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected virtual void OnObjectEntered(T other)
	{
	}

	// Token: 0x060017FC RID: 6140 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected virtual void OnObjectExited(T other)
	{
	}

	// Token: 0x060017FE RID: 6142 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000F7E RID: 3966
	private readonly Dictionary<T, int> _insideCounts = new Dictionary<T, int>();
}
