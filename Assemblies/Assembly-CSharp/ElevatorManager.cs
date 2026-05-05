using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using SkyBrave_Toolkit.Scripts.Scriptable_Game_Events;
using UnityEngine;

// Token: 0x02000155 RID: 341
public class ElevatorManager : NetworkSingleton<ElevatorManager>
{
	// Token: 0x17000111 RID: 273
	// (get) Token: 0x06000CE8 RID: 3304 RVA: 0x0003664A File Offset: 0x0003484A
	// (set) Token: 0x06000CE9 RID: 3305 RVA: 0x00036652 File Offset: 0x00034852
	public bool IsLocked
	{
		get
		{
			return this._isLocked;
		}
		set
		{
			if (this._isLocked == value)
			{
				return;
			}
			this._isLocked = value;
		}
	}

	// Token: 0x06000CEA RID: 3306 RVA: 0x00036665 File Offset: 0x00034865
	private void Start()
	{
		this.SetButtons();
	}

	// Token: 0x06000CEB RID: 3307 RVA: 0x0003666D File Offset: 0x0003486D
	public void Initialize()
	{
		this.RpcSetActiveFloorOnly(0);
	}

	// Token: 0x06000CEC RID: 3308 RVA: 0x00036678 File Offset: 0x00034878
	private void SetButtons()
	{
		int num = NetworkSingleton<GameManager>.Instance.currentFloor + 1;
		num = Mathf.Max(num, 1);
		for (int i = 0; i < this.buttonList.Count; i++)
		{
			this.buttonList[i].gameObject.SetActive(i <= num);
		}
	}

