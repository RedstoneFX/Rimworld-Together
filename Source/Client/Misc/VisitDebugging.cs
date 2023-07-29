using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using Verse;

namespace RimworldTogether.GameClient.Misc
{
    [HarmonyPatch(typeof(Map), "FinalizeLoading")]
    public static class MapFinalizeLoadingPatch
    {
        public static Caravan SpawnCaravanAt(int tile, Faction faction)
        {
            // create list of pawns
            List<Pawn> pawns = new List<Pawn>();

            // Create three pawns
            for (int i = 0; i < 3; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, faction);
                pawns.Add(pawn);
            }

            // Make the caravan
            Caravan caravan = CaravanMaker.MakeCaravan(pawns, faction, tile, true);
            return caravan;
        }

        public static void Postfix()
        {
            if (!CommandLineParamsManager.instantVisit)
            {
                NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().RegisterReplyHandler((data, cb, __) =>
                {
                    GameLogger.Debug.Log($"Replying to visit request for {data.targetToRelayTo}");
                    cb(new(true, data.targetToRelayTo));
                });
                return;
            }

            new Task(async () =>
            {
                try
                {
                    while (!NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().readToVisit)
                    {
                        Thread.Sleep(100);
                        // var reply = await NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().SendWithReplyAsync(new(new(), 2));
                        // NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().readToVisit = reply.data.data;
                        
                        NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().SendWithReply(new(new(), 2), reply => { NetworkCallbackHolder.GetType<VisitCallbackCommunicator>().readToVisit = reply.data; });
                    }

                    var goal = "B's settlement";
                    var goalSettlement = FindSettlementByName(goal);
                    if (goalSettlement != null)
                    {
                        Log.Message($"Found settlement {goalSettlement.Name} at {goalSettlement.Tile}");
                        var tile = goalSettlement.Tile;
                        ClientValues.chosenSettlement = goalSettlement;
                        ClientValues.chosenCaravan = SpawnCaravanAt(tile, Faction.OfPlayer);
                        VisitManager.RequestVisit();
                    }
                    else
                    {
                        Log.Error($"No settlement found with name {goal}");
                    }
                }
                catch (Exception e)
                {
                    GameLogger.Error(e.ToString());
                }
            }).Start();
        }

        public static Settlement FindSettlementByName(string name)
        {
            foreach (Settlement settlement in Find.WorldObjects.Settlements)
            {
                if (settlement.Name == name)
                {
                    return settlement;
                }
            }

            return null; // return null if no settlement is found with the given name
        }
    }
}