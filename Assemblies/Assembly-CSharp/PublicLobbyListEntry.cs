using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000247 RID: 583
public class PublicLobbyListEntry : MonoBehaviour
{
	// Token: 0x060014FB RID: 5371 RVA: 0x0005A040 File Offset: 0x00058240
	public void Initialize(PublicLobbyListUI owner, CSteamID lobbyId, string labelText)
	{
		this._owner = owner;
		this._lobbyId = lobbyId;
		if (this.label != null)
		{
			this.label.text = labelText;
		}
		if (this.button != null)
		{
			this.button.onClick.AddListener(new UnityAction(this.OnClick));
		}
	}

	// Token: 0x060014FC RID: 5372 RVA: 0x0005A09F File Offset: 0x0005829F
	public void OnClick()
	{
		Debug.Log(string.Format("[PublicLobbyListEntry] OnClick: lobby {0}", this._lobbyId.m_SteamID));
		PublicLobbyListUI owner = this._owner;
		if (owner == null)
		{
			return;
		}
		owner.JoinLobby(this._lobbyId);
	}

	// Token: 0x04000D65 RID: 3429
	[SerializeField]
	private TextMeshProUGUI label;

	// Token: 0x04000D66 RID: 3430
	[SerializeField]
	private Button button;

	// Token: 0x04000D67 RID: 3431
	private CSteamID _lobbyId;

	// Token: 0x04000D68 RID: 3432
	private PublicLobbyListUI _owner;
}
