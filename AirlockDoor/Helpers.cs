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

        // Vera identita' della porta: confronta il PrefabTag, stabile anche sulle istanze in
        // gioco (il name ha il suffisso "(Clone)"). Matcha SOLO il full door.
        public static bool IsAirlockDoor(Door door)
        {
            return GetPrefabId(door) == AirlockDoorConfig.ID;
        }

        // Half door (AirlockHalfMechanizedDoor): estende PressureDoorConfig.
        public static bool IsAirlockHalfDoor(Door door)
        {
            return GetPrefabId(door) == AirlockHalfDoorConfig.ID;
        }

        // Una delle NOSTRE porte (full o half). La fisica airlock condivisa (SetSimState +
        // OnCleanUp) e' scoped qui, cosi' le due porte hanno lo stesso comportamento.
        // NB: Sim200ms resta scoped al solo full door (vedi DoorMod): la half ha la porta
        // logica (automazione) che vive proprio nel Sim200ms vanilla.
        public static bool IsOurDoor(Door door)
        {
            return IsAirlockDoor(door) || IsAirlockHalfDoor(door);
        }

        // Restituisce il PrefabTag.Name della porta (o il name del GameObject come
        // fallback se il tag non e' ancora pronto), oppure null.
        private static string GetPrefabId(Door door)
        {
            if (door == null || door.gameObject == null)
                return null;
            KPrefabID kpid = door.GetComponent<KPrefabID>();
            if (kpid != null && kpid.PrefabTag.IsValid)
                return kpid.PrefabTag.Name;
            return door.gameObject.name;
        }

    }
}