	// Token: 0x06000CED RID: 3309 RVA: 0x000366D0 File Offset: 0x000348D0
	[ClientRpc]
	public void RpcEnableAllButtons()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void ElevatorManager::RpcEnableAllButtons()", -1353552624, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000CEE RID: 3310 RVA: 0x00036700 File Offset: 0x00034900
	[Server]
	public void ServerTryTeleportPlayers(int toIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ElevatorManager::ServerTryTeleportPlayers(System.Int32)' called when server was not active");
			return;
		}
		if (this.isTeleporting)
		{
			return;
		}
		if (this.IsLocked)
		{
			return;
		}
		if (this._currentFloorIndex == toIndex)
		{
			return;
		}
		if (!this.CheckPlayersInside())
		{
			return;
		}
		this.isTeleporting = true;
		this._currentFloorIndex = toIndex;
		if (this._elevatorRoutine != null)
		{
			base.StopCoroutine(this._elevatorRoutine);
		}
		this._elevatorRoutine = base.StartCoroutine(this.ServerElevatorRoutine(toIndex));
	}

	// Token: 0x06000CEF RID: 3311 RVA: 0x0003677C File Offset: 0x0003497C
	[Server]
	public void ServerForceTeleportPlayers(int toIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ElevatorManager::ServerForceTeleportPlayers(System.Int32)' called when server was not active");
			return;
		}
		if (this._currentFloorIndex == toIndex)
		{
			return;
		}
		this.isTeleporting = true;
		this._currentFloorIndex = toIndex;
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			if (!playerReferences.carry.GetIsBeingHeld())
			{
				Vector2 vector = Random.insideUnitCircle * 3f;
				Vector3 b = new Vector3(vector.x, 0f, vector.y);
				playerReferences.controller.ServerTeleport(NetworkSingleton<ElevatorManager>.Instance.playerSpawnPosition.position + b);
				playerReferences.controller.ServerRotate(new Vector2(180f, 0f));
			}
		}
		if (this._elevatorRoutine != null)
		{
			base.StopCoroutine(this._elevatorRoutine);
		}
		this._elevatorRoutine = base.StartCoroutine(this.ServerElevatorRoutine(toIndex));
	}

	// Token: 0x06000CF0 RID: 3312 RVA: 0x00036890 File Offset: 0x00034A90
	private bool CheckPlayersInside()
	{
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			if (!this.checkCollider.bounds.Contains(playerReferences.transform.position))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000CF1 RID: 3313 RVA: 0x00036908 File Offset: 0x00034B08
	private IEnumerator ServerElevatorRoutine(int toIndex)
	{
		this.RpcMoveDoors(false);
		yield return new WaitForSeconds(this.doorMoveDuration);
		foreach (ConsumableItem consumableItem in NetworkSingleton<ItemManager>.Instance.spawnedItemInstances)
		{
			if (!this.checkCollider.bounds.Contains(consumableItem.transform.position))
			{
				consumableItem.ServerSetEnabled(false);
			}
		}
		this.ServerTeleportPlayersOutside();
		this.RpcSetActiveFloorOnly(toIndex);
		this.RpcSetMusicPlaylistInAdvance(toIndex);
		yield return null;
		if (this.spawnNpc)
		{
			NetworkSingleton<NavMeshManager>.Instance.InitializeNavMesh();
			yield return null;
			yield return NetworkSingleton<NPCSpawner>.Instance.StartCoroutine(NetworkSingleton<NPCSpawner>.Instance.SpawnNPCsForFloor(toIndex));
		}
		GameEvent gameEvent = this.serverOnElevatorMoveEvent;
		if (gameEvent != null)
		{
			gameEvent.Raise();
		}
		if (toIndex == 0 && this.casinoBuilding != null)
		{
			this.casinoBuilding.ServerSpawnGoToHomeVehicle();
		}
		yield return new WaitForSeconds(1f);
		this.RpcMoveDoors(true);
		yield return new WaitForSeconds(this.doorMoveDuration);
		NetworkSingleton<GameManager>.Instance.StartDay();
		this.isTeleporting = false;
		yield break;
	}

	// Token: 0x06000CF2 RID: 3314 RVA: 0x00036920 File Offset: 0x00034B20
	private void ServerTeleportPlayersOutside()
	{
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			if (!playerReferences.carry.GetIsBeingHeld() && !this.checkCollider.bounds.Contains(playerReferences.transform.position))
			{
				Vector2 vector = Random.insideUnitCircle * 3f;
				Vector3 b = new Vector3(vector.x, 0f, vector.y);
				playerReferences.controller.ServerTeleport(NetworkSingleton<ElevatorManager>.Instance.playerSpawnPosition.position + b);
				playerReferences.controller.ServerRotate(new Vector2(180f, 0f));
			}
		}
	}

	// Token: 0x06000CF3 RID: 3315 RVA: 0x00036A0C File Offset: 0x00034C0C
	[ClientRpc]
	private void RpcSetActiveFloorOnly(int index)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(index);
		this.SendRPCInternal("System.Void ElevatorManager::RpcSetActiveFloorOnly(System.Int32)", 915214480, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000CF4 RID: 3316 RVA: 0x00036A48 File Offset: 0x00034C48
	[ClientRpc]
	private void RpcMoveDoors(bool isOpening)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isOpening);
		this.SendRPCInternal("System.Void ElevatorManager::RpcMoveDoors(System.Boolean)", -1745372052, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000CF5 RID: 3317 RVA: 0x00036A84 File Offset: 0x00034C84
	public bool IsInElevator(Vector3 position)
	{
		return this.checkCollider.bounds.Contains(position);
	}

	// Token: 0x06000CF6 RID: 3318 RVA: 0x00036AA8 File Offset: 0x00034CA8
	[ClientRpc]
	private void RpcSetMusicPlaylistInAdvance(int index)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(index);
		this.SendRPCInternal("System.Void ElevatorManager::RpcSetMusicPlaylistInAdvance(System.Int32)", -2012512390, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000CF8 RID: 3320 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000CF9 RID: 3321 RVA: 0x00036AF4 File Offset: 0x00034CF4
	protected void UserCode_RpcEnableAllButtons()
	{
		foreach (Transform transform in this.buttonList)
		{
			transform.gameObject.SetActive(true);
		}
	}

	// Token: 0x06000CFA RID: 3322 RVA: 0x00036B4C File Offset: 0x00034D4C
	protected static void InvokeUserCode_RpcEnableAllButtons(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEnableAllButtons called on server.");
			return;
		}
		((ElevatorManager)obj).UserCode_RpcEnableAllButtons();
	}

	// Token: 0x06000CFB RID: 3323 RVA: 0x00036B70 File Offset: 0x00034D70
	protected void UserCode_RpcSetActiveFloorOnly__Int32(int index)
	{
		foreach (CasinoFloor casinoFloor in this.allFloors)
		{
			bool flag = casinoFloor.floorIndex == index;
			casinoFloor.gameObject.SetActive(flag);
			casinoFloor.SetSfxTrigger(flag);
		}
		MonoSingleton<LightbakerManager>.Instance.SetLightVolume(index);
	}

	// Token: 0x06000CFC RID: 3324 RVA: 0x00036BE4 File Offset: 0x00034DE4
	protected static void InvokeUserCode_RpcSetActiveFloorOnly__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetActiveFloorOnly called on server.");
			return;
		}
		((ElevatorManager)obj).UserCode_RpcSetActiveFloorOnly__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000CFD RID: 3325 RVA: 0x00036C10 File Offset: 0x00034E10
	protected void UserCode_RpcMoveDoors__Boolean(bool isOpening)
	{
		if (isOpening)
		{
			this.rightDoor.DOLocalMoveX(2.5f, this.doorMoveDuration, false).SetEase(Ease.InOutExpo);
			this.leftDoor.DOLocalMoveX(-2.5f, this.doorMoveDuration, false).SetEase(Ease.InOutExpo);
			SFXManager.SFXOneShot(this.uiSfxElevatorOpen, this.dingSfxPos.position);
			SFXManager.SFXOneShot(this.elevatorDingSfx, this.dingSfxPos.position);
			return;
		}
		this.rightDoor.DOLocalMoveX(0f, this.doorMoveDuration, false).SetEase(Ease.InOutExpo);
		this.leftDoor.DOLocalMoveX(0f, this.doorMoveDuration, false).SetEase(Ease.InOutExpo);
		SFXManager.SFXOneShot(this.uiSfxElevatorRise, this.dingSfxPos.position);
	}

	// Token: 0x06000CFE RID: 3326 RVA: 0x00036CDF File Offset: 0x00034EDF
	protected static void InvokeUserCode_RpcMoveDoors__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveDoors called on server.");
			return;
		}
		((ElevatorManager)obj).UserCode_RpcMoveDoors__Boolean(reader.ReadBool());
	}

	// Token: 0x06000CFF RID: 3327 RVA: 0x00036D08 File Offset: 0x00034F08
	protected void UserCode_RpcSetMusicPlaylistInAdvance__Int32(int index)
	{
		string text;
		switch (index)
		{
		case 0:
			text = "Lobby";
			break;
		case 1:
			text = "CasinoLevel1";
			break;
		case 2:
			text = "CasinoLevel2";
			break;
		case 3:
			text = "CasinoLevel3";
			break;
		case 4:
			text = "CasinoLevel4";
			break;
		case 5:
			text = "BossRoom";
			break;
		default:
			text = "";
			break;
		}
		string label = text;
		if (NetworkSingleton<GameManager>.Instance.state == GameState.Win && index == 1)
		{
			label = "BossRoom";
		}
		RuntimeManager.StudioSystem.setParameterByNameWithLabel("MusicPlaylist", label, false);
	}

	// Token: 0x06000D00 RID: 3328 RVA: 0x00036D99 File Offset: 0x00034F99
	protected static void InvokeUserCode_RpcSetMusicPlaylistInAdvance__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMusicPlaylistInAdvance called on server.");
			return;
		}
		((ElevatorManager)obj).UserCode_RpcSetMusicPlaylistInAdvance__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000D01 RID: 3329 RVA: 0x00036DC4 File Offset: 0x00034FC4
	static ElevatorManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(ElevatorManager), "System.Void ElevatorManager::RpcEnableAllButtons()", new RemoteCallDelegate(ElevatorManager.InvokeUserCode_RpcEnableAllButtons));
		RemoteProcedureCalls.RegisterRpc(typeof(ElevatorManager), "System.Void ElevatorManager::RpcSetActiveFloorOnly(System.Int32)", new RemoteCallDelegate(ElevatorManager.InvokeUserCode_RpcSetActiveFloorOnly__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(ElevatorManager), "System.Void ElevatorManager::RpcMoveDoors(System.Boolean)", new RemoteCallDelegate(ElevatorManager.InvokeUserCode_RpcMoveDoors__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(ElevatorManager), "System.Void ElevatorManager::RpcSetMusicPlaylistInAdvance(System.Int32)", new RemoteCallDelegate(ElevatorManager.InvokeUserCode_RpcSetMusicPlaylistInAdvance__Int32));
	}

	// Token: 0x0400085A RID: 2138
	[Header("References")]
	[SerializeField]
	private Transform rightDoor;

	// Token: 0x0400085B RID: 2139
	[SerializeField]
	private Transform leftDoor;

	// Token: 0x0400085C RID: 2140
	public Transform playerSpawnPosition;

	// Token: 0x0400085D RID: 2141
	[SerializeField]
	private List<Transform> buttonList;

	// Token: 0x0400085E RID: 2142
	[SerializeField]
	private List<CasinoFloor> allFloors;

	// Token: 0x0400085F RID: 2143
	[SerializeField]
	private Collider checkCollider;

	// Token: 0x04000860 RID: 2144
	[SerializeField]
	private GameEvent serverOnElevatorMoveEvent;

	// Token: 0x04000861 RID: 2145
	[SerializeField]
	private CasinoBuilding casinoBuilding;

	// Token: 0x04000862 RID: 2146
	[Header("Settings")]
	public float doorMoveDuration;

	// Token: 0x04000863 RID: 2147
	public bool spawnNpc = true;

	// Token: 0x04000864 RID: 2148
	[Header("SFX")]
	[SerializeField]
	private Transform dingSfxPos;

	// Token: 0x04000865 RID: 2149
	[SerializeField]
	private EventReference uiSfxElevatorRise;

	// Token: 0x04000866 RID: 2150
	[SerializeField]
	private EventReference uiSfxElevatorOpen;

	// Token: 0x04000867 RID: 2151
	[SerializeField]
	private EventReference elevatorDingSfx;

	// Token: 0x04000868 RID: 2152
	public bool isTeleporting;

	// Token: 0x04000869 RID: 2153
	private Coroutine _elevatorRoutine;

	// Token: 0x0400086A RID: 2154
	private int _currentFloorIndex;

	// Token: 0x0400086B RID: 2155
	private bool _isLocked;
}
