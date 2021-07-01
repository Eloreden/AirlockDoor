using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirlockDoor
{
    public class Helpers
    {
        public static void doorBuildMenu(string door, string menu, string pred)
        {
            int index = TUNING.BUILDINGS.PLANORDER.FindIndex((Predicate<PlanScreen.PlanInfo>)(x => x.category == (HashedString)menu));
            if (index < 0)
                return;
            IList<string> data = (IList<string>)TUNING.BUILDINGS.PLANORDER[index].data;
            int num = -1;
            foreach (string str in (IEnumerable<string>)data)
            {
                if (str.Equals(pred))
                    num = data.IndexOf(str);
            }
            if (num == -1)
                return;
            data.Insert(num + 1, door);
        }

        public static void doorTechTree(string door, string group)
        {
            if (group == "none")
                return;
            Db.Get().Techs.TryGet(group)?.unlockedItemIDs.Add(door);
        }

    }
}
