using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aov_Mod_GUI.Models.DataSave
{
    internal class SkinPackList(List<SkinPack> skinPacks)
    {
        public List<SkinPack> SkinPacks = skinPacks;
    }

    internal class SkinPack (string packName, List<int> skinIds)
    {
        public string PackName = packName;
        public List<int> SkinIds = skinIds;

        public override string ToString()
        {
            return $"{PackName}: {string.Join(", ", SkinIds)}";
        }
    }
}
