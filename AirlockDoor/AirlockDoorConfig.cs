using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;

namespace AirlockDoor
{
    public class AirlockDoorConfig : IBuildingConfig
    {
        public const string ID = "AirlockMechanizedDoor";
        public const string DisplayName = "Airlock DoorDebug";
        public const string Description = "A door isolate gas and liquids between tow room";
        public static string Effect = "This door prevents the passage of gas and liquids between two separate areas";

        public AirlockDoorConfig()
        {
        }

        

        public override BuildingDef CreateBuildingDef()
        {//door_external_kanim airlock_mechanized_door_kanim
            Console.WriteLine($"[ AIRLOCK DOOR Main ] CreateBuildingDef");
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] all_METALS = MATERIALS.ALL_METALS;
            EffectorValues none = NOISE_POLLUTION.NONE;
            EffectorValues tieR1 = BUILDINGS.DECOR.PENALTY.TIER1;
            EffectorValues noise = none;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "airlock_mechanized_door_kanim", 30, 60f, tier, all_METALS, 1600f, BuildLocationRule.Tile, tieR1, noise, 1f);
            buildingDef.Overheatable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 120f;
            buildingDef.Floodable = false;
            buildingDef.Entombable = false;
            buildingDef.IsFoundation = true;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.TileLayer = ObjectLayer.FoundationTile;
            buildingDef.AudioCategory = "Metal";
            buildingDef.PermittedRotations = PermittedRotations.R90;
            buildingDef.SceneLayer = Grid.SceneLayer.TileMain;
            buildingDef.ForegroundLayer = Grid.SceneLayer.InteriorWall;
            buildingDef.LogicInputPorts = null;
            SoundEventVolumeCache.instance.AddVolume("door_external_kanim", "Open_DoorPressure", NOISE_POLLUTION.NOISY.TIER2);
            SoundEventVolumeCache.instance.AddVolume("door_external_kanim", "Close_DoorPressure", NOISE_POLLUTION.NOISY.TIER2);
            return buildingDef;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Door door = go.AddOrGet<Door>();
            door.hasComplexUserControls = true;
            door.unpoweredAnimSpeed = 0.65f;
            door.poweredAnimSpeed = 5f;
            door.doorClosingSoundEventName = "MechanizedAirlock_closing";
            door.doorOpeningSoundEventName = "MechanizedAirlock_opening";
            go.AddOrGet<ZoneTile>();
            go.AddOrGet<AccessControl>();
            go.AddOrGet<KBoxCollider2D>();
            Prioritizable.AddRef(go);
            go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.Door;
            go.AddOrGet<Workable>().workTime = 5f;
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object)go.GetComponent<BuildingEnabledButton>());
            go.GetComponent<AccessControl>().controlEnabled = true;
            go.GetComponent<KBatchedAnimController>().initialAnim = "closed";
        }
    }
}
