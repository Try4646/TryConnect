using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ETFXPEL
{
	// Token: 0x02000369 RID: 873
	public class PEButtonScript : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		// Token: 0x06001CD4 RID: 7380 RVA: 0x0007CC7A File Offset: 0x0007AE7A
		private void Start()
		{
			this.myButton = base.gameObject.GetComponent<Button>();
		}

		// Token: 0x06001CD5 RID: 7381 RVA: 0x0007CC8D File Offset: 0x0007AE8D
		public void OnPointerEnter(PointerEventData eventData)
		{
			UICanvasManager.GlobalAccess.MouseOverButton = true;
			UICanvasManager.GlobalAccess.UpdateToolTip(this.ButtonType);
		}

		// Token: 0x06001CD6 RID: 7382 RVA: 0x0007CCAA File Offset: 0x0007AEAA
		public void OnPointerExit(PointerEventData eventData)
		{
			UICanvasManager.GlobalAccess.MouseOverButton = false;
			UICanvasManager.GlobalAccess.ClearToolTip();
		}

		// Token: 0x06001CD7 RID: 7383 RVA: 0x0007CCC1 File Offset: 0x0007AEC1
		public void OnButtonClicked()
		{
			UICanvasManager.GlobalAccess.UIButtonClick(this.ButtonType);
		}

		// Token: 0x04001364 RID: 4964
		private Button myButton;

		// Token: 0x04001365 RID: 4965
		public ButtonTypes ButtonType;
	}
}
