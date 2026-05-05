using System;
using UnityEngine;
using UnityEngine.UI;

namespace ETFXPEL
{
	// Token: 0x0200036B RID: 875
	public class UICanvasManager : MonoBehaviour
	{
		// Token: 0x06001CE0 RID: 7392 RVA: 0x0007D112 File Offset: 0x0007B312
		private void Awake()
		{
			UICanvasManager.GlobalAccess = this;
		}

		// Token: 0x06001CE1 RID: 7393 RVA: 0x0007D11A File Offset: 0x0007B31A
		private void Start()
		{
			if (this.PENameText != null)
			{
				this.PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
			}
		}

		// Token: 0x06001CE2 RID: 7394 RVA: 0x0007D13F File Offset: 0x0007B33F
		private void Update()
		{
			if (!this.MouseOverButton && Input.GetMouseButtonUp(0))
			{
				this.SpawnCurrentParticleEffect();
			}
			if (Input.GetKeyUp(KeyCode.A))
			{
				this.SelectPreviousPE();
			}
			if (Input.GetKeyUp(KeyCode.D))
			{
				this.SelectNextPE();
			}
		}

		// Token: 0x06001CE3 RID: 7395 RVA: 0x0007D175 File Offset: 0x0007B375
		public void UpdateToolTip(ButtonTypes toolTipType)
		{
			if (this.ToolTipText != null)
			{
				if (toolTipType == ButtonTypes.Previous)
				{
					this.ToolTipText.text = "Select Previous Particle Effect";
					return;
				}
				if (toolTipType == ButtonTypes.Next)
				{
					this.ToolTipText.text = "Select Next Particle Effect";
				}
			}
		}

		// Token: 0x06001CE4 RID: 7396 RVA: 0x0007D1AE File Offset: 0x0007B3AE
		public void ClearToolTip()
		{
			if (this.ToolTipText != null)
			{
				this.ToolTipText.text = "";
			}
		}

		// Token: 0x06001CE5 RID: 7397 RVA: 0x0007D1CE File Offset: 0x0007B3CE
		private void SelectPreviousPE()
		{
			ParticleEffectsLibrary.GlobalAccess.PreviousParticleEffect();
			if (this.PENameText != null)
			{
				this.PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
			}
		}

		// Token: 0x06001CE6 RID: 7398 RVA: 0x0007D1FD File Offset: 0x0007B3FD
		private void SelectNextPE()
		{
			ParticleEffectsLibrary.GlobalAccess.NextParticleEffect();
			if (this.PENameText != null)
			{
				this.PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
			}
		}

		// Token: 0x06001CE7 RID: 7399 RVA: 0x0007D22C File Offset: 0x0007B42C
		private void SpawnCurrentParticleEffect()
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out this.rayHit))
			{
				ParticleEffectsLibrary.GlobalAccess.SpawnParticleEffect(this.rayHit.point);
			}
		}

		// Token: 0x06001CE8 RID: 7400 RVA: 0x0007D25F File Offset: 0x0007B45F
		public void UIButtonClick(ButtonTypes buttonTypeClicked)
		{
			if (buttonTypeClicked == ButtonTypes.Previous)
			{
				this.SelectPreviousPE();
				return;
			}
			if (buttonTypeClicked != ButtonTypes.Next)
			{
				return;
			}
			this.SelectNextPE();
		}

		// Token: 0x04001370 RID: 4976
		public static UICanvasManager GlobalAccess;

		// Token: 0x04001371 RID: 4977
		public bool MouseOverButton;

		// Token: 0x04001372 RID: 4978
		public Text PENameText;

		// Token: 0x04001373 RID: 4979
		public Text ToolTipText;

		// Token: 0x04001374 RID: 4980
		private RaycastHit rayHit;
	}
}
