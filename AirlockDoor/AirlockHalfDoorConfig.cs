using System;
using TUNING;
using UnityEngine;

namespace AirlockDoor
{
    // Half door: a differenza del full door (AirlockDoorConfig, IBuildingConfig "nudo"),
    // questa estende PressureDoorConfig e quindi eredita gia' la fisica airtight della
    // porta a pressione vanilla. Per questo NON ha bisogno delle patch fisiche custom
    // (OnCleanUp/Sim200ms/SetSimState) di DoorMod: gli serve solo l'override anim.
    public class AirlockHalfDoorConfig : PressureDoorConfig
    {
        public const string ID = "AirlockHalfMechanizedDoor";
        public const string DisplayName = "Airlock Half Door";
        public const string Description = "A half door that isolates gas and liquids between two rooms.";
        public static string Effect = "This door prevents the passage of gas and liquids between two separate areas";

        // Vedi AirlockDoorConfig.AddStrings: senza queste ONI mostra MISSING.STRINGS.*
        public static void AddStrings()
        {
            string upper = ID.ToUpper();
            Strings.Add("STRINGS.BUILDINGS.PREFABS." + upper + ".NAME", DisplayName);
            Strings.Add("STRINGS.BUILDINGS.PREFABS." + upper + ".DESC", Description);
            Strings.Add("STRINGS.BUILDINGS.PREFABS." + upper + ".EFFECT", Effect);
        }

        public override BuildingDef CreateBuildingDef()
        {//door_external_kanim airlock_mechanized_door_kanim
            Console.WriteLine($"[ AIRLOCK HALF DOOR Main ] CreateBuildingDef");
            int width = 1;
            int height = 1;
            string anim = "half_airlock_mechanized_door_kanim";
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
            SoundEventVolumeCache.instance.AddVolume("airlock_mechanized_door_kanim", "Open_DoorPressure", NOISE_POLLUTION.NOISY.TIER2);
            SoundEventVolumeCache.instance.AddVolume("airlock_mechanized_door_kanim", "Close_DoorPressure", NOISE_POLLUTION.NOISY.TIER2);
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