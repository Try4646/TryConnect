using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200029F RID: 671
[RequireComponent(typeof(Button))]
public class BackButtonEscapeHandler : MonoBehaviour
{
	// Token: 0x060017D6 RID: 6102 RVA: 0x00065052 File Offset: 0x00063252
	private void Awake()
	{
		this._button = base.GetComponent<Button>();
	}

	// Token: 0x060017D7 RID: 6103 RVA: 0x00065060 File Offset: 0x00063260
	private void OnEnable()
	{
		InputEvents.OnEscapeMenuEvent = (Action)Delegate.Combine(InputEvents.OnEscapeMenuEvent, new Action(this.OnEscape));
	}

	// Token: 0x060017D8 RID: 6104 RVA: 0x00065082 File Offset: 0x00063282
	private void OnDisable()
	{
		InputEvents.OnEscapeMenuEvent = (Action)Delegate.Remove(InputEvents.OnEscapeMenuEvent, new Action(this.OnEscape));
	}

	// Token: 0x060017D9 RID: 6105 RVA: 0x000650A4 File Offset: 0x000632A4
	private void OnEscape()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Debug.Log("OnEscape: " + base.gameObject.name + " - " + this._button.onClick.ToString(), base.gameObject);
			this._button.onClick.Invoke();
		}
	}

	// Token: 0x04000F6B RID: 3947
	private Button _button;
}
