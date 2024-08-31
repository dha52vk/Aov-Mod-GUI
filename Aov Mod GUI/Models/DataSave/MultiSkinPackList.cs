using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aov_Mod_GUI.Models.DataSave
{
    internal class MultiSkinPackList(List<MultiSkinPack> PackList)
    {
        public List<MultiSkinPack> PackList { get; set; } = PackList;
    }

    internal class MultiSkinPack(string PackName, List<MultiSkin> MultiSkins)
    {
        public string PackName = PackName;
        public List<MultiSkin> MultiSkins = MultiSkins;

        public override string ToString()
        {
            return PackName + ": " + String.Join("; ", MultiSkins);
        }
    }

    internal class MultiSkin(int heroId, List<KeyValuePair<int,int>> SkinChanges)
    {
        public int heroId = heroId;
        public List<KeyValuePair<int, int>> SkinChanges = SkinChanges;

        public override string ToString()
        {
            return String.Join(", ", SkinChanges.Select(pair => pair.Key + "-" + pair.Value));
        }
    }
}
