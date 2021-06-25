using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Database;
using ProcGen;
using STRINGS;

using BUILDINGS = TUNING.BUILDINGS;

namespace AirlockDoor
{
    public class AirlockDoorPatch
    {
        public static bool didStartupBuilding;
        public static bool didStartupDb;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            //Vanilla
            //public static void Prefix()
            //{
            //    if (!didStartupBuilding)
            //    {

            //        OniUtils.AddBuildingStrings(AirlockDoorConfig.ID, AirlockDoorConfig.DisplayName,
            //            AirlockDoorConfig.Description, AirlockDoorConfig.Effect);
            //        OniUtils.AddBuildingToBuildMenu("Base", AirlockDoorConfig.ID);
            //        didStartupBuilding = true;
            //    }
            //}

            //SpaceOut
            public static void Prefix()
            {
                AddBuildingStrings(AirlockDoorConfig.ID, AirlockDoorConfig.DisplayName,
                    AirlockDoorConfig.Description, AirlockDoorConfig.Effect);
                AddBuildingToBuildMenu("Base", AirlockDoorConfig.ID);
                didStartupBuilding = true;
            }
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
            public static void Prefix()
            {
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
            var id_up = id.ToUpperInvariant();

            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.NAME", UI.FormatAsLink(name, id));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.DESC", desc);
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.EFFECT", effect);
        }
        #endregion
    }
}
