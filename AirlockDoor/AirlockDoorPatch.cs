﻿using HarmonyLib;
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
                AddBuildingStrings(AirlockDoorConfig.ID, AirlockDoorConfig.DisplayName, AirlockDoorConfig.Description, AirlockDoorConfig.Effect);
                AddBuildingToBuildMenu("Base", AirlockDoorConfig.ID);
                Console.WriteLine($"[ AIRLOCK DOOR - Vanilla ] OnLoad");
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            //Vanilla
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

            //SpaceOut
            //public static void Postfix()
            //{
            //    Console.WriteLine($"[ AIRLOCK DOOR ] Prefix For Add Building");
            //    AddBuildingStrings(AirlockDoorConfig.ID, AirlockDoorConfig.DisplayName,
            //        AirlockDoorConfig.Description, AirlockDoorConfig.Effect);
            //    AddBuildingToBuildMenu("Base", AirlockDoorConfig.ID);
            //    didStartupBuilding = true;
            //}
        }

        ////vanilla
        //[HarmonyPatch(typeof(Db), "Initialize")]
        //public static class Db_Initialize_Patch
        //{
        //    public static void Prefix()
        //    {
        //        if (!didStartupDb)
        //        {
        //            OniUtils.AddBuildingToTech("DirectedAirStreams", AirlockDoorConfig.ID);

        //            didStartupDb = true;
        //        }
        //    }
        //}

        //Space Out
        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void PostFix()
            {
                Console.WriteLine($"[ AIRLOCK DOOR - Vanilla ] {Db.Get().Techs.Get("DirectedAirStreams").unlockedItemIDs}");
                Db.Get().Techs.Get("DirectedAirStreams").unlockedItemIDs.Add(AirlockDoorConfig.ID);
            }
        }

        #region Add Building to Menu
        //Vanilla
        //public static void AddBuildingToTech(string tech, string buildingid)
        //{
        //    var techlist = new List<string>(Techs.TECH_GROUPING[tech]);
        //    techlist.Add(buildingid);
        //    Techs.TECH_GROUPING[tech] = techlist.ToArray();
        //}

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
        #endregion
    }
}
