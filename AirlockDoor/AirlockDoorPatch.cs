using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Database;
using ProcGen;
using STRINGS;

using BUILDINGS = TUNING.BUILDINGS;
using System;

namespace AirlockDoor
{
    public class AirlockDoorPatch
    {
        public static bool didStartupBuilding;
        public static bool didStartupDb;

        public class AirlockMod : KMod.UserMod2
        {
            public override void OnLoad(Harmony harmony)
            {
                harmony.PatchAll();
                Console.WriteLine($"------------------- [ AIRLOCK DOOR - Vanilla ] OnLoad");
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartupBuilding)
                {
                    Console.WriteLine($"[ AIRLOCK DOOR - Vanilla ] Prefix For Add Building");
                    AddBuildingStrings(AirlockDoorConfig.ID, AirlockDoorConfig.DisplayName,
                        AirlockDoorConfig.Description, AirlockDoorConfig.Effect);
                    AddBuildingToBuildMenu("Base", AirlockDoorConfig.ID);
                    didStartupBuilding = true;
                }
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void PostFix()
            {
                Console.WriteLine($"[ AIRLOCK DOOR - Vanilla ] {Db.Get().Techs.Get("DirectedAirStreams").unlockedItemIDs}");
                Db.Get().Techs.Get("DirectedAirStreams").unlockedItemIDs.Add(AirlockDoorConfig.ID);
            }
        }

        public static void AddBuildingToBuildMenu(HashedString category, string buildingid, string addAfterId = null)
        {
            Console.WriteLine($"[ AIRLOCK DOOR - Vanilla ] AddBuildingToBuildMenu");
            var i = BUILDINGS.PLANORDER.FindIndex(x => x.category == category);
            if (i == -1)
            {
                return;
            }

            var planorderlist = BUILDINGS.PLANORDER[i].data as IList<string>;
            if (planorderlist == null)
            {
                return;
            }

            if (addAfterId == null)
            {
                planorderlist.Add(buildingid);
            }
            else
            {
                var neigh_i = planorderlist.IndexOf(addAfterId);
                if (neigh_i == -1)
                {
                    return;
                }

                planorderlist.Insert(neigh_i + 1, buildingid);
            }
        }

        public static void AddBuildingStrings(string id, string name, string desc, string effect)
        {
            Console.WriteLine($"[ AIRLOCK DOOR ] AddBuildingStrings");
            var id_up = id.ToUpperInvariant();

            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.NAME", UI.FormatAsLink(name, id));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.DESC", desc);
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.EFFECT", effect);
        }
    }
}
