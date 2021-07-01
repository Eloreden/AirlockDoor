using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AirlockDoor
{
    //Add anim override (necesary to prevent game crash)
    [HarmonyPatch(typeof(Door), "OnPrefabInit")]
    internal class AirlockDoor_OnPrefabInit
    {
        private static void Postfix(ref Door __instance)
        {
            __instance.overrideAnims = new KAnimFile[]
            {
                Assets.GetAnim("airlock_mechanized_door_kanim")
            };
        }
    }

    [HarmonyPatch(typeof(Door), "OnCleanUp")]
    internal class AirlockDoor_OnCleanUp
    {
        private static bool Prefix(Door __instance)
        {
            if (__instance.gameObject == null)
                return true;

            if (!__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
                return true;

            foreach (int cell in __instance.building.PlacementCells)
            {
                SimMessages.ClearCellProperties(cell, 3);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Door), "Sim200ms")]
    internal class AirlockDoor_Sim200ms
    {


        public static bool Prefix(Door __instance, float dt)
        {

            if ((UnityEngine.Object)__instance == (UnityEngine.Object)null)
                return true;

            return false;
            //if (__instance.doorOpenLiquidRefreshHack)
            //{
            //    this.doorOpenLiquidRefreshTime -= dt;
            //    if ((double)this.doorOpenLiquidRefreshTime <= 0.0)
            //    {
            //        this.doorOpenLiquidRefreshHack = false;
            //        foreach (int placementCell in this.building.PlacementCells)
            //            Pathfinding.Instance.AddDirtyNavGridCell(placementCell);
            //    }
            //}
            //if (__instance.applyLogicChange)
            //{
            //    __instance.applyLogicChange = false;
            //    __instance.ApplyRequestedControlState();
            //}
            //if (!__instance.do_melt_check)
            //    return;
            //StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
            //HandleVector<int>.Handle handle = structureTemperatures.GetHandle(__instance.gameObject);
            //if (handle.IsValid() && structureTemperatures.IsBypassed(handle))
            //{
            //    foreach (int placementCell in __instance.building.PlacementCells)
            //    {
            //        if (!Grid.Solid[placementCell])
            //        {
            //            Util.KDestroyGameObject((Component)__instance);
            //            break;
            //        }
            //    }
            //}
        }
    }

    //[HarmonyPatch(typeof(Door), "OnSpawn")]
    //internal class AirlockDoor_OnSpawn
    //{
    //    private static void Postfix(Door __instance)
    //    {
    //        Console.WriteLine("[OnSpawn - MOD VANILLA]  Add Item on DoorPosition ");
    //        foreach (int cell in __instance.building.PlacementCells)
    //        {
    //            DoorPosition.cells.Add(cell);
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(Door), "SetSimState")]
    internal class AirlockDoor_SetSimState
    {
        private static bool Prefix(Door __instance, bool is_door_open, IList<int> cells)
        {
            if (__instance.gameObject == null)
                return true;

            if (!__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
            {
                Console.WriteLine($"MESSAGGIO CHE NON DEVE FAR COMPARIRE ALTRO: {__instance.gameObject}");
                return true;
            }

            Door.ControlState controlState = Traverse.Create(__instance).Field("controlState").GetValue<Door.ControlState>();

            Debug.Log($"Vanilla - Door Control State: {controlState} - DoorType : {__instance.doorType} - Cells:{cells[0]}");

            //if (doorType == Door.DoorType.Internal || controlState == Door.ControlState.Opened)
            //{ return true; }


            PrimaryElement element = __instance.GetComponent<PrimaryElement>();
            float mass_per_cell = element.Mass / cells.Count;


            for (int i = 0; i < cells.Count; i++)
            {
                int cell = cells[i];
                switch (__instance.doorType)
                {
                    case Door.DoorType.Pressure:
                    case Door.DoorType.ManualPressure:
                    case Door.DoorType.Internal:
                        World.Instance.groundRenderer.MarkDirty(cell);
                        if (is_door_open)
                        {
                            MethodInfo method_opened = AccessTools.Method(typeof(Door), "OnSimDoorOpened", null, null);
                            System.Action cb_opened = (System.Action)Delegate.CreateDelegate(typeof(System.Action), __instance, method_opened);
                            HandleVector<Game.CallbackInfo>.Handle handle = Game.Instance.callbackManager.Add(new Game.CallbackInfo(cb_opened));
                            SimMessages.Dig(cell, handle.index, true);
                            if (__instance.ShouldBlockFallingSand)
                            {
                                SimMessages.ClearCellProperties(cell, 4);
                                break;
                            }
                            HandleVector<Game.CallbackInfo>.Handle handle2 = Game.Instance.callbackManager.Add(new Game.CallbackInfo(cb_opened));
                            SimMessages.ReplaceAndDisplaceElement(cell, element.ElementID, CellEventLogger.Instance.DoorClose, mass_per_cell, element.Temperature, byte.MaxValue, 0, handle2.index);
                            SimMessages.SetCellProperties(cell, 4);
                            break;
                        }
                        Debug.Log("Vanilla - Closing");
                        MethodInfo method_closed = AccessTools.Method(typeof(Door), "OnSimDoorClosed", null, null);
                        System.Action cb_closed = (System.Action)Delegate.CreateDelegate(typeof(System.Action), __instance, method_closed);
                        HandleVector<Game.CallbackInfo>.Handle handle1 = Game.Instance.callbackManager.Add(new Game.CallbackInfo(cb_closed, false));
                        SimMessages.ReplaceAndDisplaceElement(cell, element.ElementID, CellEventLogger.Instance.DoorClose, mass_per_cell, element.Temperature, byte.MaxValue, 0, handle1.index);
                        SimMessages.SetCellProperties(cell, 4);
                        break;
                }
            }
            return false;
        }
    }

}
