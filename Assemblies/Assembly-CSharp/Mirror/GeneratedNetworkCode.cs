using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dissonance.Integrations.MirrorIgnorance;
using Mirror.Discovery;
using Smooth;
using UnityEngine;

namespace Mirror
{
	// Token: 0x020003A5 RID: 933
	[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
	public static class GeneratedNetworkCode
	{
		// Token: 0x06001E38 RID: 7736 RVA: 0x0008224C File Offset: 0x0008044C
		public static TimeSnapshotMessage TimeSnapshotMessage(NetworkReader reader)
		{
			return default(TimeSnapshotMessage);
		}

		// Token: 0x06001E39 RID: 7737 RVA: 0x00082264 File Offset: 0x00080464
		public static void TimeSnapshotMessage(NetworkWriter writer, TimeSnapshotMessage value)
		{
		}

		// Token: 0x06001E3A RID: 7738 RVA: 0x00082274 File Offset: 0x00080474
		public static ReadyMessage ReadyMessage(NetworkReader reader)
		{
			return default(ReadyMessage);
		}

		// Token: 0x06001E3B RID: 7739 RVA: 0x0008228C File Offset: 0x0008048C
		public static void ReadyMessage(NetworkWriter writer, ReadyMessage value)
		{
		}

		// Token: 0x06001E3C RID: 7740 RVA: 0x0008229C File Offset: 0x0008049C
		public static NotReadyMessage NotReadyMessage(NetworkReader reader)
		{
			return default(NotReadyMessage);
		}

		// Token: 0x06001E3D RID: 7741 RVA: 0x000822B4 File Offset: 0x000804B4
		public static void NotReadyMessage(NetworkWriter writer, NotReadyMessage value)
		{
		}

		// Token: 0x06001E3E RID: 7742 RVA: 0x000822C4 File Offset: 0x000804C4
		public static AddPlayerMessage AddPlayerMessage(NetworkReader reader)
		{
			return default(AddPlayerMessage);
		}

		// Token: 0x06001E3F RID: 7743 RVA: 0x000822DC File Offset: 0x000804DC
		public static void AddPlayerMessage(NetworkWriter writer, AddPlayerMessage value)
		{
		}

		// Token: 0x06001E40 RID: 7744 RVA: 0x000822EC File Offset: 0x000804EC
		public static SceneMessage SceneMessage(NetworkReader reader)
		{
			return new SceneMessage
			{
				sceneName = reader.ReadString(),
				sceneOperation = GeneratedNetworkCode._Read_Mirror.SceneOperation(reader),
				customHandling = reader.ReadBool()
			};
		}

		// Token: 0x06001E41 RID: 7745 RVA: 0x00082334 File Offset: 0x00080534
		public static SceneOperation SceneOperation(NetworkReader reader)
		{
			return (SceneOperation)reader.ReadByte();
		}

		// Token: 0x06001E42 RID: 7746 RVA: 0x00082348 File Offset: 0x00080548
		public static void SceneMessage(NetworkWriter writer, SceneMessage value)
		{
			writer.WriteString(value.sceneName);
			GeneratedNetworkCode._Write_Mirror.SceneOperation(writer, value.sceneOperation);
			writer.WriteBool(value.customHandling);
		}

		// Token: 0x06001E43 RID: 7747 RVA: 0x0008237C File Offset: 0x0008057C
		public static void SceneOperation(NetworkWriter writer, SceneOperation value)
		{
			writer.WriteByte((byte)value);
		}

		// Token: 0x06001E44 RID: 7748 RVA: 0x00082390 File Offset: 0x00080590
		public static CommandMessage CommandMessage(NetworkReader reader)
		{
			return new CommandMessage
			{
				netId = reader.ReadVarUInt(),
				componentIndex = reader.ReadByte(),
				functionHash = reader.ReadUShort(),
				payload = reader.ReadArraySegmentAndSize()
			};
		}

		// Token: 0x06001E45 RID: 7749 RVA: 0x000823E4 File Offset: 0x000805E4
		public static void CommandMessage(NetworkWriter writer, CommandMessage value)
		{
			writer.WriteVarUInt(value.netId);
			writer.WriteByte(value.componentIndex);
			writer.WriteUShort(value.functionHash);
			writer.WriteArraySegmentAndSize(value.payload);
		}

		// Token: 0x06001E46 RID: 7750 RVA: 0x00082424 File Offset: 0x00080624
		public static RpcMessage RpcMessage(NetworkReader reader)
		{
			return new RpcMessage
			{
				netId = reader.ReadVarUInt(),
				componentIndex = reader.ReadByte(),
				functionHash = reader.ReadUShort(),
				payload = reader.ReadArraySegmentAndSize()
			};
		}

		// Token: 0x06001E47 RID: 7751 RVA: 0x00082478 File Offset: 0x00080678
		public static void RpcMessage(NetworkWriter writer, RpcMessage value)
		{
			writer.WriteVarUInt(value.netId);
			writer.WriteByte(value.componentIndex);
			writer.WriteUShort(value.functionHash);
			writer.WriteArraySegmentAndSize(value.payload);
		}

		// Token: 0x06001E48 RID: 7752 RVA: 0x000824B8 File Offset: 0x000806B8
		public static SpawnMessage SpawnMessage(NetworkReader reader)
		{
			return new SpawnMessage
			{
				netId = reader.ReadVarUInt(),
				spawnFlags = GeneratedNetworkCode._Read_Mirror.SpawnFlags(reader),
				sceneId = reader.ReadVarULong(),
				assetId = reader.ReadVarUInt(),
				position = reader.ReadVector3(),
				rotation = reader.ReadQuaternion(),
				scale = reader.ReadVector3(),
				payload = reader.ReadArraySegmentAndSize()
			};
		}

		// Token: 0x06001E49 RID: 7753 RVA: 0x00082548 File Offset: 0x00080748
		public static SpawnFlags SpawnFlags(NetworkReader reader)
		{
			return (SpawnFlags)reader.ReadByte();
		}

		// Token: 0x06001E4A RID: 7754 RVA: 0x0008255C File Offset: 0x0008075C
		public static void SpawnMessage(NetworkWriter writer, SpawnMessage value)
		{
			writer.WriteVarUInt(value.netId);
			GeneratedNetworkCode._Write_Mirror.SpawnFlags(writer, value.spawnFlags);
			writer.WriteVarULong(value.sceneId);
			writer.WriteVarUInt(value.assetId);
			writer.WriteVector3(value.position);
			writer.WriteQuaternion(value.rotation);
			writer.WriteVector3(value.scale);
			writer.WriteArraySegmentAndSize(value.payload);
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x000825CC File Offset: 0x000807CC
		public static void SpawnFlags(NetworkWriter writer, SpawnFlags value)
		{
			writer.WriteByte((byte)value);
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x000825E0 File Offset: 0x000807E0
		public static ChangeOwnerMessage ChangeOwnerMessage(NetworkReader reader)
		{
			return new ChangeOwnerMessage
			{
				netId = reader.ReadVarUInt(),
				spawnFlags = GeneratedNetworkCode._Read_Mirror.SpawnFlags(reader)
			};
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x00082618 File Offset: 0x00080818
		public static void ChangeOwnerMessage(NetworkWriter writer, ChangeOwnerMessage value)
		{
			writer.WriteVarUInt(value.netId);
			GeneratedNetworkCode._Write_Mirror.SpawnFlags(writer, value.spawnFlags);
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x00082640 File Offset: 0x00080840
		public static ObjectSpawnStartedMessage ObjectSpawnStartedMessage(NetworkReader reader)
		{
			return default(ObjectSpawnStartedMessage);
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x00082658 File Offset: 0x00080858
		public static void ObjectSpawnStartedMessage(NetworkWriter writer, ObjectSpawnStartedMessage value)
		{
		}

		// Token: 0x06001E50 RID: 7760 RVA: 0x00082668 File Offset: 0x00080868
		public static ObjectSpawnFinishedMessage ObjectSpawnFinishedMessage(NetworkReader reader)
		{
			return default(ObjectSpawnFinishedMessage);
		}

		// Token: 0x06001E51 RID: 7761 RVA: 0x00082680 File Offset: 0x00080880
		public static void ObjectSpawnFinishedMessage(NetworkWriter writer, ObjectSpawnFinishedMessage value)
		{
		}

		// Token: 0x06001E52 RID: 7762 RVA: 0x00082690 File Offset: 0x00080890
		public static ObjectDestroyMessage ObjectDestroyMessage(NetworkReader reader)
		{
			return new ObjectDestroyMessage
			{
				netId = reader.ReadVarUInt()
			};
		}

		// Token: 0x06001E53 RID: 7763 RVA: 0x000826B8 File Offset: 0x000808B8
		public static void ObjectDestroyMessage(NetworkWriter writer, ObjectDestroyMessage value)
		{
			writer.WriteVarUInt(value.netId);
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x000826D4 File Offset: 0x000808D4
		public static ObjectHideMessage ObjectHideMessage(NetworkReader reader)
		{
			return new ObjectHideMessage
			{
				netId = reader.ReadVarUInt()
			};
		}

		// Token: 0x06001E55 RID: 7765 RVA: 0x000826FC File Offset: 0x000808FC
		public static void ObjectHideMessage(NetworkWriter writer, ObjectHideMessage value)
		{
			writer.WriteVarUInt(value.netId);
		}

		// Token: 0x06001E56 RID: 7766 RVA: 0x00082718 File Offset: 0x00080918
		public static EntityStateMessage EntityStateMessage(NetworkReader reader)
		{
			return new EntityStateMessage
			{
				netId = reader.ReadVarUInt(),
				payload = reader.ReadArraySegmentAndSize()
			};
		}

		// Token: 0x06001E57 RID: 7767 RVA: 0x00082750 File Offset: 0x00080950
		public static void EntityStateMessage(NetworkWriter writer, EntityStateMessage value)
		{
			writer.WriteVarUInt(value.netId);
			writer.WriteArraySegmentAndSize(value.payload);
		}

		// Token: 0x06001E58 RID: 7768 RVA: 0x00082778 File Offset: 0x00080978
		public static NetworkPingMessage NetworkPingMessage(NetworkReader reader)
		{
			return new NetworkPingMessage
			{
				localTime = reader.ReadDouble(),
				predictedTimeAdjusted = reader.ReadDouble()
			};
		}

		// Token: 0x06001E59 RID: 7769 RVA: 0x000827B0 File Offset: 0x000809B0
		public static void NetworkPingMessage(NetworkWriter writer, NetworkPingMessage value)
		{
			writer.WriteDouble(value.localTime);
			writer.WriteDouble(value.predictedTimeAdjusted);
		}

		// Token: 0x06001E5A RID: 7770 RVA: 0x000827D8 File Offset: 0x000809D8
		public static NetworkPongMessage NetworkPongMessage(NetworkReader reader)
		{
			return new NetworkPongMessage
			{
				localTime = reader.ReadDouble(),
				predictionErrorUnadjusted = reader.ReadDouble(),
				predictionErrorAdjusted = reader.ReadDouble()
			};
		}

		// Token: 0x06001E5B RID: 7771 RVA: 0x00082820 File Offset: 0x00080A20
		public static void NetworkPongMessage(NetworkWriter writer, NetworkPongMessage value)
		{
			writer.WriteDouble(value.localTime);
			writer.WriteDouble(value.predictionErrorUnadjusted);
			writer.WriteDouble(value.predictionErrorAdjusted);
		}

		// Token: 0x06001E5C RID: 7772 RVA: 0x00082854 File Offset: 0x00080A54
		public static ServerRequest ServerRequest(NetworkReader reader)
		{
			return default(ServerRequest);
		}

		// Token: 0x06001E5D RID: 7773 RVA: 0x0008286C File Offset: 0x00080A6C
		public static void ServerRequest(NetworkWriter writer, ServerRequest value)
		{
		}

		// Token: 0x06001E5E RID: 7774 RVA: 0x0008287C File Offset: 0x00080A7C
		public static ServerResponse ServerResponse(NetworkReader reader)
		{
			return new ServerResponse
			{
				uri = reader.ReadUri(),
				serverId = reader.ReadVarLong()
			};
		}

		// Token: 0x06001E5F RID: 7775 RVA: 0x000828B4 File Offset: 0x00080AB4
		public static void ServerResponse(NetworkWriter writer, ServerResponse value)
		{
			writer.WriteUri(value.uri);
			writer.WriteVarLong(value.serverId);
		}

		// Token: 0x06001E60 RID: 7776 RVA: 0x000828DC File Offset: 0x00080ADC
		public static WebSocketRelayMessage _Read_WebSocketRelayMessage(NetworkReader reader)
		{
			return new WebSocketRelayMessage
			{
				rawData = reader.ReadString(),
				messageType = reader.ReadString()
			};
		}

		// Token: 0x06001E61 RID: 7777 RVA: 0x00082914 File Offset: 0x00080B14
		public static void _Write_WebSocketRelayMessage(NetworkWriter writer, WebSocketRelayMessage value)
		{
			writer.WriteString(value.rawData);
			writer.WriteString(value.messageType);
		}

		// Token: 0x06001E62 RID: 7778 RVA: 0x0008293C File Offset: 0x00080B3C
		public static SceneReadyMessage _Read_SceneReadyMessage(NetworkReader reader)
		{
			return default(SceneReadyMessage);
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x00082954 File Offset: 0x00080B54
		public static void _Write_SceneReadyMessage(NetworkWriter writer, SceneReadyMessage value)
		{
		}

		// Token: 0x06001E64 RID: 7780 RVA: 0x00082964 File Offset: 0x00080B64
		public static JoinGameMessage _Read_JoinGameMessage(NetworkReader reader)
		{
			return default(JoinGameMessage);
		}

		// Token: 0x06001E65 RID: 7781 RVA: 0x0008297C File Offset: 0x00080B7C
		public static void _Write_JoinGameMessage(NetworkWriter writer, JoinGameMessage value)
		{
		}

		// Token: 0x06001E66 RID: 7782 RVA: 0x0008298C File Offset: 0x00080B8C
		public static ClientScenePlayReadyMessage _Read_ClientScenePlayReadyMessage(NetworkReader reader)
		{
			return new ClientScenePlayReadyMessage
			{
				epoch = reader.ReadVarInt()
			};
		}

		// Token: 0x06001E67 RID: 7783 RVA: 0x000829B4 File Offset: 0x00080BB4
		public static void _Write_ClientScenePlayReadyMessage(NetworkWriter writer, ClientScenePlayReadyMessage value)
		{
			writer.WriteVarInt(value.epoch);
		}

		// Token: 0x06001E68 RID: 7784 RVA: 0x000829D0 File Offset: 0x00080BD0
		public static CardData _Read_CardData(NetworkReader reader)
		{
			return new CardData
			{
				Suit = GeneratedNetworkCode._Read_Suit(reader),
				Rank = GeneratedNetworkCode._Read_Rank(reader)
			};
		}

		// Token: 0x06001E69 RID: 7785 RVA: 0x00082A08 File Offset: 0x00080C08
		public static Suit _Read_Suit(NetworkReader reader)
		{
			return (Suit)reader.ReadVarInt();
		}

		// Token: 0x06001E6A RID: 7786 RVA: 0x00082A1C File Offset: 0x00080C1C
		public static Rank _Read_Rank(NetworkReader reader)
		{
			return (Rank)reader.ReadVarInt();
		}

		// Token: 0x06001E6B RID: 7787 RVA: 0x00082A30 File Offset: 0x00080C30
		public static void _Write_CardData(NetworkWriter writer, CardData value)
		{
			GeneratedNetworkCode._Write_Suit(writer, value.Suit);
			GeneratedNetworkCode._Write_Rank(writer, value.Rank);
		}

		// Token: 0x06001E6C RID: 7788 RVA: 0x00082A58 File Offset: 0x00080C58
		public static void _Write_Suit(NetworkWriter writer, Suit value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E6D RID: 7789 RVA: 0x00082A6C File Offset: 0x00080C6C
		public static void _Write_Rank(NetworkWriter writer, Rank value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E6E RID: 7790 RVA: 0x00082A80 File Offset: 0x00080C80
		public static void _Write_Baccarat/CardAreaType(NetworkWriter writer, Baccarat.CardAreaType value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E6F RID: 7791 RVA: 0x00082A94 File Offset: 0x00080C94
		public static Baccarat.CardAreaType _Read_Baccarat/CardAreaType(NetworkReader reader)
		{
			return (Baccarat.CardAreaType)reader.ReadVarInt();
		}

		// Token: 0x06001E70 RID: 7792 RVA: 0x00082AA8 File Offset: 0x00080CA8
		public static void _Write_BaccaratBetType(NetworkWriter writer, BaccaratBetType value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E71 RID: 7793 RVA: 0x00082ABC File Offset: 0x00080CBC
		public static BaccaratBetType _Read_BaccaratBetType(NetworkReader reader)
		{
			return (BaccaratBetType)reader.ReadVarInt();
		}

		// Token: 0x06001E72 RID: 7794 RVA: 0x00082AD0 File Offset: 0x00080CD0
		public static void _Write_Blackjack/CardAreaType(NetworkWriter writer, Blackjack.CardAreaType value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E73 RID: 7795 RVA: 0x00082AE4 File Offset: 0x00080CE4
		public static Blackjack.CardAreaType _Read_Blackjack/CardAreaType(NetworkReader reader)
		{
			return (Blackjack.CardAreaType)reader.ReadVarInt();
		}

		// Token: 0x06001E74 RID: 7796 RVA: 0x00082AF8 File Offset: 0x00080CF8
		public static void Gradient(NetworkWriter writer, Gradient value)
		{
			if (value == null)
			{
				writer.WriteBool(false);
				return;
			}
			writer.WriteBool(true);
		}

		// Token: 0x06001E75 RID: 7797 RVA: 0x00082B1C File Offset: 0x00080D1C
		public static Gradient Gradient(NetworkReader reader)
		{
			if (!reader.ReadBool())
			{
				return null;
			}
			return new Gradient();
		}

		// Token: 0x06001E76 RID: 7798 RVA: 0x00082B40 File Offset: 0x00080D40
		public static void _Write_DragonTowerButton/ButtonState(NetworkWriter writer, DragonTowerButton.ButtonState value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E77 RID: 7799 RVA: 0x00082B54 File Offset: 0x00080D54
		public static DragonTowerButton.ButtonState _Read_DragonTowerButton/ButtonState(NetworkReader reader)
		{
			return (DragonTowerButton.ButtonState)reader.ReadVarInt();
		}

		// Token: 0x06001E78 RID: 7800 RVA: 0x00082B68 File Offset: 0x00080D68
		public static void _Write_BankMode(NetworkWriter writer, BankMode value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E79 RID: 7801 RVA: 0x00082B7C File Offset: 0x00080D7C
		public static BankMode _Read_BankMode(NetworkReader reader)
		{
			return (BankMode)reader.ReadVarInt();
		}

		// Token: 0x06001E7A RID: 7802 RVA: 0x00082B90 File Offset: 0x00080D90
		public static void List(NetworkWriter writer, List<int> value)
		{
			writer.WriteList(value);
		}

		// Token: 0x06001E7B RID: 7803 RVA: 0x00082BA4 File Offset: 0x00080DA4
		public static List<int> List(NetworkReader reader)
		{
			return reader.ReadList<int>();
		}

		// Token: 0x06001E7C RID: 7804 RVA: 0x00082BB8 File Offset: 0x00080DB8
		public static void _Write_GameState(NetworkWriter writer, GameState value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E7D RID: 7805 RVA: 0x00082BCC File Offset: 0x00080DCC
		public static GameState _Read_GameState(NetworkReader reader)
		{
			return (GameState)reader.ReadVarInt();
		}

		// Token: 0x06001E7E RID: 7806 RVA: 0x00082BE0 File Offset: 0x00080DE0
		public static void _Write_SFXParams[](NetworkWriter writer, SFXParams[] value)
		{
			writer.WriteArray(value);
		}

		// Token: 0x06001E7F RID: 7807 RVA: 0x00082BF4 File Offset: 0x00080DF4
		public static void _Write_SFXParams(NetworkWriter writer, SFXParams value)
		{
			writer.WriteString(value.name);
			writer.WriteFloat(value.value);
		}

		// Token: 0x06001E80 RID: 7808 RVA: 0x00082C1C File Offset: 0x00080E1C
		public static SFXParams[] _Read_SFXParams[](NetworkReader reader)
		{
			return reader.ReadArray<SFXParams>();
		}

		// Token: 0x06001E81 RID: 7809 RVA: 0x00082C30 File Offset: 0x00080E30
		public static SFXParams _Read_SFXParams(NetworkReader reader)
		{
			return new SFXParams
			{
				name = reader.ReadString(),
				value = reader.ReadFloat()
			};
		}

		// Token: 0x06001E82 RID: 7810 RVA: 0x00082C68 File Offset: 0x00080E68
		public static void _Write_ChallengeSyncData[](NetworkWriter writer, ChallengeSyncData[] value)
		{
			writer.WriteArray(value);
		}

		// Token: 0x06001E83 RID: 7811 RVA: 0x00082C7C File Offset: 0x00080E7C
		public static void _Write_ChallengeSyncData(NetworkWriter writer, ChallengeSyncData value)
		{
			writer.WriteVarInt(value.challengeID);
			writer.WriteFloat(value.progress);
			writer.WriteBool(value.isCompleted);
			writer.WriteBool(value.isClaimed);
			writer.WriteVarInt(value.completionCount);
			writer.WriteVarLong(value.lastBet);
			writer.WriteVarLong(value.lastPayout);
			GeneratedNetworkCode._Write_CasinoGameType(writer, value.lastGameType);
			GeneratedNetworkCode._Write_ConditionStateSyncData[](writer, value.conditionStates);
		}

		// Token: 0x06001E84 RID: 7812 RVA: 0x00082CF8 File Offset: 0x00080EF8
		public static void _Write_CasinoGameType(NetworkWriter writer, CasinoGameType value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E85 RID: 7813 RVA: 0x00082D0C File Offset: 0x00080F0C
		public static void _Write_ConditionStateSyncData[](NetworkWriter writer, ConditionStateSyncData[] value)
		{
			writer.WriteArray(value);
		}

		// Token: 0x06001E86 RID: 7814 RVA: 0x00082D20 File Offset: 0x00080F20
		public static void _Write_ConditionStateSyncData(NetworkWriter writer, ConditionStateSyncData value)
		{
			writer.WriteVarInt(value.currentWinCount);
			writer.WriteVarInt(value.consecutiveWinCount);
			writer.WriteVarInt(value.currentLossCount);
			writer.WriteVarInt(value.consecutiveLossCount);
			writer.WriteVarLong(value.totalBetAmount);
			writer.WriteVarLong(value.totalPayoutAmount);
			writer.WriteVarLong(value.totalProfit);
			writer.WriteFloat(value.elapsedSinceStart);
			writer.WriteFloat(value.elapsedSinceLastGame);
		}

		// Token: 0x06001E87 RID: 7815 RVA: 0x00082D9C File Offset: 0x00080F9C
		public static ChallengeSyncData[] _Read_ChallengeSyncData[](NetworkReader reader)
		{
			return reader.ReadArray<ChallengeSyncData>();
		}

		// Token: 0x06001E88 RID: 7816 RVA: 0x00082DB0 File Offset: 0x00080FB0
		public static ChallengeSyncData _Read_ChallengeSyncData(NetworkReader reader)
		{
			return new ChallengeSyncData
			{
				challengeID = reader.ReadVarInt(),
				progress = reader.ReadFloat(),
				isCompleted = reader.ReadBool(),
				isClaimed = reader.ReadBool(),
				completionCount = reader.ReadVarInt(),
				lastBet = reader.ReadVarLong(),
				lastPayout = reader.ReadVarLong(),
				lastGameType = GeneratedNetworkCode._Read_CasinoGameType(reader),
				conditionStates = GeneratedNetworkCode._Read_ConditionStateSyncData[](reader)
			};
		}

		// Token: 0x06001E89 RID: 7817 RVA: 0x00082E50 File Offset: 0x00081050
		public static CasinoGameType _Read_CasinoGameType(NetworkReader reader)
		{
			return (CasinoGameType)reader.ReadVarInt();
		}

		// Token: 0x06001E8A RID: 7818 RVA: 0x00082E64 File Offset: 0x00081064
		public static ConditionStateSyncData[] _Read_ConditionStateSyncData[](NetworkReader reader)
		{
			return reader.ReadArray<ConditionStateSyncData>();
		}

		// Token: 0x06001E8B RID: 7819 RVA: 0x00082E78 File Offset: 0x00081078
		public static ConditionStateSyncData _Read_ConditionStateSyncData(NetworkReader reader)
		{
			return new ConditionStateSyncData
			{
				currentWinCount = reader.ReadVarInt(),
				consecutiveWinCount = reader.ReadVarInt(),
				currentLossCount = reader.ReadVarInt(),
				consecutiveLossCount = reader.ReadVarInt(),
				totalBetAmount = reader.ReadVarLong(),
				totalPayoutAmount = reader.ReadVarLong(),
				totalProfit = reader.ReadVarLong(),
				elapsedSinceStart = reader.ReadFloat(),
				elapsedSinceLastGame = reader.ReadFloat()
			};
		}

		// Token: 0x06001E8C RID: 7820 RVA: 0x00082F18 File Offset: 0x00081118
		public static void _Write_PlayerCreditsSnapshot[](NetworkWriter writer, PlayerCreditsSnapshot[] value)
		{
			writer.WriteArray(value);
		}

		// Token: 0x06001E8D RID: 7821 RVA: 0x00082F2C File Offset: 0x0008112C
		public static PlayerCreditsSnapshot[] _Read_PlayerCreditsSnapshot[](NetworkReader reader)
		{
			return reader.ReadArray<PlayerCreditsSnapshot>();
		}

		// Token: 0x06001E8E RID: 7822 RVA: 0x00082F40 File Offset: 0x00081140
		public static void _Write_ChangeType(NetworkWriter writer, ChangeType value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E8F RID: 7823 RVA: 0x00082F54 File Offset: 0x00081154
		public static ChangeType _Read_ChangeType(NetworkReader reader)
		{
			return (ChangeType)reader.ReadVarInt();
		}

		// Token: 0x06001E90 RID: 7824 RVA: 0x00082F68 File Offset: 0x00081168
		public static PayoutRecord _Read_PayoutRecord(NetworkReader reader)
		{
			if (!reader.ReadBool())
			{
				return null;
			}
			return new PayoutRecord
			{
				timestamp = reader.ReadFloat(),
				playerName = reader.ReadString(),
				playerProfile = reader.ReadNetworkBehaviour<PlayerProfile>(),
				bet = reader.ReadVarLong(),
				payout = reader.ReadVarLong(),
				profit = reader.ReadVarLong(),
				isWin = reader.ReadBool(),
				isLoss = reader.ReadBool(),
				gameType = GeneratedNetworkCode._Read_CasinoGameType(reader),
				gamePosition = reader.ReadVector3()
			};
		}

		// Token: 0x06001E91 RID: 7825 RVA: 0x00083020 File Offset: 0x00081220
		public static void _Write_PayoutRecord(NetworkWriter writer, PayoutRecord value)
		{
			if (value == null)
			{
				writer.WriteBool(false);
				return;
			}
			writer.WriteBool(true);
			writer.WriteFloat(value.timestamp);
			writer.WriteString(value.playerName);
			writer.WriteNetworkBehaviour(value.playerProfile);
			writer.WriteVarLong(value.bet);
			writer.WriteVarLong(value.payout);
			writer.WriteVarLong(value.profit);
			writer.WriteBool(value.isWin);
			writer.WriteBool(value.isLoss);
			GeneratedNetworkCode._Write_CasinoGameType(writer, value.gameType);
			writer.WriteVector3(value.gamePosition);
		}

		// Token: 0x06001E92 RID: 7826 RVA: 0x000830BC File Offset: 0x000812BC
		public static void _Write_PlayerUpgradeType(NetworkWriter writer, PlayerUpgradeType value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E93 RID: 7827 RVA: 0x000830D0 File Offset: 0x000812D0
		public static PlayerUpgradeType _Read_PlayerUpgradeType(NetworkReader reader)
		{
			return (PlayerUpgradeType)reader.ReadVarInt();
		}

		// Token: 0x06001E94 RID: 7828 RVA: 0x000830E4 File Offset: 0x000812E4
		public static void _Write_NPC/NPCState(NetworkWriter writer, NPC.NPCState value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E95 RID: 7829 RVA: 0x000830F8 File Offset: 0x000812F8
		public static NPC.NPCState _Read_NPC/NPCState(NetworkReader reader)
		{
			return (NPC.NPCState)reader.ReadVarInt();
		}

		// Token: 0x06001E96 RID: 7830 RVA: 0x0008310C File Offset: 0x0008130C
		public static PlayerBuffType _Read_PlayerBuffType(NetworkReader reader)
		{
			return (PlayerBuffType)reader.ReadVarInt();
		}

		// Token: 0x06001E97 RID: 7831 RVA: 0x00083120 File Offset: 0x00081320
		public static void _Write_PlayerBuffType(NetworkWriter writer, PlayerBuffType value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E98 RID: 7832 RVA: 0x00083134 File Offset: 0x00081334
		public static void _Write_PlayerController/PlayerState(NetworkWriter writer, PlayerController.PlayerState value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E99 RID: 7833 RVA: 0x00083148 File Offset: 0x00081348
		public static PlayerController.PlayerState _Read_PlayerController/PlayerState(NetworkReader reader)
		{
			return (PlayerController.PlayerState)reader.ReadVarInt();
		}

		// Token: 0x06001E9A RID: 7834 RVA: 0x0008315C File Offset: 0x0008135C
		public static void _Write_CosmeticType(NetworkWriter writer, CosmeticType value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E9B RID: 7835 RVA: 0x00083170 File Offset: 0x00081370
		public static CosmeticType _Read_CosmeticType(NetworkReader reader)
		{
			return (CosmeticType)reader.ReadVarInt();
		}

		// Token: 0x06001E9C RID: 7836 RVA: 0x00083184 File Offset: 0x00081384
		public static void _Write_VoipManipulationManager/VoipFX(NetworkWriter writer, VoipManipulationManager.VoipFX value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E9D RID: 7837 RVA: 0x00083198 File Offset: 0x00081398
		public static VoipManipulationManager.VoipFX _Read_VoipManipulationManager/VoipFX(NetworkReader reader)
		{
			return (VoipManipulationManager.VoipFX)reader.ReadVarInt();
		}

		// Token: 0x06001E9E RID: 7838 RVA: 0x000831AC File Offset: 0x000813AC
		public static void KeyCode(NetworkWriter writer, KeyCode value)
		{
			writer.WriteVarInt((int)value);
		}

		// Token: 0x06001E9F RID: 7839 RVA: 0x000831C0 File Offset: 0x000813C0
		public static KeyCode KeyCode(NetworkReader reader)
		{
			return (KeyCode)reader.ReadVarInt();
		}

		// Token: 0x06001EA0 RID: 7840 RVA: 0x000831D4 File Offset: 0x000813D4
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitReadWriters()
		{
			Writer<byte>.write = new Action<NetworkWriter, byte>(NetworkWriterExtensions.WriteByte);
			Writer<byte?>.write = new Action<NetworkWriter, byte?>(NetworkWriterExtensions.WriteByteNullable);
			Writer<sbyte>.write = new Action<NetworkWriter, sbyte>(NetworkWriterExtensions.WriteSByte);
			Writer<sbyte?>.write = new Action<NetworkWriter, sbyte?>(NetworkWriterExtensions.WriteSByteNullable);
			Writer<char>.write = new Action<NetworkWriter, char>(NetworkWriterExtensions.WriteChar);
			Writer<char?>.write = new Action<NetworkWriter, char?>(NetworkWriterExtensions.WriteCharNullable);
			Writer<bool>.write = new Action<NetworkWriter, bool>(NetworkWriterExtensions.WriteBool);
			Writer<bool?>.write = new Action<NetworkWriter, bool?>(NetworkWriterExtensions.WriteBoolNullable);
			Writer<short>.write = new Action<NetworkWriter, short>(NetworkWriterExtensions.WriteShort);
			Writer<short?>.write = new Action<NetworkWriter, short?>(NetworkWriterExtensions.WriteShortNullable);
			Writer<ushort>.write = new Action<NetworkWriter, ushort>(NetworkWriterExtensions.WriteUShort);
			Writer<ushort?>.write = new Action<NetworkWriter, ushort?>(NetworkWriterExtensions.WriteUShortNullable);
			Writer<int>.write = new Action<NetworkWriter, int>(NetworkWriterExtensions.WriteVarInt);
			Writer<int?>.write = new Action<NetworkWriter, int?>(NetworkWriterExtensions.WriteIntNullable);
			Writer<uint>.write = new Action<NetworkWriter, uint>(NetworkWriterExtensions.WriteVarUInt);
			Writer<uint?>.write = new Action<NetworkWriter, uint?>(NetworkWriterExtensions.WriteUIntNullable);
			Writer<long>.write = new Action<NetworkWriter, long>(NetworkWriterExtensions.WriteVarLong);
			Writer<long?>.write = new Action<NetworkWriter, long?>(NetworkWriterExtensions.WriteLongNullable);
			Writer<ulong>.write = new Action<NetworkWriter, ulong>(NetworkWriterExtensions.WriteVarULong);
			Writer<ulong?>.write = new Action<NetworkWriter, ulong?>(NetworkWriterExtensions.WriteULongNullable);
			Writer<float>.write = new Action<NetworkWriter, float>(NetworkWriterExtensions.WriteFloat);
			Writer<float?>.write = new Action<NetworkWriter, float?>(NetworkWriterExtensions.WriteFloatNullable);
			Writer<double>.write = new Action<NetworkWriter, double>(NetworkWriterExtensions.WriteDouble);
			Writer<double?>.write = new Action<NetworkWriter, double?>(NetworkWriterExtensions.WriteDoubleNullable);
			Writer<decimal>.write = new Action<NetworkWriter, decimal>(NetworkWriterExtensions.WriteDecimal);
			Writer<decimal?>.write = new Action<NetworkWriter, decimal?>(NetworkWriterExtensions.WriteDecimalNullable);
			Writer<System.Half>.write = new Action<NetworkWriter, System.Half>(NetworkWriterExtensions.WriteHalf);
			Writer<string>.write = new Action<NetworkWriter, string>(NetworkWriterExtensions.WriteString);
			Writer<byte[]>.write = new Action<NetworkWriter, byte[]>(NetworkWriterExtensions.WriteBytesAndSize);
			Writer<ArraySegment<byte>>.write = new Action<NetworkWriter, ArraySegment<byte>>(NetworkWriterExtensions.WriteArraySegmentAndSize);
			Writer<Vector2>.write = new Action<NetworkWriter, Vector2>(NetworkWriterExtensions.WriteVector2);
			Writer<Vector2?>.write = new Action<NetworkWriter, Vector2?>(NetworkWriterExtensions.WriteVector2Nullable);
			Writer<Vector3>.write = new Action<NetworkWriter, Vector3>(NetworkWriterExtensions.WriteVector3);
			Writer<Vector3?>.write = new Action<NetworkWriter, Vector3?>(NetworkWriterExtensions.WriteVector3Nullable);
			Writer<Vector4>.write = new Action<NetworkWriter, Vector4>(NetworkWriterExtensions.WriteVector4);
			Writer<Vector4?>.write = new Action<NetworkWriter, Vector4?>(NetworkWriterExtensions.WriteVector4Nullable);
			Writer<Vector2Int>.write = new Action<NetworkWriter, Vector2Int>(NetworkWriterExtensions.WriteVector2Int);
			Writer<Vector2Int?>.write = new Action<NetworkWriter, Vector2Int?>(NetworkWriterExtensions.WriteVector2IntNullable);
			Writer<Vector3Int>.write = new Action<NetworkWriter, Vector3Int>(NetworkWriterExtensions.WriteVector3Int);
			Writer<Vector3Int?>.write = new Action<NetworkWriter, Vector3Int?>(NetworkWriterExtensions.WriteVector3IntNullable);
			Writer<Color>.write = new Action<NetworkWriter, Color>(NetworkWriterExtensions.WriteColor);
			Writer<Color?>.write = new Action<NetworkWriter, Color?>(NetworkWriterExtensions.WriteColorNullable);
			Writer<Color32>.write = new Action<NetworkWriter, Color32>(NetworkWriterExtensions.WriteColor32);
			Writer<Color32?>.write = new Action<NetworkWriter, Color32?>(NetworkWriterExtensions.WriteColor32Nullable);
			Writer<Quaternion>.write = new Action<NetworkWriter, Quaternion>(NetworkWriterExtensions.WriteQuaternion);
			Writer<Quaternion?>.write = new Action<NetworkWriter, Quaternion?>(NetworkWriterExtensions.WriteQuaternionNullable);
			Writer<Rect>.write = new Action<NetworkWriter, Rect>(NetworkWriterExtensions.WriteRect);
			Writer<Rect?>.write = new Action<NetworkWriter, Rect?>(NetworkWriterExtensions.WriteRectNullable);
			Writer<Plane>.write = new Action<NetworkWriter, Plane>(NetworkWriterExtensions.WritePlane);
			Writer<Plane?>.write = new Action<NetworkWriter, Plane?>(NetworkWriterExtensions.WritePlaneNullable);
			Writer<Ray>.write = new Action<NetworkWriter, Ray>(NetworkWriterExtensions.WriteRay);
			Writer<Ray?>.write = new Action<NetworkWriter, Ray?>(NetworkWriterExtensions.WriteRayNullable);
			Writer<LayerMask>.write = new Action<NetworkWriter, LayerMask>(NetworkWriterExtensions.WriteLayerMask);
			Writer<LayerMask?>.write = new Action<NetworkWriter, LayerMask?>(NetworkWriterExtensions.WriteLayerMaskNullable);
			Writer<Matrix4x4>.write = new Action<NetworkWriter, Matrix4x4>(NetworkWriterExtensions.WriteMatrix4x4);
			Writer<Matrix4x4?>.write = new Action<NetworkWriter, Matrix4x4?>(NetworkWriterExtensions.WriteMatrix4x4Nullable);
			Writer<Guid>.write = new Action<NetworkWriter, Guid>(NetworkWriterExtensions.WriteGuid);
			Writer<Guid?>.write = new Action<NetworkWriter, Guid?>(NetworkWriterExtensions.WriteGuidNullable);
			Writer<NetworkIdentity>.write = new Action<NetworkWriter, NetworkIdentity>(NetworkWriterExtensions.WriteNetworkIdentity);
			Writer<NetworkBehaviour>.write = new Action<NetworkWriter, NetworkBehaviour>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<Transform>.write = new Action<NetworkWriter, Transform>(NetworkWriterExtensions.WriteTransform);
			Writer<GameObject>.write = new Action<NetworkWriter, GameObject>(NetworkWriterExtensions.WriteGameObject);
			Writer<Uri>.write = new Action<NetworkWriter, Uri>(NetworkWriterExtensions.WriteUri);
			Writer<Texture2D>.write = new Action<NetworkWriter, Texture2D>(NetworkWriterExtensions.WriteTexture2D);
			Writer<Sprite>.write = new Action<NetworkWriter, Sprite>(NetworkWriterExtensions.WriteSprite);
			Writer<DateTime>.write = new Action<NetworkWriter, DateTime>(NetworkWriterExtensions.WriteDateTime);
			Writer<DateTime?>.write = new Action<NetworkWriter, DateTime?>(NetworkWriterExtensions.WriteDateTimeNullable);
			Writer<TimeSnapshotMessage>.write = new Action<NetworkWriter, TimeSnapshotMessage>(GeneratedNetworkCode._Write_Mirror.TimeSnapshotMessage);
			Writer<ReadyMessage>.write = new Action<NetworkWriter, ReadyMessage>(GeneratedNetworkCode._Write_Mirror.ReadyMessage);
			Writer<NotReadyMessage>.write = new Action<NetworkWriter, NotReadyMessage>(GeneratedNetworkCode._Write_Mirror.NotReadyMessage);
			Writer<AddPlayerMessage>.write = new Action<NetworkWriter, AddPlayerMessage>(GeneratedNetworkCode._Write_Mirror.AddPlayerMessage);
			Writer<SceneMessage>.write = new Action<NetworkWriter, SceneMessage>(GeneratedNetworkCode._Write_Mirror.SceneMessage);
			Writer<SceneOperation>.write = new Action<NetworkWriter, SceneOperation>(GeneratedNetworkCode._Write_Mirror.SceneOperation);
			Writer<CommandMessage>.write = new Action<NetworkWriter, CommandMessage>(GeneratedNetworkCode._Write_Mirror.CommandMessage);
			Writer<RpcMessage>.write = new Action<NetworkWriter, RpcMessage>(GeneratedNetworkCode._Write_Mirror.RpcMessage);
			Writer<SpawnMessage>.write = new Action<NetworkWriter, SpawnMessage>(GeneratedNetworkCode._Write_Mirror.SpawnMessage);
			Writer<SpawnFlags>.write = new Action<NetworkWriter, SpawnFlags>(GeneratedNetworkCode._Write_Mirror.SpawnFlags);
			Writer<ChangeOwnerMessage>.write = new Action<NetworkWriter, ChangeOwnerMessage>(GeneratedNetworkCode._Write_Mirror.ChangeOwnerMessage);
			Writer<ObjectSpawnStartedMessage>.write = new Action<NetworkWriter, ObjectSpawnStartedMessage>(GeneratedNetworkCode._Write_Mirror.ObjectSpawnStartedMessage);
			Writer<ObjectSpawnFinishedMessage>.write = new Action<NetworkWriter, ObjectSpawnFinishedMessage>(GeneratedNetworkCode._Write_Mirror.ObjectSpawnFinishedMessage);
			Writer<ObjectDestroyMessage>.write = new Action<NetworkWriter, ObjectDestroyMessage>(GeneratedNetworkCode._Write_Mirror.ObjectDestroyMessage);
			Writer<ObjectHideMessage>.write = new Action<NetworkWriter, ObjectHideMessage>(GeneratedNetworkCode._Write_Mirror.ObjectHideMessage);
			Writer<EntityStateMessage>.write = new Action<NetworkWriter, EntityStateMessage>(GeneratedNetworkCode._Write_Mirror.EntityStateMessage);
			Writer<NetworkPingMessage>.write = new Action<NetworkWriter, NetworkPingMessage>(GeneratedNetworkCode._Write_Mirror.NetworkPingMessage);
			Writer<NetworkPongMessage>.write = new Action<NetworkWriter, NetworkPongMessage>(GeneratedNetworkCode._Write_Mirror.NetworkPongMessage);
			Writer<SyncData>.write = new Action<NetworkWriter, SyncData>(SyncDataReaderWriter.WriteSyncData);
			Writer<PredictedSyncData>.write = new Action<NetworkWriter, PredictedSyncData>(PredictedSyncDataReadWrite.WritePredictedSyncData);
			Writer<ServerRequest>.write = new Action<NetworkWriter, ServerRequest>(GeneratedNetworkCode._Write_Mirror.Discovery.ServerRequest);
			Writer<ServerResponse>.write = new Action<NetworkWriter, ServerResponse>(GeneratedNetworkCode._Write_Mirror.Discovery.ServerResponse);
			Writer<PlayerCreditsSnapshot>.write = new Action<NetworkWriter, PlayerCreditsSnapshot>(PlayerCreditsSnapshotSerialization.WritePlayerCreditsSnapshot);
			Writer<NetworkStateMirror>.write = new Action<NetworkWriter, NetworkStateMirror>(SyncProjectilesMessageFunctions.Serialize);
			Writer<DissonanceNetworkMessage>.write = new Action<NetworkWriter, DissonanceNetworkMessage>(DissonanceNetworkMessageExtensions.Serialize);
			Writer<WebSocketRelayMessage>.write = new Action<NetworkWriter, WebSocketRelayMessage>(GeneratedNetworkCode._Write_WebSocketRelayMessage);
			Writer<SceneReadyMessage>.write = new Action<NetworkWriter, SceneReadyMessage>(GeneratedNetworkCode._Write_SceneReadyMessage);
			Writer<JoinGameMessage>.write = new Action<NetworkWriter, JoinGameMessage>(GeneratedNetworkCode._Write_JoinGameMessage);
			Writer<ClientScenePlayReadyMessage>.write = new Action<NetworkWriter, ClientScenePlayReadyMessage>(GeneratedNetworkCode._Write_ClientScenePlayReadyMessage);
			Writer<CardData>.write = new Action<NetworkWriter, CardData>(GeneratedNetworkCode._Write_CardData);
			Writer<Suit>.write = new Action<NetworkWriter, Suit>(GeneratedNetworkCode._Write_Suit);
			Writer<Rank>.write = new Action<NetworkWriter, Rank>(GeneratedNetworkCode._Write_Rank);
			Writer<Baccarat.CardAreaType>.write = new Action<NetworkWriter, Baccarat.CardAreaType>(GeneratedNetworkCode._Write_Baccarat/CardAreaType);
			Writer<BaccaratBetType>.write = new Action<NetworkWriter, BaccaratBetType>(GeneratedNetworkCode._Write_BaccaratBetType);
			Writer<Blackjack.CardAreaType>.write = new Action<NetworkWriter, Blackjack.CardAreaType>(GeneratedNetworkCode._Write_Blackjack/CardAreaType);
			Writer<Gradient>.write = new Action<NetworkWriter, Gradient>(GeneratedNetworkCode._Write_UnityEngine.Gradient);
			Writer<PlayerInteract>.write = new Action<NetworkWriter, PlayerInteract>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<DragonTowerButton.ButtonState>.write = new Action<NetworkWriter, DragonTowerButton.ButtonState>(GeneratedNetworkCode._Write_DragonTowerButton/ButtonState);
			Writer<BankMode>.write = new Action<NetworkWriter, BankMode>(GeneratedNetworkCode._Write_BankMode);
			Writer<List<int>>.write = new Action<NetworkWriter, List<int>>(GeneratedNetworkCode._Write_System.Collections.Generic.List`1<System.Int32>);
			Writer<SlotReel>.write = new Action<NetworkWriter, SlotReel>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<GameState>.write = new Action<NetworkWriter, GameState>(GeneratedNetworkCode._Write_GameState);
			Writer<PlayerInventory>.write = new Action<NetworkWriter, PlayerInventory>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<PlayerController>.write = new Action<NetworkWriter, PlayerController>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<NPC>.write = new Action<NetworkWriter, NPC>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<Item>.write = new Action<NetworkWriter, Item>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<SFXParams[]>.write = new Action<NetworkWriter, SFXParams[]>(GeneratedNetworkCode._Write_SFXParams[]);
			Writer<SFXParams>.write = new Action<NetworkWriter, SFXParams>(GeneratedNetworkCode._Write_SFXParams);
			Writer<GameBase>.write = new Action<NetworkWriter, GameBase>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<ChallengeSyncData[]>.write = new Action<NetworkWriter, ChallengeSyncData[]>(GeneratedNetworkCode._Write_ChallengeSyncData[]);
			Writer<ChallengeSyncData>.write = new Action<NetworkWriter, ChallengeSyncData>(GeneratedNetworkCode._Write_ChallengeSyncData);
			Writer<CasinoGameType>.write = new Action<NetworkWriter, CasinoGameType>(GeneratedNetworkCode._Write_CasinoGameType);
			Writer<ConditionStateSyncData[]>.write = new Action<NetworkWriter, ConditionStateSyncData[]>(GeneratedNetworkCode._Write_ConditionStateSyncData[]);
			Writer<ConditionStateSyncData>.write = new Action<NetworkWriter, ConditionStateSyncData>(GeneratedNetworkCode._Write_ConditionStateSyncData);
			Writer<PlayerCreditsSnapshot[]>.write = new Action<NetworkWriter, PlayerCreditsSnapshot[]>(GeneratedNetworkCode._Write_PlayerCreditsSnapshot[]);
			Writer<PlayerProfile>.write = new Action<NetworkWriter, PlayerProfile>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<ChangeType>.write = new Action<NetworkWriter, ChangeType>(GeneratedNetworkCode._Write_ChangeType);
			Writer<PayoutRecord>.write = new Action<NetworkWriter, PayoutRecord>(GeneratedNetworkCode._Write_PayoutRecord);
			Writer<PlayerUpgradeType>.write = new Action<NetworkWriter, PlayerUpgradeType>(GeneratedNetworkCode._Write_PlayerUpgradeType);
			Writer<NPC.NPCState>.write = new Action<NetworkWriter, NPC.NPCState>(GeneratedNetworkCode._Write_NPC/NPCState);
			Writer<PlayerBuffType>.write = new Action<NetworkWriter, PlayerBuffType>(GeneratedNetworkCode._Write_PlayerBuffType);
			Writer<PlayerCarry>.write = new Action<NetworkWriter, PlayerCarry>(NetworkWriterExtensions.WriteNetworkBehaviour);
			Writer<PlayerController.PlayerState>.write = new Action<NetworkWriter, PlayerController.PlayerState>(GeneratedNetworkCode._Write_PlayerController/PlayerState);
			Writer<CosmeticType>.write = new Action<NetworkWriter, CosmeticType>(GeneratedNetworkCode._Write_CosmeticType);
			Writer<VoipManipulationManager.VoipFX>.write = new Action<NetworkWriter, VoipManipulationManager.VoipFX>(GeneratedNetworkCode._Write_VoipManipulationManager/VoipFX);
			Writer<KeyCode>.write = new Action<NetworkWriter, KeyCode>(GeneratedNetworkCode._Write_UnityEngine.KeyCode);
			Reader<byte>.read = new Func<NetworkReader, byte>(NetworkReaderExtensions.ReadByte);
			Reader<byte?>.read = new Func<NetworkReader, byte?>(NetworkReaderExtensions.ReadByteNullable);
			Reader<sbyte>.read = new Func<NetworkReader, sbyte>(NetworkReaderExtensions.ReadSByte);
			Reader<sbyte?>.read = new Func<NetworkReader, sbyte?>(NetworkReaderExtensions.ReadSByteNullable);
			Reader<char>.read = new Func<NetworkReader, char>(NetworkReaderExtensions.ReadChar);
			Reader<char?>.read = new Func<NetworkReader, char?>(NetworkReaderExtensions.ReadCharNullable);
			Reader<bool>.read = new Func<NetworkReader, bool>(NetworkReaderExtensions.ReadBool);
			Reader<bool?>.read = new Func<NetworkReader, bool?>(NetworkReaderExtensions.ReadBoolNullable);
			Reader<short>.read = new Func<NetworkReader, short>(NetworkReaderExtensions.ReadShort);
			Reader<short?>.read = new Func<NetworkReader, short?>(NetworkReaderExtensions.ReadShortNullable);
			Reader<ushort>.read = new Func<NetworkReader, ushort>(NetworkReaderExtensions.ReadUShort);
			Reader<ushort?>.read = new Func<NetworkReader, ushort?>(NetworkReaderExtensions.ReadUShortNullable);
			Reader<int>.read = new Func<NetworkReader, int>(NetworkReaderExtensions.ReadVarInt);
			Reader<int?>.read = new Func<NetworkReader, int?>(NetworkReaderExtensions.ReadIntNullable);
			Reader<uint>.read = new Func<NetworkReader, uint>(NetworkReaderExtensions.ReadVarUInt);
			Reader<uint?>.read = new Func<NetworkReader, uint?>(NetworkReaderExtensions.ReadUIntNullable);
			Reader<long>.read = new Func<NetworkReader, long>(NetworkReaderExtensions.ReadVarLong);
			Reader<long?>.read = new Func<NetworkReader, long?>(NetworkReaderExtensions.ReadLongNullable);
			Reader<ulong>.read = new Func<NetworkReader, ulong>(NetworkReaderExtensions.ReadVarULong);
			Reader<ulong?>.read = new Func<NetworkReader, ulong?>(NetworkReaderExtensions.ReadULongNullable);
			Reader<float>.read = new Func<NetworkReader, float>(NetworkReaderExtensions.ReadFloat);
			Reader<float?>.read = new Func<NetworkReader, float?>(NetworkReaderExtensions.ReadFloatNullable);
			Reader<double>.read = new Func<NetworkReader, double>(NetworkReaderExtensions.ReadDouble);
			Reader<double?>.read = new Func<NetworkReader, double?>(NetworkReaderExtensions.ReadDoubleNullable);
			Reader<decimal>.read = new Func<NetworkReader, decimal>(NetworkReaderExtensions.ReadDecimal);
			Reader<decimal?>.read = new Func<NetworkReader, decimal?>(NetworkReaderExtensions.ReadDecimalNullable);
			Reader<System.Half>.read = new Func<NetworkReader, System.Half>(NetworkReaderExtensions.ReadHalf);
			Reader<string>.read = new Func<NetworkReader, string>(NetworkReaderExtensions.ReadString);
			Reader<byte[]>.read = new Func<NetworkReader, byte[]>(NetworkReaderExtensions.ReadBytesAndSize);
			Reader<ArraySegment<byte>>.read = new Func<NetworkReader, ArraySegment<byte>>(NetworkReaderExtensions.ReadArraySegmentAndSize);
			Reader<Vector2>.read = new Func<NetworkReader, Vector2>(NetworkReaderExtensions.ReadVector2);
			Reader<Vector2?>.read = new Func<NetworkReader, Vector2?>(NetworkReaderExtensions.ReadVector2Nullable);
			Reader<Vector3>.read = new Func<NetworkReader, Vector3>(NetworkReaderExtensions.ReadVector3);
			Reader<Vector3?>.read = new Func<NetworkReader, Vector3?>(NetworkReaderExtensions.ReadVector3Nullable);
			Reader<Vector4>.read = new Func<NetworkReader, Vector4>(NetworkReaderExtensions.ReadVector4);
			Reader<Vector4?>.read = new Func<NetworkReader, Vector4?>(NetworkReaderExtensions.ReadVector4Nullable);
			Reader<Vector2Int>.read = new Func<NetworkReader, Vector2Int>(NetworkReaderExtensions.ReadVector2Int);
			Reader<Vector2Int?>.read = new Func<NetworkReader, Vector2Int?>(NetworkReaderExtensions.ReadVector2IntNullable);
			Reader<Vector3Int>.read = new Func<NetworkReader, Vector3Int>(NetworkReaderExtensions.ReadVector3Int);
			Reader<Vector3Int?>.read = new Func<NetworkReader, Vector3Int?>(NetworkReaderExtensions.ReadVector3IntNullable);
			Reader<Color>.read = new Func<NetworkReader, Color>(NetworkReaderExtensions.ReadColor);
			Reader<Color?>.read = new Func<NetworkReader, Color?>(NetworkReaderExtensions.ReadColorNullable);
			Reader<Color32>.read = new Func<NetworkReader, Color32>(NetworkReaderExtensions.ReadColor32);
			Reader<Color32?>.read = new Func<NetworkReader, Color32?>(NetworkReaderExtensions.ReadColor32Nullable);
			Reader<Quaternion>.read = new Func<NetworkReader, Quaternion>(NetworkReaderExtensions.ReadQuaternion);
			Reader<Quaternion?>.read = new Func<NetworkReader, Quaternion?>(NetworkReaderExtensions.ReadQuaternionNullable);
			Reader<Rect>.read = new Func<NetworkReader, Rect>(NetworkReaderExtensions.ReadRect);
			Reader<Rect?>.read = new Func<NetworkReader, Rect?>(NetworkReaderExtensions.ReadRectNullable);
			Reader<Plane>.read = new Func<NetworkReader, Plane>(NetworkReaderExtensions.ReadPlane);
			Reader<Plane?>.read = new Func<NetworkReader, Plane?>(NetworkReaderExtensions.ReadPlaneNullable);
			Reader<Ray>.read = new Func<NetworkReader, Ray>(NetworkReaderExtensions.ReadRay);
			Reader<Ray?>.read = new Func<NetworkReader, Ray?>(NetworkReaderExtensions.ReadRayNullable);
			Reader<LayerMask>.read = new Func<NetworkReader, LayerMask>(NetworkReaderExtensions.ReadLayerMask);
			Reader<LayerMask?>.read = new Func<NetworkReader, LayerMask?>(NetworkReaderExtensions.ReadLayerMaskNullable);
			Reader<Matrix4x4>.read = new Func<NetworkReader, Matrix4x4>(NetworkReaderExtensions.ReadMatrix4x4);
			Reader<Matrix4x4?>.read = new Func<NetworkReader, Matrix4x4?>(NetworkReaderExtensions.ReadMatrix4x4Nullable);
			Reader<Guid>.read = new Func<NetworkReader, Guid>(NetworkReaderExtensions.ReadGuid);
			Reader<Guid?>.read = new Func<NetworkReader, Guid?>(NetworkReaderExtensions.ReadGuidNullable);
			Reader<NetworkIdentity>.read = new Func<NetworkReader, NetworkIdentity>(NetworkReaderExtensions.ReadNetworkIdentity);
			Reader<NetworkBehaviour>.read = new Func<NetworkReader, NetworkBehaviour>(NetworkReaderExtensions.ReadNetworkBehaviour);
			Reader<NetworkBehaviourSyncVar>.read = new Func<NetworkReader, NetworkBehaviourSyncVar>(NetworkReaderExtensions.ReadNetworkBehaviourSyncVar);
			Reader<Transform>.read = new Func<NetworkReader, Transform>(NetworkReaderExtensions.ReadTransform);
			Reader<GameObject>.read = new Func<NetworkReader, GameObject>(NetworkReaderExtensions.ReadGameObject);
			Reader<Uri>.read = new Func<NetworkReader, Uri>(NetworkReaderExtensions.ReadUri);
			Reader<Texture2D>.read = new Func<NetworkReader, Texture2D>(NetworkReaderExtensions.ReadTexture2D);
			Reader<Sprite>.read = new Func<NetworkReader, Sprite>(NetworkReaderExtensions.ReadSprite);
			Reader<DateTime>.read = new Func<NetworkReader, DateTime>(NetworkReaderExtensions.ReadDateTime);
			Reader<DateTime?>.read = new Func<NetworkReader, DateTime?>(NetworkReaderExtensions.ReadDateTimeNullable);
			Reader<TimeSnapshotMessage>.read = new Func<NetworkReader, TimeSnapshotMessage>(GeneratedNetworkCode._Read_Mirror.TimeSnapshotMessage);
			Reader<ReadyMessage>.read = new Func<NetworkReader, ReadyMessage>(GeneratedNetworkCode._Read_Mirror.ReadyMessage);
			Reader<NotReadyMessage>.read = new Func<NetworkReader, NotReadyMessage>(GeneratedNetworkCode._Read_Mirror.NotReadyMessage);
			Reader<AddPlayerMessage>.read = new Func<NetworkReader, AddPlayerMessage>(GeneratedNetworkCode._Read_Mirror.AddPlayerMessage);
			Reader<SceneMessage>.read = new Func<NetworkReader, SceneMessage>(GeneratedNetworkCode._Read_Mirror.SceneMessage);
			Reader<SceneOperation>.read = new Func<NetworkReader, SceneOperation>(GeneratedNetworkCode._Read_Mirror.SceneOperation);
			Reader<CommandMessage>.read = new Func<NetworkReader, CommandMessage>(GeneratedNetworkCode._Read_Mirror.CommandMessage);
			Reader<RpcMessage>.read = new Func<NetworkReader, RpcMessage>(GeneratedNetworkCode._Read_Mirror.RpcMessage);
			Reader<SpawnMessage>.read = new Func<NetworkReader, SpawnMessage>(GeneratedNetworkCode._Read_Mirror.SpawnMessage);
			Reader<SpawnFlags>.read = new Func<NetworkReader, SpawnFlags>(GeneratedNetworkCode._Read_Mirror.SpawnFlags);
			Reader<ChangeOwnerMessage>.read = new Func<NetworkReader, ChangeOwnerMessage>(GeneratedNetworkCode._Read_Mirror.ChangeOwnerMessage);
			Reader<ObjectSpawnStartedMessage>.read = new Func<NetworkReader, ObjectSpawnStartedMessage>(GeneratedNetworkCode._Read_Mirror.ObjectSpawnStartedMessage);
			Reader<ObjectSpawnFinishedMessage>.read = new Func<NetworkReader, ObjectSpawnFinishedMessage>(GeneratedNetworkCode._Read_Mirror.ObjectSpawnFinishedMessage);
			Reader<ObjectDestroyMessage>.read = new Func<NetworkReader, ObjectDestroyMessage>(GeneratedNetworkCode._Read_Mirror.ObjectDestroyMessage);
			Reader<ObjectHideMessage>.read = new Func<NetworkReader, ObjectHideMessage>(GeneratedNetworkCode._Read_Mirror.ObjectHideMessage);
			Reader<EntityStateMessage>.read = new Func<NetworkReader, EntityStateMessage>(GeneratedNetworkCode._Read_Mirror.EntityStateMessage);
			Reader<NetworkPingMessage>.read = new Func<NetworkReader, NetworkPingMessage>(GeneratedNetworkCode._Read_Mirror.NetworkPingMessage);
			Reader<NetworkPongMessage>.read = new Func<NetworkReader, NetworkPongMessage>(GeneratedNetworkCode._Read_Mirror.NetworkPongMessage);
			Reader<SyncData>.read = new Func<NetworkReader, SyncData>(SyncDataReaderWriter.ReadSyncData);
			Reader<PredictedSyncData>.read = new Func<NetworkReader, PredictedSyncData>(PredictedSyncDataReadWrite.ReadPredictedSyncData);
			Reader<ServerRequest>.read = new Func<NetworkReader, ServerRequest>(GeneratedNetworkCode._Read_Mirror.Discovery.ServerRequest);
			Reader<ServerResponse>.read = new Func<NetworkReader, ServerResponse>(GeneratedNetworkCode._Read_Mirror.Discovery.ServerResponse);
			Reader<PlayerCreditsSnapshot>.read = new Func<NetworkReader, PlayerCreditsSnapshot>(PlayerCreditsSnapshotSerialization.ReadPlayerCreditsSnapshot);
			Reader<NetworkStateMirror>.read = new Func<NetworkReader, NetworkStateMirror>(SyncProjectilesMessageFunctions.Deserialize);
			Reader<DissonanceNetworkMessage>.read = new Func<NetworkReader, DissonanceNetworkMessage>(DissonanceNetworkMessageExtensions.Deserialize);
			Reader<WebSocketRelayMessage>.read = new Func<NetworkReader, WebSocketRelayMessage>(GeneratedNetworkCode._Read_WebSocketRelayMessage);
			Reader<SceneReadyMessage>.read = new Func<NetworkReader, SceneReadyMessage>(GeneratedNetworkCode._Read_SceneReadyMessage);
			Reader<JoinGameMessage>.read = new Func<NetworkReader, JoinGameMessage>(GeneratedNetworkCode._Read_JoinGameMessage);
			Reader<ClientScenePlayReadyMessage>.read = new Func<NetworkReader, ClientScenePlayReadyMessage>(GeneratedNetworkCode._Read_ClientScenePlayReadyMessage);
			Reader<CardData>.read = new Func<NetworkReader, CardData>(GeneratedNetworkCode._Read_CardData);
			Reader<Suit>.read = new Func<NetworkReader, Suit>(GeneratedNetworkCode._Read_Suit);
			Reader<Rank>.read = new Func<NetworkReader, Rank>(GeneratedNetworkCode._Read_Rank);
			Reader<Baccarat.CardAreaType>.read = new Func<NetworkReader, Baccarat.CardAreaType>(GeneratedNetworkCode._Read_Baccarat/CardAreaType);
			Reader<BaccaratBetType>.read = new Func<NetworkReader, BaccaratBetType>(GeneratedNetworkCode._Read_BaccaratBetType);
			Reader<Blackjack.CardAreaType>.read = new Func<NetworkReader, Blackjack.CardAreaType>(GeneratedNetworkCode._Read_Blackjack/CardAreaType);
			Reader<Gradient>.read = new Func<NetworkReader, Gradient>(GeneratedNetworkCode._Read_UnityEngine.Gradient);
			Reader<PlayerInteract>.read = new Func<NetworkReader, PlayerInteract>(NetworkReaderExtensions.ReadNetworkBehaviour<PlayerInteract>);
			Reader<DragonTowerButton.ButtonState>.read = new Func<NetworkReader, DragonTowerButton.ButtonState>(GeneratedNetworkCode._Read_DragonTowerButton/ButtonState);
			Reader<BankMode>.read = new Func<NetworkReader, BankMode>(GeneratedNetworkCode._Read_BankMode);
			Reader<List<int>>.read = new Func<NetworkReader, List<int>>(GeneratedNetworkCode._Read_System.Collections.Generic.List`1<System.Int32>);
			Reader<SlotReel>.read = new Func<NetworkReader, SlotReel>(NetworkReaderExtensions.ReadNetworkBehaviour<SlotReel>);
			Reader<GameState>.read = new Func<NetworkReader, GameState>(GeneratedNetworkCode._Read_GameState);
			Reader<PlayerInventory>.read = new Func<NetworkReader, PlayerInventory>(NetworkReaderExtensions.ReadNetworkBehaviour<PlayerInventory>);
			Reader<PlayerController>.read = new Func<NetworkReader, PlayerController>(NetworkReaderExtensions.ReadNetworkBehaviour<PlayerController>);
			Reader<NPC>.read = new Func<NetworkReader, NPC>(NetworkReaderExtensions.ReadNetworkBehaviour<NPC>);
			Reader<Item>.read = new Func<NetworkReader, Item>(NetworkReaderExtensions.ReadNetworkBehaviour<Item>);
			Reader<SFXParams[]>.read = new Func<NetworkReader, SFXParams[]>(GeneratedNetworkCode._Read_SFXParams[]);
			Reader<SFXParams>.read = new Func<NetworkReader, SFXParams>(GeneratedNetworkCode._Read_SFXParams);
			Reader<GameBase>.read = new Func<NetworkReader, GameBase>(NetworkReaderExtensions.ReadNetworkBehaviour<GameBase>);
			Reader<ChallengeSyncData[]>.read = new Func<NetworkReader, ChallengeSyncData[]>(GeneratedNetworkCode._Read_ChallengeSyncData[]);
			Reader<ChallengeSyncData>.read = new Func<NetworkReader, ChallengeSyncData>(GeneratedNetworkCode._Read_ChallengeSyncData);
			Reader<CasinoGameType>.read = new Func<NetworkReader, CasinoGameType>(GeneratedNetworkCode._Read_CasinoGameType);
			Reader<ConditionStateSyncData[]>.read = new Func<NetworkReader, ConditionStateSyncData[]>(GeneratedNetworkCode._Read_ConditionStateSyncData[]);
			Reader<ConditionStateSyncData>.read = new Func<NetworkReader, ConditionStateSyncData>(GeneratedNetworkCode._Read_ConditionStateSyncData);
			Reader<PlayerCreditsSnapshot[]>.read = new Func<NetworkReader, PlayerCreditsSnapshot[]>(GeneratedNetworkCode._Read_PlayerCreditsSnapshot[]);
			Reader<PlayerProfile>.read = new Func<NetworkReader, PlayerProfile>(NetworkReaderExtensions.ReadNetworkBehaviour<PlayerProfile>);
			Reader<ChangeType>.read = new Func<NetworkReader, ChangeType>(GeneratedNetworkCode._Read_ChangeType);
			Reader<PayoutRecord>.read = new Func<NetworkReader, PayoutRecord>(GeneratedNetworkCode._Read_PayoutRecord);
			Reader<PlayerUpgradeType>.read = new Func<NetworkReader, PlayerUpgradeType>(GeneratedNetworkCode._Read_PlayerUpgradeType);
			Reader<NPC.NPCState>.read = new Func<NetworkReader, NPC.NPCState>(GeneratedNetworkCode._Read_NPC/NPCState);
			Reader<PlayerBuffType>.read = new Func<NetworkReader, PlayerBuffType>(GeneratedNetworkCode._Read_PlayerBuffType);
			Reader<PlayerCarry>.read = new Func<NetworkReader, PlayerCarry>(NetworkReaderExtensions.ReadNetworkBehaviour<PlayerCarry>);
			Reader<PlayerController.PlayerState>.read = new Func<NetworkReader, PlayerController.PlayerState>(GeneratedNetworkCode._Read_PlayerController/PlayerState);
			Reader<CosmeticType>.read = new Func<NetworkReader, CosmeticType>(GeneratedNetworkCode._Read_CosmeticType);
			Reader<VoipManipulationManager.VoipFX>.read = new Func<NetworkReader, VoipManipulationManager.VoipFX>(GeneratedNetworkCode._Read_VoipManipulationManager/VoipFX);
			Reader<KeyCode>.read = new Func<NetworkReader, KeyCode>(GeneratedNetworkCode._Read_UnityEngine.KeyCode);
		}
	}
}
