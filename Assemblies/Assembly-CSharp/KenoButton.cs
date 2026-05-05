using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x02000063 RID: 99
public class KenoButton : InteractableBase
{
	// Token: 0x0600034E RID: 846 RVA: 0x000100FA File Offset: 0x0000E2FA
	protected override void OnAwake()
	{
		base.OnAwake();
		this._diamondMaterial = this.diamondRenderer.material;
		this._scale = this.rend.transform.localScale;
	}

	// Token: 0x0600034F RID: 847 RVA: 0x00010129 File Offset: 0x0000E329
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		if (this.keno.isPlaying)
		{
			return;
		}
		base.ServerOnInteract(playerInteract);
		this._isSelected = this.keno.SelectButton(this);
		this.RpcChangeMaterial(this._isSelected);
	}

	// Token: 0x06000350 RID: 848 RVA: 0x00010160 File Offset: 0x0000E360
	[ClientRpc]
	private void RpcChangeMaterial(bool isSelected)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isSelected);
		this.SendRPCInternal("System.Void KenoButton::RpcChangeMaterial(System.Boolean)", -822695651, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000351 RID: 849 RVA: 0x0001019A File Offset: 0x0000E39A
	[Server]
	public void ServerRevealDiamond(bool isTrue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void KenoButton::ServerRevealDiamond(System.Boolean)' called when server was not active");
			return;
		}
		this.RpcRevealDiamond(isTrue, this._isSelected);
	}

	// Token: 0x06000352 RID: 850 RVA: 0x000101C0 File Offset: 0x0000E3C0
	[ClientRpc]
	private void RpcRevealDiamond(bool isTrue, bool isSelected)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isTrue);
		writer.WriteBool(isSelected);
		this.SendRPCInternal("System.Void KenoButton::RpcRevealDiamond(System.Boolean,System.Boolean)", -326007306, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000353 RID: 851 RVA: 0x00010204 File Offset: 0x0000E404
	[Server]
	public void ServerWarningFeedback()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void KenoButton::ServerWarningFeedback()' called when server was not active");
			return;
		}
		this.RpcWarningFeedback();
	}

	// Token: 0x06000354 RID: 852 RVA: 0x00010224 File Offset: 0x0000E424
	[ClientRpc]
	private void RpcWarningFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void KenoButton::RpcWarningFeedback()", -1152120922, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000357 RID: 855 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000358 RID: 856 RVA: 0x00010292 File Offset: 0x0000E492
	protected void UserCode_RpcChangeMaterial__Boolean(bool isSelected)
	{
		this.rend.material = (isSelected ? this.selectedMaterial : this.notSelectedMaterial);
		this.TooltipMessage = (isSelected ? "[E] Unselect" : "[E] Select");
		this.pressFb.PlayFeedbacks();
	}

	// Token: 0x06000359 RID: 857 RVA: 0x000102D0 File Offset: 0x0000E4D0
	protected static void InvokeUserCode_RpcChangeMaterial__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeMaterial called on server.");
			return;
		}
		((KenoButton)obj).UserCode_RpcChangeMaterial__Boolean(reader.ReadBool());
	}

	// Token: 0x0600035A RID: 858 RVA: 0x000102FC File Offset: 0x0000E4FC
	protected void UserCode_RpcRevealDiamond__Boolean__Boolean(bool isTrue, bool isSelected)
	{
		if (!isTrue)
		{
			this.diamond.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(delegate
			{
				Color grey = Color.grey;
				Color grey2 = Color.grey;
				this._diamondMaterial.color = grey;
				this._diamondMaterial.SetColor("_EmissionColor", grey2);
			});
			this.matchParticles.Stop();
			return;
		}
		if (isSelected)
		{
			this.diamond.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
			this.matchFb.PlayFeedbacks();
			Color antiqueWhite = Color.antiqueWhite;
			Color white = Color.white;
			this._diamondMaterial.color = antiqueWhite;
			this._diamondMaterial.SetColor("_EmissionColor", white);
			return;
		}
		this.diamond.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.OutBack);
	}

	// Token: 0x0600035B RID: 859 RVA: 0x000103C2 File Offset: 0x0000E5C2
	protected static void InvokeUserCode_RpcRevealDiamond__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRevealDiamond called on server.");
			return;
		}
		((KenoButton)obj).UserCode_RpcRevealDiamond__Boolean__Boolean(reader.ReadBool(), reader.ReadBool());
	}

	// Token: 0x0600035C RID: 860 RVA: 0x000103F4 File Offset: 0x0000E5F4
	protected void UserCode_RpcWarningFeedback()
	{
		Transform transform = this.rend.transform;
		transform.DOKill(false);
		Vector3 endValue = this._scale * 1.2f;
		Sequence s = DOTween.Sequence();
		s.Append(transform.DOScale(endValue, 0.15f).SetEase(Ease.OutQuad));
		s.Append(transform.DOScale(this._scale, 0.15f).SetEase(Ease.InQuad));
	}

	// Token: 0x0600035D RID: 861 RVA: 0x00010461 File Offset: 0x0000E661
	protected static void InvokeUserCode_RpcWarningFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcWarningFeedback called on server.");
			return;
		}
		((KenoButton)obj).UserCode_RpcWarningFeedback();
	}

	// Token: 0x0600035E RID: 862 RVA: 0x00010484 File Offset: 0x0000E684
	static KenoButton()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(KenoButton), "System.Void KenoButton::RpcChangeMaterial(System.Boolean)", new RemoteCallDelegate(KenoButton.InvokeUserCode_RpcChangeMaterial__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(KenoButton), "System.Void KenoButton::RpcRevealDiamond(System.Boolean,System.Boolean)", new RemoteCallDelegate(KenoButton.InvokeUserCode_RpcRevealDiamond__Boolean__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(KenoButton), "System.Void KenoButton::RpcWarningFeedback()", new RemoteCallDelegate(KenoButton.InvokeUserCode_RpcWarningFeedback));
	}

	// Token: 0x04000272 RID: 626
	[SerializeField]
	private Keno keno;

	// Token: 0x04000273 RID: 627
	[SerializeField]
	private MeshRenderer rend;

	// Token: 0x04000274 RID: 628
	[SerializeField]
	private Transform diamond;

	// Token: 0x04000275 RID: 629
	[SerializeField]
	private MeshRenderer diamondRenderer;

	// Token: 0x04000276 RID: 630
	[SerializeField]
	private ParticleSystem matchParticles;

	// Token: 0x04000277 RID: 631
	[SerializeField]
	private MMF_Player matchFb;

	// Token: 0x04000278 RID: 632
	[SerializeField]
	private MMF_Player pressFb;

	// Token: 0x04000279 RID: 633
	[SerializeField]
	private Material selectedMaterial;

	// Token: 0x0400027A RID: 634
	[SerializeField]
	private Material notSelectedMaterial;

	// Token: 0x0400027B RID: 635
	private bool _isSelected;

	// Token: 0x0400027C RID: 636
	private Material _diamondMaterial;

	// Token: 0x0400027D RID: 637
	private Vector3 _scale;
}
