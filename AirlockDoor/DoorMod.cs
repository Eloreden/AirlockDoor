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
            // Door eredita da Workable: overrideAnims e' l'animazione del DUPE che opera la porta
            // (NON l'aspetto dell'edificio, che arriva dal BuildingDef). In vanilla vale il campo
            // statico condiviso Door.OVERRIDE_ANIMS = [ Assets.GetAnim("anim_use_remote_kanim") ].
            //
            // Se quel campo statico viene inizializzato prima che gli anim siano caricati, resta
            // [null] per SEMPRE e ogni porta (vanilla incluse) logga "AddAnimOverrides tried to add
            // a null override" quando un dupe la opera. Qui ripariamo: se manca, rimettiamo l'anim
            // vera del dupe (a save/spawn gli anim sono gia' caricati). Vale per tutte le porte,
            // cosi' sistemiamo anche quelle vanilla, non solo le nostre.
            if (__instance.overrideAnims == null
                || __instance.overrideAnims.Length == 0
                || __instance.overrideAnims[0] == null)
            {
                KAnimFile useRemote = Assets.GetAnim("anim_use_remote_kanim");
                if (useRemote != null)
                    __instance.overrideAnims = new KAnimFile[] { useRemote };
                else
                    Debug.LogWarning("[AirlockDoor] anim_use_remote_kanim non trovata: impossibile riparare l'override del dupe.");
            }
        }
    }

    [HarmonyPatch(typeof(Door), "OnCleanUp")]
    internal class AirlockDoor_OnCleanUp
    {
        private static bool Prefix(Door __instance)
        {
            if (!Helpers.IsOurDoor(__instance))
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

            // Salta il Sim200ms vanilla SOLO per la full door. NON lo saltiamo per la half door:
            // la half ha la porta logica (LogicInputPorts) e l'automazione viene applicata proprio
            // qui (applyLogicChange -> ApplyRequestedControlState). Saltarlo le romperebbe
            // l'automazione. La full door non ha automazione (LogicInputPorts = null), quindi e' safe.
            // (Il melt check non scatta: entrambe Overheatable = false.)
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
            if (!Helpers.IsOurDoor(__instance))
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

                            // Solo se la porta e' BLOCCATA APERTA dal giocatore (ControlState.Opened)
                            // lasciamo passare tutto: azzeriamo il bit impermeabile (4) e NON ri-riempiamo
                            // la cella di solido, cosi' gas e liquidi attraversano.
                            // Nel passaggio TRANSITORIO di un dupe (Auto) manteniamo il comportamento
                            // airlock: la cella resta sigillata (vedi sotto).
                            if (__instance.CurrentState == Door.ControlState.Opened)
                            {
                                SimMessages.ClearCellProperties(cell, 4);
                                Pathfinding.Instance.AddDirtyNavGridCell(cell);
                                break;
                            }

                            // NB: NON replichiamo il ramo vanilla `ShouldBlockFallingSand`.
                            // ShouldBlockFallingSand e' true quando la porta e' RUOTATA (orizzontale):
                            // in vanilla quel ramo si limita a togliere il bit impermeabile (4) senza
                            // risigillare la cella, cosi' la pressure door orizzontale aperta lascia
                            // passare gas/liquidi (si comporta come una botola). Per la NOSTRA porta
                            // airlock vogliamo invece restare a tenuta anche in orizzontale: cadiamo
                            // sempre nel ramo di risigillatura (ReplaceAndDisplaceElement + bit 4).
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
