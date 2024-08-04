using AovClass;
using AovClass.Models;
using System.Windows;

namespace Aov_Mod_GUI.CustomModWd
{
    /// <summary>
    /// Interaction logic for CusIcon.xaml
    /// </summary>
    public partial class CusIcon : Window
    {
        public IconWrapper? iconWrapper;
        public LabelWrapper? labelWrapper;
        List<Hero>? Heroes { get => MainWindow.GetHeroList(); }
        ModSources? modSources { get => MainWindow.GetModSources(); }
        SkinLevelWrapper? skinLvWp { get => MainWindow.GetSkinLevelWp(); }
        List<Skin> oldSkins = [];

        public CusIcon(string skinLabel, IconWrapper? iconWrapper, LabelWrapper? labelWrapper)
        {
            InitializeComponent();
            this.iconWrapper = iconWrapper;
            this.labelWrapper = labelWrapper;
            CusWdTitle.Content = "Custom Mod Icon: " + skinLabel;
        }

        public void SetOldSkins(List<Skin>? skins)
        {
            if (skins == null)
                return;
            int?[] ids = skins.Select((skin) => skin.Id).Where((id) => id != null).ToArray() ?? [];
            string baseIds = string.Join(", ", ids);
            IconBaseId.Text = baseIds;
            LabelBaseId.Text = baseIds;
            IconBaseId.ToolTip = baseIds;
            LabelBaseId.ToolTip = baseIds;
            oldSkins = skins;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int[] checkId = new int[2];
            bool check = int.TryParse(IconNewId.Text, out checkId[0])
                    && int.TryParse(LabelNewId.Text, out checkId[1]);
            if (!check || checkId.ToList().FindIndex((id) => !MainWindow.CheckSkinIdExists(id)) != -1)
            {
                MessageBox.Show("Có id không hợp lệ!");
                return;
            }
            if (iconWrapper == null || labelWrapper == null)
            {
                MessageBox.Show("IconWrapper hoặc LabelWrapper bằng null!");
                return;
            }

            // mod icon
            Skin? NewSkin = Heroes?.SelectMany((h) => h.Skins ?? []).ToList().Find((s) => s.Id == checkId[0]);
            if (NewSkin != null)
            {
                string id = NewSkin.IsComponentSkin
                        ? NewSkin.Id / 100 + ""
                        : NewSkin.Id + "";
                int heroId = int.Parse(id[..3]);
                int targetId = heroId * 100 + int.Parse(id[3..]) - 1;
                byte[]? iconBytes = null;
                if (modSources?.SpecialIconElements.ContainsKey(int.Parse(id)) ?? false)
                {
                    iconBytes = modSources.SpecialIconElements[int.Parse(id)];
                }
                foreach (Skin skin in oldSkins)
                {
                    int baseId = int.Parse((oldSkins[0].Id.ToString()??"")[..3]) * 100
                            + int.Parse((skin.Id.ToString()??"")[3..]) - 1;
                    if (iconBytes == null)
                    {
                        iconWrapper.CopyIcon(baseId, targetId, SwapIndexCheckBox.IsChecked??true);
                    }
                    else
                    {
                        iconWrapper.SetIcon(baseId, iconBytes);
                        iconWrapper.SetIcon(targetId, iconBytes);
                    }
                }
            }
            // mod label
            Skin? LabelNewSkin = Heroes?.SelectMany((h) => h.Skins ?? []).ToList().Find((s) => s.Id == checkId[1]);
            if (LabelNewSkin != null)
            {
                string id = LabelNewSkin.IsComponentSkin
                        ? LabelNewSkin.Id / 100 + ""
                        : LabelNewSkin.Id + "";
                int heroId = int.Parse(id[..3]);
                int targetId = heroId * 100 + int.Parse(id[3..]) - 1;
                byte[]? iconBytes = null;
                if (modSources?.SpecialLabelElements.ContainsKey(int.Parse(id)) ?? false)
                {
                    iconBytes = modSources.SpecialLabelElements[int.Parse(id)];
                }
                foreach (Skin skin in oldSkins)
                {
                    int baseId = int.Parse((oldSkins[0].Id.ToString() ?? "")[..3]) * 100
                            + int.Parse((skin.Id.ToString() ?? "")[3..]) - 1;
                    if (iconBytes == null)
                    {
                        int result = labelWrapper.CopyLabel(baseId, targetId);
                        while (result == 2)
                        {
                            foreach (Hero hero in Heroes ?? [])
                            {
                                if (hero.Skins == null)
                                    continue;
                                foreach (Skin skin2 in hero.Skins)
                                {
                                    if (skin2.Label == LabelNewSkin.Label)
                                    {
                                        targetId = int.Parse((skin2.Id.ToString() ?? "")[..3]) * 100
                                                + int.Parse((skin2.Id.ToString() ?? "")[3..]) - 1;
                                        result = labelWrapper.CopyLabel(baseId, targetId);
                                        if (result != 2)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (result != 2)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        labelWrapper.SetLabel(baseId, iconBytes);
                        labelWrapper.SetLabel(targetId, iconBytes);
                    }
                }
            }
            Close();
        }
    }
}
