using Harmony;

namespace AirlockDoor
{
    public class AirlockDoorPatch
    {
        public static bool didStartupBuilding;
        public static bool didStartupDb;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            //Vanilla
            //public static void Prefix()
            //{
            //    if (!didStartupBuilding)
            //    {

            //        OniUtils.AddBuildingStrings(AirlockDoorConfig.ID, AirlockDoorConfig.DisplayName,
            //            AirlockDoorConfig.Description, AirlockDoorConfig.Effect);
            //        OniUtils.AddBuildingToBuildMenu("Base", AirlockDoorConfig.ID);
            //        didStartupBuilding = true;
            //    }
            //}

            //SpaceOut
            public static void Prefix()
            {
                OniUtils.AddBuildingStrings(AirlockDoorConfig.ID, AirlockDoorConfig.DisplayName,
                    AirlockDoorConfig.Description, AirlockDoorConfig.Effect);
                OniUtils.AddBuildingToBuildMenu("Base", AirlockDoorConfig.ID);
                didStartupBuilding = true;
            }
        }

        ////vanilla
        //[HarmonyPatch(typeof(Db), "Initialize")]
        //public static class Db_Initialize_Patch
        //{
        //    public static void Prefix()
        //    {
        //        if (!didStartupDb)
        //        {
        //            OniUtils.AddBuildingToTech("DirectedAirStreams", AirlockDoorConfig.ID);

        //            didStartupDb = true;
        //        }
        //    }
        //}

        //Space Out
        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Db.Get().Techs.Get("DirectedAirStreams").unlockedItemIDs.Add(AirlockDoorConfig.ID);
            }
        }

    }
}
