using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AirlockDoor
{
    //// Add anim override (necesary to prevent game crash)
    //[HarmonyPatch(typeof(Door), "OnPrefabInit")]
    //internal class AirlockDoor_OnPrefabInit
    //{
    //    private static void Postfix(ref Door __instance)
    //    {
    //        __instance.overrideAnims = new KAnimFile[]
    //        {
    //            Assets.GetAnim("airlock_mechanized_door_kanim")
    //        };
    //    }
    //}

    [HarmonyPatch(typeof(Door), "OnCleanUp")]
    internal class AirlockDoor_OnCleanUp
    {
        private static bool Prefix(Door __instance)
        {
            foreach (int cell in __instance.building.PlacementCells)
            {
                SimMessages.ClearCellProperties(cell, 3);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Door), "OnSpawn")]
    internal class AirlockDoor_OnSpawn
    {
        private static void Postfix(Door __instance)
        {
            Console.WriteLine("[OnSpawn - MOD VANILLA]  Add Item on DoorPosition ");
            foreach (int cell in __instance.building.PlacementCells)
            {
                DoorPosition.cells.Add(cell);
            }
        }
    }

    public static class DoorPosition
    {
        public static List<int> cells = new List<int>();
    }

    [HarmonyPatch(typeof(Door), "SetPassableState")]
    internal class AirlockDoor_SetPassableState
    {

        private static bool Prefix(Door __instance, bool is_door_open, IList<int> cells)
        {
            if (!__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
                return true;

            Console.WriteLine($"[ MOD VANILLA ] - SetPassableState - {DoorPosition.cells.Count}");
            Door.ControlState controlState = Traverse.Create(__instance).Field("controlState").GetValue<Door.ControlState>();
            bool doorOpenLiquidRefreshHack = Traverse.Create(__instance).Field("doorOpenLiquidRefreshHack").GetValue<bool>();
            float doorOpenLiquidRefreshTime = Traverse.Create(__instance).Field("doorOpenLiquidRefreshTime").GetValue<float>();
            Door.DoorType doorType = __instance.doorType;
            for (int index = 0; index < cells.Count; ++index)
            {
                int cell = cells[index];
                switch (__instance.doorType)
                {
                    case Door.DoorType.Pressure:
                    case Door.DoorType.ManualPressure:
                    case Door.DoorType.Sealed:
                        bool passable = controlState != Door.ControlState.Locked;
                        bool solid = !is_door_open;
                        if (__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
                        {
                            Console.WriteLine($"[MOD VANILLA ] - Override porta Meccanizzata");
                            solid = false;
                            passable = true;
                        }
                        else
                        {
                            return true;
                        }

                        Grid.CritterImpassable[cell] = controlState != Door.ControlState.Opened;
                        Game.Instance.SetDupePassableSolid(cell, passable, solid);

                        break;
                    case Door.DoorType.Internal:
                        Grid.CritterImpassable[cell] = controlState != Door.ControlState.Opened;
                        Grid.DupeImpassable[cell] = controlState == Door.ControlState.Locked;
                        break;
                }
                Pathfinding.Instance.AddDirtyNavGridCell(cell);
            }
            return false;
        }
    }


    [HarmonyPatch(typeof(Door), "SetSimState")]
    internal class AirlockDoor_SetSimState
    {
        private static bool Prefix(Door __instance, bool is_door_open, IList<int> cells)
        {
            if (__instance.gameObject == null)
                return true;

            if (!__instance.gameObject.ToString().Contains("AirlockMechanizedDoorComplete"))
                return true;

            Door.ControlState controlState = Traverse.Create(__instance).Field("controlState").GetValue<Door.ControlState>();
            Door.DoorType doorType = __instance.doorType;

            Debug.Log($"Vanilla - Door Control State: {controlState} - DoorType : {doorType}");

            if (doorType == Door.DoorType.Internal || controlState == Door.ControlState.Opened)
            { return true; }


            PrimaryElement element = __instance.GetComponent<PrimaryElement>();
            float mass_per_cell = element.Mass / cells.Count;


            for (int i = 0; i < cells.Count; i++)
            {
                int cell = cells[i];
                World.Instance.groundRenderer.MarkDirty(cell);
                if (is_door_open)
                {
                    //Debug.Log("Vanilla - Opening");
                    MethodInfo method_opened = AccessTools.Method(typeof(Door), "OnSimDoorOpened", null, null);
                    System.Action cb_opened = (System.Action)Delegate.CreateDelegate(typeof(System.Action), __instance, method_opened);
                    HandleVector<Game.CallbackInfo>.Handle handle = Game.Instance.callbackManager.Add(new Game.CallbackInfo(cb_opened, false));
                    if (__instance.ShouldBlockFallingSand)
                    {
                        Console.WriteLine($"__instance.ShouldBlockFallingSand: {__instance.ShouldBlockFallingSand}");
                        SimMessages.ClearCellProperties(cell, 4);
                        break;
                    }
                    ReplaceAndDisplaceElement(__instance, cell, element.ElementID, CellEventLogger.Instance.DoorOpen, mass_per_cell, element.Temperature, byte.MaxValue, 0, handle.index);
                }
                else
                {
                    //Debug.Log("Vanilla - Closing");
                    MethodInfo method_closed = AccessTools.Method(typeof(Door), "OnSimDoorClosed", null, null);
                    System.Action cb_closed = (System.Action)Delegate.CreateDelegate(typeof(System.Action), __instance, method_closed);
                    HandleVector<Game.CallbackInfo>.Handle handle = Game.Instance.callbackManager.Add(new Game.CallbackInfo(cb_closed, false));
                    ReplaceAndDisplaceElement(__instance, cell, element.ElementID, CellEventLogger.Instance.DoorClose, mass_per_cell, element.Temperature, byte.MaxValue, 0, handle.index);
                }
                SimMessages.SetCellProperties(cell, 4);
            }

            return false;
        }



        private static void ReplaceAndDisplaceElement(Door obj, int gameCell, SimHashes new_element, CellElementEvent ev, float mass, float temperature = -1f, byte disease_idx = 255, int disease_count = 0, int callbackIdx = -1)
        {
            Console.WriteLine($"[AIRLOCK DOOR VANILLA CELL]  Call Element Event - {ev}");
            int elementIndex = SimMessages.GetElementIndex(new_element);

            if (elementIndex == -1)
                return;
            Element element = ElementLoader.elements[elementIndex];
            Console.WriteLine($"[AIRLOCK DOOR VANILLA CELL]  element index - {element.name}");
            float temperature1 = (double)temperature != -1.0 ? temperature : element.defaultValues.temperature;
            ModifyCell(gameCell, elementIndex, temperature1, mass, disease_idx, disease_count, SimMessages.ReplaceType.ReplaceAndDisplace, do_vertical_solid_displacement: true, callbackIdx: callbackIdx);
        }

        unsafe private static void ModifyCell(int gameCell, int elementIdx, float temperature, float mass, byte disease_idx, int disease_count, SimMessages.ReplaceType replace_type = SimMessages.ReplaceType.None, bool do_vertical_solid_displacement = false, int callbackIdx = -1)
        {
            Console.WriteLine($"[AIRLOCK DOOR VANILLA CELL] - ModifyCell  - {gameCell} - ");
            if (!Grid.IsValidCell(gameCell))
            {
                UnityEngine.Debug.AssertFormat(false, "Invalid cell: {0}", (object)gameCell);
            }
            else
            {
                Element element = ElementLoader.elements[elementIdx];
                if ((double)element.maxMass == 0.0 && (double)mass > (double)element.maxMass)
                {
                    Debug.LogWarningFormat("Invalid cell modification (mass greater than element maximum): Cell={0}, EIdx={1}, T={2}, M={3}, {4} max mass = {5}", (object)gameCell, (object)elementIdx, (object)temperature, (object)mass, (object)element.id, (object)element.maxMass);
                    mass = element.maxMass;
                }
                if ((double)temperature < 0.0 || (double)temperature > 10000.0)
                {
                    Debug.LogWarningFormat("Invalid cell modification (temp out of bounds): Cell={0}, EIdx={1}, T={2}, M={3}, {4} default temp = {5}", (object)gameCell, (object)elementIdx, (object)temperature, (object)mass, (object)element.id, (object)element.defaultValues.temperature);
                    temperature = element.defaultValues.temperature;
                }
                if ((double)temperature == 0.0 && (double)mass > 0.0)
                {
                    Debug.LogWarningFormat("Invalid cell modification (zero temp with non-zero mass): Cell={0}, EIdx={1}, T={2}, M={3}, {4} default temp = {5}", (object)gameCell, (object)elementIdx, (object)temperature, (object)mass, (object)element.id, (object)element.defaultValues.temperature);
                    temperature = element.defaultValues.temperature;
                }

                //Console.WriteLine($"- gameCell: {gameCell} \n- callbackIdx: {callbackIdx} \n- temperature: {temperature} \n- mass: {mass} \n- elementIdx: {elementIdx} \n- replaceType: {replace_type} \n- diseaseIdx: {disease_count} \n- addSubType: {do_vertical_solid_displacement} \n");

                ModifyCellMessage* modifyCellMessagePtr = stackalloc ModifyCellMessage[1];
                modifyCellMessagePtr->cellIdx = gameCell;
                modifyCellMessagePtr->callbackIdx = callbackIdx;
                modifyCellMessagePtr->temperature = temperature;
                modifyCellMessagePtr->mass = mass;
                modifyCellMessagePtr->elementIdx = (byte)elementIdx;
                modifyCellMessagePtr->replaceType = (byte)replace_type;
                modifyCellMessagePtr->diseaseIdx = disease_idx;
                modifyCellMessagePtr->diseaseCount = disease_count;
                modifyCellMessagePtr->addSubType = do_vertical_solid_displacement ? (byte)0 : (byte)1;
                Sim.SIM_HandleMessage((int)SimMessageHashes.ModifyCell, sizeof(ModifyCellMessage), (byte*)modifyCellMessagePtr);
            }


        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
        private struct ModifyCellMessage
        {
            public int cellIdx;
            public int callbackIdx;
            public float temperature;
            public float mass;
            public int diseaseCount;
            public byte elementIdx;
            public byte replaceType;
            public byte diseaseIdx;
            public byte addSubType;
        }
    }

}
