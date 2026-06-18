using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirlockDoor
{
    public class Helpers
    {
        public static void doorBuildMenu(string door, string menu, string subcategory, string pred)
        {
            // API nuova (sostituisce la manipolazione diretta di BUILDINGS.PLANORDER, ora deprecata):
            // inserisce 'door' nella categoria 'menu', sottocategoria 'subcategory',
            // subito dopo l'edificio 'pred'.
            ModUtil.AddBuildingToPlanScreen((HashedString)menu, door, subcategory, pred, ModUtil.BuildingOrdering.After);
        }

        public static void doorTechTree(string door, string group)
        {
            if (group == "none")
                return;
            Db.Get().Techs.TryGet(group)?.unlockedItemIDs.Add(door);
        }

        // Vera identita' della porta: confronta il PrefabTag (= AirlockDoorConfig.ID), stabile
        // anche sulle istanze in gioco (il name ha il suffisso "(Clone)").
        public static bool IsAirlockDoor(Door door)
        {
            if (door == null || door.gameObject == null)
                return false;
            KPrefabID kpid = door.GetComponent<KPrefabID>();
            if (kpid != null && kpid.PrefabTag.IsValid)
                return kpid.PrefabTag.Name == AirlockDoorConfig.ID;
            // fallback nel caso il PrefabTag non sia ancora pronto
            return door.gameObject.name.Contains(AirlockDoorConfig.ID);
        }

    }
}
