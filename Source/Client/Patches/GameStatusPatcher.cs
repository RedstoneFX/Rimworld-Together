﻿using HarmonyLib;
using RimWorld.Planet;
using Shared;
using Verse;

namespace GameClient
{
    public class GameStatusPatcher
    {
        [HarmonyPatch(typeof(Game), "InitNewGame")]
        public static class InitModePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Game __instance)
            {
                if (Network.isConnectedToServer)
                {
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();

                    SettlementData settlementData = new SettlementData();
                    settlementData.tile = __instance.CurrentMap.Tile.ToString();
                    settlementData.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();

                    if (ClientValues.needsToGenerateWorld)
                    {
                        WorldGeneratorManager.SendWorldToServer();
                        ClientValues.ToggleGenerateWorld(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Game), "LoadGame")]
        public static class LoadModePatch
        {
            [HarmonyPostfix]
            public static void GetIDFromExistingGame()
            {
                if (Network.isConnectedToServer)
                {
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();

                    PlanetManager.BuildPlanet();

                    ClientValues.ToggleReadyToPlay(true);
                }
            }
        }

        [HarmonyPatch(typeof(SettleInEmptyTileUtility), "Settle")]
        public static class SettlePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Caravan caravan)
            {
                if (Network.isConnectedToServer)
                {
                    SettlementData settlementData = new SettlementData();
                    settlementData.tile = caravan.Tile.ToString();
                    settlementData.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(SettleInExistingMapUtility), "Settle")]
        public static class SettleInMapPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Map map)
            {
                if (Network.isConnectedToServer)
                {
                    SettlementData settlementData = new SettlementData();
                    settlementData.tile = map.Tile.ToString();
                    settlementData.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(SettlementAbandonUtility), "Abandon")]
        public static class AbandonPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Settlement settlement)
            {
                if (Network.isConnectedToServer)
                {
                    SettlementData settlementData = new SettlementData();
                    settlementData.tile = settlement.Tile.ToString();
                    settlementData.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Remove).ToString();

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();
                }
            }
        }
    }
}
