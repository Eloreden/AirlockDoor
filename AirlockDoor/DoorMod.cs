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
            // Applica l'override anim SOLO alle nostre porte (full + half), ognuna con la
            // propria kanim. Per tutte le altre porte vanilla GetOverrideAnim ritorna null.
            string anim = Helpers.GetOverrideAnim(__instance);
            if (anim == null)
                return;

            __instance.overrideAnims = new KAnimFile[]
            {
                Assets.GetAnim(anim)
            };
        }
    }

    [HarmonyPatch(typeof(Door), "OnCleanUp")]
    internal class AirlockDoor_OnCleanUp
    {
        private static bool Prefix(Door __instance)
        {
            if (!Helpers.IsAirlockDoor(__instance))
                return true;

            foreach (int cell in __instance.building.PlacementCells)
            {
                // Azzera i bit impermeabile (4) + porta (8), come fa Door.OnCleanUp vanilla.
                // (prima azzerava 3 = bit 1+2, che NON includono il bit impermeabile -> la cella restava sigillata)
                SimMessages.ClearCellProperties(cell, 12);
                // Rimuovi la massa solida piazzata dalla porta, così gas/liquidi tornano a passare.
                if (Grid.Element[cell].IsSolid)
                    SimMessages.ReplaceAndDisplaceElement(cell, SimHashes.Vacuum, CellEventLogger.Instance.DoorOpen, 0f);
                Pathfinding.Instance.AddDirtyNavGridCell(cell);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Door), "Sim200ms")]
    internal class AirlockDoor_Sim200ms
    {


        public static bool Prefix(Door __instance, float dt)
        {

            if (__instance == null)
                return true;

            // Salta il Sim200ms vanilla SOLO per la nostra porta. Per tutte le altre porte
            // deve girare l'originale, altrimenti si rompono automazione/melt check/refresh liquidi.
            if (!Helpers.IsAirlockDoor(__instance))
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
            if (!Helpers.IsAirlockDoor(__instance))
                return true;

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
