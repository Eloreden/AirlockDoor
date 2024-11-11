﻿using HarmonyLib;
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
                //AirlockDoorCell.CellList.AddRange(__instance.building.PlacementCells);

                __instance.overrideAnims = new KAnimFile[]
                {
                  Assets.GetAnim("airlock_mechanized_door_kanim")
                };
            }

        }
    }

    //public static class AirlockDoorCell
    //{
    //    public static List<int> CellList { get; set; } = new List<int>();
    //}

    //[HarmonyPatch(typeof(Door), "OnCleanUp")]
    //internal class AirlockDoor_OnCleanUp
    //{
    //    private static void Postfix(Door __instance)
    //    {
    //        if (__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
    //        {
    //            foreach (int cell in __instance.building.PlacementCells)
    //            {
    //                SimMessages.ClearCellProperties(cell, 3);
    //            }
    //        }
    //    }
    //}


    //[HarmonyPatch(typeof(NavTableValidator), "IsCellPassable")]
    //internal class AirlockDoor_IsCellPassable
    //{
    //    public static bool Prefix(NavTableValidator __instance,ref bool __result,int cell, bool is_dupe)
    //    {
    //        if (AirlockDoorCell.CellList.Contains(cell))
    //        {
    //            //Console.WriteLine($" ------------- IsCellPassable {cell} --------------- ");
    //            __result = false;
    //            return false;
    //        }
    //        return true;
    //    }
    //}

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

    //[HarmonyPatch(typeof(Door), "SetSimState")]
    //internal class AirlockDoor_SetSimState
    //{
    //    private static bool Prefix(Door __instance, bool is_door_open, IList<int> cells)
    //    {
    //        if (__instance.gameObject == null)
    //            return true;

    //        if (!__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
    //            return true;

    //        Door.ControlState controlState = Traverse.Create(__instance).Field("controlState").GetValue<Door.ControlState>();
    //        Door.DoorType doorType = __instance.doorType;

    //        //Debug.Log($"Door Control State: {controlState} - DoorType : {doorType}");

    //        if (doorType == Door.DoorType.Internal || controlState == Door.ControlState.Opened)
    //        { return true; }


    //        PrimaryElement element = __instance.GetComponent<PrimaryElement>();
    //        float mass_per_cell = element.Mass / cells.Count;

    //        for (int i = 0; i < cells.Count; i++)
    //        {
    //            int cell = cells[i];
    //            // On opening
    //            if (is_door_open)
    //            {
    //                //Debug.Log("Opening");
    //                MethodInfo method_opened = AccessTools.Method(typeof(Door), "OnSimDoorOpened", null, null);
    //                System.Action cb_opened = (System.Action)Delegate.CreateDelegate(typeof(System.Action), __instance, method_opened);
    //                HandleVector<Game.CallbackInfo>.Handle handle = Game.Instance.callbackManager.Add(new Game.CallbackInfo(cb_opened, false));
    //                SimMessages.ReplaceAndDisplaceElement(cell, element.ElementID, CellEventLogger.Instance.DoorOpen, mass_per_cell, element.Temperature, byte.MaxValue, 0, handle.index);
    //            }
    //            // On closing
    //            else
    //            {
    //                //Debug.Log("Closing");
    //                MethodInfo method_closed = AccessTools.Method(typeof(Door), "OnSimDoorClosed", null, null);
    //                System.Action cb_closed = (System.Action)Delegate.CreateDelegate(typeof(System.Action), __instance, method_closed);
    //                HandleVector<Game.CallbackInfo>.Handle handle = Game.Instance.callbackManager.Add(new Game.CallbackInfo(cb_closed, false));
    //                SimMessages.ReplaceAndDisplaceElement(cell, element.ElementID, CellEventLogger.Instance.DoorClose, mass_per_cell, element.Temperature, byte.MaxValue, 0, handle.index);
    //            }
    //            SimMessages.SetCellProperties(cell, 4);
    //        }

    //        return false;
    //    }
    //}

}