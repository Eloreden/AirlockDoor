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
        public static bool didStartupBuilding = false;
        public static bool didStartupDb = false;

        public class AirlockMod : KMod.UserMod2
        {
            public override void OnLoad(Harmony harmony)
            {
                harmony.PatchAll();
                Console.WriteLine($"------------------- [ AIRLOCK DOOR - Vanilla ] OnLoad");
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
            }
            public static void Postfix()
            {
                Helpers.doorBuildMenu("AirlockMechanizedDoor", "Base", "PressureDoor");
                Helpers.doorTechTree("AirlockMechanizedDoor", "HVAC");
            }
        }
    }
}
