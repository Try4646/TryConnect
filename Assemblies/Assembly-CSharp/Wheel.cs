using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMOD.Studio;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000097 RID: 151
public class Wheel : NetworkBehaviour
{
	// Token: 0x1700008C RID: 140
	// (get) Token: 0x06000587 RID: 1415 RVA: 0x0001892D File Offset: 0x00016B2D
	private WheelResult[] Results
	{
		get
		{
			if (this._results == null || this._results.Length == 0)
			{
				this._results = this.resultsParent.GetComponentsInChildren<WheelResult>();
			}
			return this._results;
		}
	}

	// Token: 0x14000002 RID: 2
	// (add) Token: 0x06000588 RID: 1416 RVA: 0x00018958 File Offset: 0x00016B58
	// (remove) Token: 0x06000589 RID: 1417 RVA: 0x00018990 File Offset: 0x00016B90
	public event Action<string> OnWheelStopped;

	// Token: 0x0600058A RID: 1418 RVA: 0x000189C8 File Offset: 0x00016BC8
	[Server]
	public virtual void SpinTheWheel(Random rng)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Wheel::SpinTheWheel(System.Random)' called when server was not active");
			return;
		}
		if (this._isSpinning)
		{
			return;
		}
		this._isSpinning = true;
		float num = (float)(rng.NextDouble() * 360.0);
		float num2 = (float)this.minTurnAmount * 360f + num;
		if (this.spinDirection)
		{
			num2 *= -1f;
		}
		this.RpcSpinWheel(num2, this.spinDuration);
		base.StartCoroutine(this.WaitAndStop());
	}

	// Token: 0x0600058B RID: 1419 RVA: 0x00018A46 File Offset: 0x00016C46
	protected IEnumerator WaitAndStop()
	{
		yield return new WaitForSeconds(this.spinDuration);
		this.StopTheWheel();
		yield break;
	}

	// Token: 0x0600058C RID: 1420 RVA: 0x00018A58 File Offset: 0x00016C58
	private void StopTheWheel()
	{
		string obj = this.FindResult();
		this.ResetWheel();
		Action<string> onWheelStopped = this.OnWheelStopped;
		if (onWheelStopped == null)
		{
			return;
		}
		onWheelStopped(obj);
	}

	// Token: 0x0600058D RID: 1421 RVA: 0x00018A83 File Offset: 0x00016C83
	public void ResetWheel()
	{
		this._isSpinning = false;
		this.RpcResetWheel();
	}

	// Token: 0x0600058E RID: 1422 RVA: 0x00018A94 File Offset: 0x00016C94
	[ClientRpc]
	private void RpcResetWheel()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Wheel::RpcResetWheel()", -1712800393, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600058F RID: 1423 RVA: 0x00018AC4 File Offset: 0x00016CC4
	private string FindResult()
	{
		Vector3 position = this.resultSelector.transform.position;
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < this.Results.Length; i++)
		{
			float sqrMagnitude = (this.Results[i].transform.position - position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				num2 = i;
			}
		}
		if (num2 >= 0)
		{
			this.RpcResultFeedback(num2);
		}
		if (num2 < 0)
		{
			return "Unknown";
		}
		return this.Results[num2].result;
	}

	// Token: 0x06000590 RID: 1424 RVA: 0x00018B4C File Offset: 0x00016D4C
	[ClientRpc]
	protected void RpcSpinWheel(float finalAngle, float duration)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(finalAngle);
		writer.WriteFloat(duration);
		this.SendRPCInternal("System.Void Wheel::RpcSpinWheel(System.Single,System.Single)", 617594292, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000591 RID: 1425 RVA: 0x00018B90 File Offset: 0x00016D90
	[ClientRpc]
	private void RpcResultFeedback(int index)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(index);
		this.SendRPCInternal("System.Void Wheel::RpcResultFeedback(System.Int32)", 1059369322, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000594 RID: 1428 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000595 RID: 1429 RVA: 0x00018C0F File Offset: 0x00016E0F
	protected void UserCode_RpcResetWheel()
	{
		this.sfxSpinInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		this.sfxSpinInstance.release();
	}

	// Token: 0x06000596 RID: 1430 RVA: 0x00018C2A File Offset: 0x00016E2A
	protected static void InvokeUserCode_RpcResetWheel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetWheel called on server.");
			return;
		}
		((Wheel)obj).UserCode_RpcResetWheel();
	}

	// Token: 0x06000597 RID: 1431 RVA: 0x00018C50 File Offset: 0x00016E50
	protected void UserCode_RpcSpinWheel__Single__Single(float finalAngle, float duration)
	{
		this.wheelTransform.DOLocalRotate(new Vector3(0f, 0f, -finalAngle), duration, RotateMode.FastBeyond360).SetEase(this.easing);
		this.sfxSpinInstance = RuntimeManager.CreateInstance(this.sfxSpinEvent);
		this.sfxSpinInstance.set3DAttributes(base.transform.position.To3DAttributes());
		this.sfxSpinInstance.setParameterByName("spinDuration", duration * 1000f, false);
		this.sfxSpinInstance.start();
	}

	// Token: 0x06000598 RID: 1432 RVA: 0x00018CD9 File Offset: 0x00016ED9
	protected static void InvokeUserCode_RpcSpinWheel__Single__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpinWheel called on server.");
			return;
		}
		((Wheel)obj).UserCode_RpcSpinWheel__Single__Single(reader.ReadFloat(), reader.ReadFloat());
	}

	// Token: 0x06000599 RID: 1433 RVA: 0x00018D0C File Offset: 0x00016F0C
	protected void UserCode_RpcResultFeedback__Int32(int index)
	{
		if (index < 0 || index >= this.Results.Length)
		{
			return;
		}
		this.Results[index].SelectedResultFeedback();
		if (this.resultsLight)
		{
			this.resultsLight.transform.rotation = Quaternion.LookRotation(this.Results[index].transform.position - this.resultsLight.transform.position);
			this.resultsLight.DOIntensity(this.lightIntensity, 0.5f).OnComplete(delegate
			{
				this.resultsLight.DOIntensity(0f, 1f);
			});
		}
	}

	// Token: 0x0600059A RID: 1434 RVA: 0x00018DA7 File Offset: 0x00016FA7
	protected static void InvokeUserCode_RpcResultFeedback__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResultFeedback called on server.");
			return;
		}
		((Wheel)obj).UserCode_RpcResultFeedback__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600059B RID: 1435 RVA: 0x00018DD0 File Offset: 0x00016FD0
	static Wheel()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Wheel), "System.Void Wheel::RpcResetWheel()", new RemoteCallDelegate(Wheel.InvokeUserCode_RpcResetWheel));
		RemoteProcedureCalls.RegisterRpc(typeof(Wheel), "System.Void Wheel::RpcSpinWheel(System.Single,System.Single)", new RemoteCallDelegate(Wheel.InvokeUserCode_RpcSpinWheel__Single__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(Wheel), "System.Void Wheel::RpcResultFeedback(System.Int32)", new RemoteCallDelegate(Wheel.InvokeUserCode_RpcResultFeedback__Int32));
	}

	// Token: 0x040003D9 RID: 985
	[Header("Wheel Settings")]
	[SerializeField]
	protected float spinDuration = 3f;

	// Token: 0x040003DA RID: 986
	[SerializeField]
	protected int minTurnAmount = 3;

	// Token: 0x040003DB RID: 987
	[SerializeField]
	protected bool spinDirection;

	// Token: 0x040003DC RID: 988
	[SerializeField]
	protected Ease easing = Ease.OutCubic;

	// Token: 0x040003DD RID: 989
	[SerializeField]
	protected float lightIntensity = 20f;

	// Token: 0x040003DE RID: 990
	[Header("References")]
	[SerializeField]
	private Transform wheelTransform;

	// Token: 0x040003DF RID: 991
	[SerializeField]
	private Transform resultsParent;

	// Token: 0x040003E0 RID: 992
	[SerializeField]
	protected Transform resultSelector;

	// Token: 0x040003E1 RID: 993
	[SerializeField]
	private Light resultsLight;

	// Token: 0x040003E2 RID: 994
	private WheelResult[] _results;

	// Token: 0x040003E3 RID: 995
	[Header("SFX")]
	[SerializeField]
	protected EventReference sfxSpinEvent;

	// Token: 0x040003E4 RID: 996
	protected EventInstance sfxSpinInstance;

	// Token: 0x040003E6 RID: 998
	protected bool _isSpinning;
}
