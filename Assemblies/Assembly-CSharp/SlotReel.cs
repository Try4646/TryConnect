using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200007F RID: 127
public class SlotReel : NetworkBehaviour
{
	// Token: 0x06000490 RID: 1168 RVA: 0x00014994 File Offset: 0x00012B94
	private void Awake()
	{
		this._symbolPosY = new float[this.symbols.Length];
		for (int i = 0; i < this.symbols.Length; i++)
		{
			this._symbolPosY[i] = this.symbols[i].rectTransform.anchoredPosition.y;
		}
		this._step = Mathf.Abs(this._symbolPosY[0] - this._symbolPosY[1]);
		float num = this._symbolPosY[0];
		float[] symbolPosY = this._symbolPosY;
		this._reelLenght = Mathf.Abs(num - symbolPosY[symbolPosY.Length - 1]);
		float[] symbolPosY2 = this._symbolPosY;
		this._botY = symbolPosY2[symbolPosY2.Length - 1];
	}

	// Token: 0x06000491 RID: 1169 RVA: 0x00014A34 File Offset: 0x00012C34
	[Server]
	public void ServerStartScrolling(float duration, int turnCount, int seed)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SlotReel::ServerStartScrolling(System.Single,System.Int32,System.Int32)' called when server was not active");
			return;
		}
		this.RpcStartScrolling(duration, turnCount, seed);
	}

	// Token: 0x06000492 RID: 1170 RVA: 0x00014A54 File Offset: 0x00012C54
	[ClientRpc]
	private void RpcStartScrolling(float duration, int turnCount, int seed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(duration);
		writer.WriteVarInt(turnCount);
		writer.WriteVarInt(seed);
		this.SendRPCInternal("System.Void SlotReel::RpcStartScrolling(System.Single,System.Int32,System.Int32)", -1735686638, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000493 RID: 1171 RVA: 0x00014AA2 File Offset: 0x00012CA2
	private IEnumerator ScrollRoutine(float duration, int turnCount)
	{
		float timer = 0f;
		float totalDistance = this._reelLenght * (float)turnCount;
		while (timer < duration)
		{
			float time = timer / duration;
			float offset = this.scrollCurve.Evaluate(time) * totalDistance;
			this.UpdateReel(offset);
			timer += Time.deltaTime;
			yield return null;
		}
		this.UpdateReel(totalDistance);
		yield break;
	}

	// Token: 0x06000494 RID: 1172 RVA: 0x00014AC0 File Offset: 0x00012CC0
	private void UpdateReel(float offset)
	{
		float num = offset % (this._reelLenght + this._step);
		int num2 = Mathf.FloorToInt(offset / (this._reelLenght + this._step));
		for (int i = 0; i < this.symbols.Length; i++)
		{
			float num3 = this._symbolPosY[i] - num;
			while (num3 < this._botY)
			{
				num3 += this._reelLenght + this._step;
				int num4 = (this._currentSeed * 31 + num2) * 31 + i;
				int num5 = (num4 ^ num4 >> 16) * 2146121005;
				int num6 = (num5 ^ num5 >> 15) * -2073254261;
				int num7 = ((num6 ^ num6 >> 16) & int.MaxValue) % this.atlas.Length;
				this.symbols[i].sprite = this.atlas[num7];
			}
			Vector2 anchoredPosition = this.symbols[i].rectTransform.anchoredPosition;
			anchoredPosition.y = num3;
			this.symbols[i].rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	// Token: 0x06000495 RID: 1173 RVA: 0x00014BB3 File Offset: 0x00012DB3
	[Server]
	public void ServerReset()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SlotReel::ServerReset()' called when server was not active");
			return;
		}
		this.RpcReset();
	}

	// Token: 0x06000496 RID: 1174 RVA: 0x00014BD0 File Offset: 0x00012DD0
	[ClientRpc]
	private void RpcReset()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void SlotReel::RpcReset()", 1040813013, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000497 RID: 1175 RVA: 0x00014C00 File Offset: 0x00012E00
	public List<int> GetResult()
	{
		List<int> list = new List<int>();
		this.symbols = (from s in this.symbols
		orderby s.rectTransform.anchoredPosition.y descending
		select s).ToArray<Image>();
		for (int i = 0; i < this.symbols.Length - 1; i++)
		{
			Sprite sprite = this.symbols[i].sprite;
			list.Add(Array.IndexOf<Sprite>(this.atlas, sprite));
		}
		return list;
	}

	// Token: 0x06000499 RID: 1177 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600049A RID: 1178 RVA: 0x00014C80 File Offset: 0x00012E80
	protected void UserCode_RpcStartScrolling__Single__Int32__Int32(float duration, int turnCount, int seed)
	{
		Random random = new Random(seed);
		this._currentSeed = random.Next();
		this.symbols = (from s in this.symbols
		orderby s.rectTransform.anchoredPosition.y descending
		select s).ToArray<Image>();
		for (int i = 0; i < this.symbols.Length; i++)
		{
			this._symbolPosY[i] = this.symbols[i].rectTransform.anchoredPosition.y;
		}
		if (this._spinRoutine != null)
		{
			base.StopCoroutine(this._spinRoutine);
		}
		this._spinRoutine = base.StartCoroutine(this.ScrollRoutine(duration, turnCount));
	}

	// Token: 0x0600049B RID: 1179 RVA: 0x00014D2F File Offset: 0x00012F2F
	protected static void InvokeUserCode_RpcStartScrolling__Single__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartScrolling called on server.");
			return;
		}
		((SlotReel)obj).UserCode_RpcStartScrolling__Single__Int32__Int32(reader.ReadFloat(), reader.ReadVarInt(), reader.ReadVarInt());
	}

	// Token: 0x0600049C RID: 1180 RVA: 0x00014D68 File Offset: 0x00012F68
	protected void UserCode_RpcReset()
	{
		if (this._spinRoutine != null)
		{
			base.StopCoroutine(this._spinRoutine);
		}
		this.symbols = (from s in this.symbols
		orderby s.rectTransform.anchoredPosition.y descending
		select s).ToArray<Image>();
		for (int i = 0; i < this.symbols.Length; i++)
		{
			Vector2 anchoredPosition = this.symbols[i].rectTransform.anchoredPosition;
			anchoredPosition.y = this._symbolPosY[i];
			this.symbols[i].rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	// Token: 0x0600049D RID: 1181 RVA: 0x00014E06 File Offset: 0x00013006
	protected static void InvokeUserCode_RpcReset(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReset called on server.");
			return;
		}
		((SlotReel)obj).UserCode_RpcReset();
	}

	// Token: 0x0600049E RID: 1182 RVA: 0x00014E2C File Offset: 0x0001302C
	static SlotReel()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(SlotReel), "System.Void SlotReel::RpcStartScrolling(System.Single,System.Int32,System.Int32)", new RemoteCallDelegate(SlotReel.InvokeUserCode_RpcStartScrolling__Single__Int32__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(SlotReel), "System.Void SlotReel::RpcReset()", new RemoteCallDelegate(SlotReel.InvokeUserCode_RpcReset));
	}

	// Token: 0x04000320 RID: 800
	public AnimationCurve scrollCurve;

	// Token: 0x04000321 RID: 801
	public Image[] symbols;

	// Token: 0x04000322 RID: 802
	public Sprite[] atlas;

	// Token: 0x04000323 RID: 803
	private float[] _symbolPosY;

	// Token: 0x04000324 RID: 804
	private float _step;

	// Token: 0x04000325 RID: 805
	private float _botY;

	// Token: 0x04000326 RID: 806
	private float _reelLenght;

	// Token: 0x04000327 RID: 807
	private int _currentSeed;

	// Token: 0x04000328 RID: 808
	private Coroutine _spinRoutine;
}
