using System;
using System.Collections.Concurrent;
using UnityEngine;

// Token: 0x02000110 RID: 272
public class UnityMainThreadDispatcher : MonoBehaviour
{
	// Token: 0x06000B56 RID: 2902 RVA: 0x0002DD64 File Offset: 0x0002BF64
	public static void Enqueue(Action job)
	{
		if (UnityMainThreadDispatcher._inst == null)
		{
			Debug.LogError("⚠️  Add a UnityMainThreadDispatcher GameObject in the first scene!");
			return;
		}
		UnityMainThreadDispatcher._inst._jobs.Enqueue(job);
	}

	// Token: 0x06000B57 RID: 2903 RVA: 0x0002DD8E File Offset: 0x0002BF8E
	private void Awake()
	{
		if (UnityMainThreadDispatcher._inst != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		UnityMainThreadDispatcher._inst = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	// Token: 0x06000B58 RID: 2904 RVA: 0x0002DDBC File Offset: 0x0002BFBC
	private void Update()
	{
		Action action;
		while (this._jobs.TryDequeue(out action))
		{
			if (action != null)
			{
				action();
			}
		}
	}

	// Token: 0x04000706 RID: 1798
	private static UnityMainThreadDispatcher _inst;

	// Token: 0x04000707 RID: 1799
	private readonly ConcurrentQueue<Action> _jobs = new ConcurrentQueue<Action>();
}
