using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AirlockDoor
{
    // Add anim override (necesary to prevent game crash)
    [HarmonyPatch(typeof(Door), "OnPrefabInit")]
    internal class AirlockDoor_OnPrefabInit
    {
        private static void Postfix(ref Door __instance)
        {
            if (__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
            {
                __instance.overrideAnims = new KAnimFile[]
                {
                  Assets.GetAnim("airlock_mechanized_door_kanim")
                };
            }

        }
    }

    [HarmonyPatch(typeof(Door), "SetWorldState")]
    internal class AirlockDoor_SetWorldState
    {
        private static void Postfix(Door __instance)
        {
            // If the attached gameobject doesn't exist, exit here
            if (__instance.gameObject == null) return;

            if (!__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
                return;

            Door.DoorType doorType = __instance.doorType;
            if (doorType <= Door.DoorType.ManualPressure || doorType == Door.DoorType.Sealed)
            {
                for (var i = 0; i < __instance.building.PlacementCells.Length; i++)
                {
                    var offsetCell = __instance.building.PlacementCells[i];
                    SimMessages.ClearCellProperties(offsetCell, 1);
                    SimMessages.ClearCellProperties(offsetCell, 2);
                    SimMessages.ClearCellProperties(offsetCell, 4);
                    SimMessages.SetCellProperties(offsetCell, (byte)(__instance.CurrentState == Door.ControlState.Auto ? 7 : 4));
                }
            }
        }
    }
}
