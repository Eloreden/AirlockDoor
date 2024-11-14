using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;

namespace AirlockDoor
{
    public class AirtighHalfDoorConfig : PressureDoorConfig
    {
        public const string ID = "AirthigHalfMechanizedDoor";
        public const string DisplayName = "Airtight half Door";
        public const string Description = "An half door isolate gas and liquids between tow room";
        public static string Effect = "This door prevents the passage of gas and liquids between two separate areas";

        public override BuildingDef CreateBuildingDef()
        {//door_external_kanim airlock_mechanized_door_kanim
            Console.WriteLine($"[ AIRLOCK HALF DOOR Main ] CreateBuildingDef");
            int width = 1;
            int height = 1;
            string anim = "airlock_mechanized_door_kanim";
            int hitpoint = 30;
            float construction_time = 30f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] all_METALS = MATERIALS.ALL_METALS;
            EffectorValues none = NOISE_POLLUTION.NONE;
            EffectorValues tieR1 = BUILDINGS.DECOR.PENALTY.TIER1;
            EffectorValues noise = none;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoint, construction_time, tier, all_METALS, 1600f, BuildLocationRule.Tile, tieR1, noise, 1f);
            buildingDef.Overheatable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 60f;
            buildingDef.Floodable = false;
            buildingDef.Entombable = false;
            buildingDef.IsFoundation = true;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.TileLayer = ObjectLayer.FoundationTile;
            buildingDef.AudioCategory = "Metal";
            buildingDef.PermittedRotations = PermittedRotations.R90;
            buildingDef.SceneLayer = Grid.SceneLayer.TileMain;
            buildingDef.ForegroundLayer = Grid.SceneLayer.InteriorWall;
            buildingDef.LogicInputPorts = DoorConfig.CreateSingleInputPortList(new CellOffset(0, 0));
            SoundEventVolumeCache.instance.AddVolume("half_airlock_mechanized_door", "Open_DoorPressure", NOISE_POLLUTION.NOISY.TIER2);
            SoundEventVolumeCache.instance.AddVolume("half_airlock_mechanized_door", "Close_DoorPressure", NOISE_POLLUTION.NOISY.TIER2);
            return buildingDef;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            base.DoPostConfigureComplete(go);
            go.AddComponent<KAminControllerResize>().height = 0.5f; 
        }
        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            base.DoPostConfigurePreview(def, go);
            go.AddComponent<KAminControllerResize>().height = 0.5f;
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            base.DoPostConfigureUnderConstruction(go);
            go.AddComponent<KAminControllerResize>().height = 0.5f;
        }
    }

    internal class KAminControllerResize : KMonoBehaviour
    {
        public float width = 1f;
        public float height = 1f;

        [MyCmpGet]
        private KBatchedAnimController controller;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (controller != null)
            {
                if (this.width != 1f)
                {
                    controller.animWidth = this.width;
                }
                if (this.height != 1f)
                {
                    controller.animHeight = this.height;
                }
            }
        }
    }
}
