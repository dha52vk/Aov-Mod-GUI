using Aov_Mod_GUI.CustomModWd;
using Aov_Mod_GUI.Models;
using AovClass;
using AovClass.Models;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Aov_Mod_GUI.MainWindowControls
{
    /// <summary>
    /// Interaction logic for CustomModPage.xaml
    /// </summary>
    public partial class CustomModPage : UserControl
    {
        IconWrapper? iconWp;
        LabelWrapper? labelWp;
        Dictionary<string, SoundWrapper>? soundWps;
        InfosPackage? infoElement;
        PackageElement? assetRefElement;
        ProjectPackage? heroActionsPkg;
        ProjectPackage? commonActionsPkg;
        ModSources? modSources { get => MainWindow.GetModSources(); }
        List<Hero>? heroes { get => MainWindow.GetHeroList(); }
        SkinLevelWrapper? skinLvWp { get => MainWindow.GetSkinLevelWp(); }
        List<Skin> OldSkins = [];
        List<string> SoundKeyAcceptMulti = ["BattleBank.bytes"];

        bool _FolderGenerated = false;
        bool FolderGenerated { get => _FolderGenerated; set { _FolderGenerated = value; MainWindow.SetCustomizingState(value); } }
        string ParentSavePath = "";
        string iconPath { get => Path.Combine(ParentSavePath, "Databin/Client/Actor/heroSkin.bytes"); }
        string labelPath { get => Path.Combine(ParentSavePath, "Databin/Client/Shop/HeroSkinShop.bytes"); }
        string[] soundPath
        {
            get => [
            Path.Combine(ParentSavePath, "Databin/Client/Sound/BattleBank.bytes"),
            Path.Combine(ParentSavePath, "Databin/Client/Sound/ChatSound.bytes"),
            Path.Combine(ParentSavePath, "Databin/Client/Sound/HeroSound.bytes"),
            Path.Combine(ParentSavePath, "Databin/Client/Sound/LobbyBank.bytes"),
            Path.Combine(ParentSavePath, "Databin/Client/Sound/LobbySound.bytes")
            ];
        }
        string infoPath { get => Path.Combine(ParentSavePath, $"Prefab_Characters\\Actor_{CustomHeroId.GetText()}_Infos.pkg.bytes"); }
        string assetrefPath { get => Path.Combine(ParentSavePath, $"AssetRefs\\Hero\\{CustomHeroId.GetText()}_AssetRef.bytes"); }
        string heroActionsPath { get => Path.Combine(ParentSavePath, $"Ages\\Prefab_Characters\\Prefab_Hero\\Actor_{CustomHeroId.GetText()}_Actions.pkg.bytes"); }
        string commonActionsPath { get => Path.Combine(ParentSavePath, "Ages\\Prefab_Characters\\Prefab_Hero\\CommonActions.pkg.bytes"); }
        readonly string[] modFiles = [
            "Ages\\Prefab_Characters\\Prefab_Hero\\Actor_{0}_Actions.pkg.bytes",
            "Ages\\Prefab_Characters\\Prefab_Hero\\CommonActions.pkg.bytes",
            "AssetRefs\\Hero\\{0}_AssetRef.bytes",
            "Databin/Client/Actor/heroSkin.bytes",
            "Databin/Client/Actor/organSkin.bytes",
            "Databin/Client/Character/ResCharacterComponent.bytes",
            "Databin/Client/Shop/HeroSkinShop.bytes",
            "Databin/Client/Skill/liteBulletCfg.bytes",
            "Databin/Client/Skill/skillmark.bytes",
            "Databin/Client/Sound/BattleBank.bytes",
            "Databin/Client/Sound/ChatSound.bytes",
            "Databin/Client/Sound/HeroSound.bytes",
            "Databin/Client/Sound/LobbyBank.bytes",
            "Databin/Client/Sound/LobbySound.bytes",
            "Prefab_Characters\\Actor_{0}_Infos.pkg.bytes"
        ];

        public CustomModPage()
        {
            InitializeComponent();

            //CustomIconBtn.Click += CustomIconBtn_Click;
            CustomInfoBtn.Click += CustomInfoBtn_Click;
            CustomActionBtn.Click += CustomActionBtn_Click;
            CustomCommonActionsBtn.Click += CustomCommonActionsBtn_Click;
            //CustomSoundBtn.Click += CustomSoundBtn_Click;
            //TestBtn.Click += TestBtn_Click;
            CustomModControl.Visibility = Visibility.Collapsed;
        }

        private void CustomCommonActionsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (commonActionsPkg == null)
            {
                MessageBox.Show("Common Actions Package bằng null!!", "Lỗi");
                return;
            }
            CusProjectXml cusProject = new(commonActionsPkg);
            cusProject.Show();
        }

        private void CustomActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (heroActionsPkg == null)
            {
                MessageBox.Show("Hero Action Package bằng null!!", "Lỗi");
                return;
            }
            CusProjectXml cusProject = new(heroActionsPkg);
            cusProject.Show();
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!FolderGenerated)
                return;

        }

        private void CustomSoundBtn_Click(object sender, RoutedEventArgs e)
        {
            if (soundWps == null)
            {
                return;
            }
            CusSounds cusSounds = new CusSounds(soundWps);
            cusSounds.Show();
        }

        private void CustomInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (infoElement == null)
            {
                MessageBox.Show("Info Element bằng null!!", "Lỗi");
                return;
            }
            CusInfos cusInfos = new(infoElement);
            cusInfos.Show();
        }

        private void CustomIconBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!FolderGenerated)
            {
                MessageBox.Show("Chưa khởi tạo folder mod!");
                return;
            }
            CusIcon cusIconWd = new(CustomSkinName.GetText(), iconWp, labelWp) { Owner = Window.GetWindow(this) };
            int hero = int.Parse(CustomHeroId.GetText());
            cusIconWd.SetOldSkins(GetSkinLevelA(hero));
            cusIconWd.ShowDialog();
        }

        private void GenerateModFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CustomHeroId.GetText(), out int heroId) || !MainWindow.CheckHeroIdExists(heroId))
            {
                MessageBox.Show("HeroId không hợp lệ (không thuộc danh sách)!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(CustomSkinName.GetText()))
            {
                MessageBox.Show("Vui lòng nhập tên skin!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(FolderSavePath.GetText()) || !Directory.Exists(FolderSavePath.GetText()))
            {
                MessageBox.Show("Đường dẫn lỗi!!!");
                return;
            }
            if (FolderGenerated)
            {
                MessageBox.Show("Đã khởi tạo folder!!!");
                return;
            }
            var result = MessageBox
                .Show("Sau khi khởi tạo thư mục sẽ không thể thay đổi id và tên skin! Xác nhận không?"
                , "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            string generateParentDir = Path.Combine(FolderSavePath.GetText(), CustomSkinName.GetText(),
                    "Resources", modSources?.AovVersion ?? "");
            if (Directory.Exists(generateParentDir))
            {
                MessageBoxResult result2 = MessageBox.Show("Thư mục " + generateParentDir + " đã tồn tại!\nCó muốn xóa thư mục cũ không? Sau khi xóa sẽ không thể hoàn tác",
                   "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (result2 == MessageBoxResult.Yes)
                {
                    Directory.Delete(generateParentDir, true);
                }
                else
                {
                    return;
                }
            }
            FolderGenerated = true;
            CustomHeroId.SetReadOnly(true);
            CustomSkinName.SetReadOnly(true);
            FolderSavePath.SetReadOnly(true);
            MainWindow.SetCustomizingState(true);
            FolderSavePath.Text = generateParentDir;
            foreach (string _file in modFiles)
            {
                string file = string.Format(_file, CustomHeroId.GetText());
                string path = Path.Combine(generateParentDir, file);
                string parentDir = Directory.GetParent(path)?.FullName ?? "";
                string resourcesPath = modSources?.ResourcesPath ?? "";

                if (!Directory.Exists(parentDir))
                    Directory.CreateDirectory(parentDir);
                File.Copy(Path.Combine(resourcesPath, file), path);
            }
            ParentSavePath = generateParentDir;
            ReloadMod();
        }

        private void EditModFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FolderGenerated)
            {
                MessageBox.Show("Đã khởi tạo folder!!!");
                return;
            }
            ParentSavePath = "";
            string path = FolderSavePath.GetText();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("Đường dẫn lỗi!!!");
                return;
            }
            List<string> children = new(Directory.GetDirectories(path));
            int resourcesIndex = children.FindIndex((c) => c.EndsWith("Resources"));
            if (resourcesIndex != -1)
            {
                string parent = Path.Combine(path, "Resources\\1.55.1\\Prefab_Characters");
                string name = Directory.GetFiles(parent).Where((s) => s.EndsWith("_Infos.pkg.bytes")).First();
                CustomHeroId.Text = Path.GetFileName(name)[6..9];
                CustomSkinName.Text = Path.GetFileName(path);
                ParentSavePath = Path.Combine(children[resourcesIndex], modSources?.AovVersion ?? "");
                FolderGenerated = true;
                CustomHeroId.SetReadOnly(true);
                CustomSkinName.SetReadOnly(true);
                FolderSavePath.SetReadOnly(true);
                MainWindow.SetCustomizingState(true);
                ReloadMod();
                return;
            }
            List<int> checkIndexes = [children.FindIndex((c) => c.EndsWith("Ages")),
                                children.FindIndex((c) => c.EndsWith("AssetRefs")),
                                children.FindIndex((c) => c.EndsWith("Databin")),
                                children.FindIndex((c) => c.EndsWith("Prefab_Characters")) ];

            bool check = checkIndexes.FindIndex((i) => i == -1) == -1;
            if (check)
            {
                string parent = Path.Combine(path, "Prefab_Characters");
                string name = Directory.GetFiles(parent).Where((s) => s.EndsWith("_Infos.pkg.bytes")).First();
                CustomHeroId.Text = Path.GetFileName(name)[6..9];
                ParentSavePath = path;
                FolderGenerated = true;
                CustomHeroId.SetReadOnly(true);
                CustomSkinName.SetReadOnly(true);
                FolderSavePath.SetReadOnly(true);
                MainWindow.SetCustomizingState(true);
                ReloadMod();
            }
            else
            {
                MessageBox.Show("Lỗi, không phát hiện folder mod!", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
        }

        public void ReloadMod()
        {
            if (skinLvWp != null)
            {
                List<string> sources = [];
                sources.AddRange(skinLvWp.SkinLabelLevels.Select((label) => label.label).Where((l) => l != null).Cast<string>().ToList());
                if (modSources?.SpecialLabelElements != null)
                {
                    foreach (var pair in modSources.SpecialLabelElements)
                    {
                        sources.Add(pair.Key.ToString());
                    }
                }
                LabelCombobox.ItemsSource = sources;
            }
            CustomModControl.Visibility = Visibility.Visible;
            OldSkins = (GetSkinLevelA(int.Parse(CustomHeroId.GetText())) ?? []);
            string InfosPath = infoPath,
                AssetRefPath = assetrefPath,
                HeroActionsPath = heroActionsPath;
            ProgressWindow progressWd = new() { Owner = Window.GetWindow(this) };
            progressWd.SetProgressMaxium(6 + soundPath.Length);
            progressWd.SetCancelable(false);
            progressWd.Execute(() =>
            {
                progressWd.UpdateProgress(0, "Loading icon...");
                iconWp = new(GetAovBytesFrom(iconPath));
                progressWd.UpdateProgress(1, "Loading label...");
                labelWp = new(GetAovBytesFrom(labelPath));
                progressWd.UpdateProgress(2, "Loading infos...");
                infoElement = new(InfosPath);
                progressWd.UpdateProgress(3, "Loading assetref...");
                assetRefElement = PackageSerializer.Deserialize(GetAovBytesFrom(AssetRefPath));
                progressWd.UpdateProgress(4, "Loading hero actions...");
                heroActionsPkg = new(HeroActionsPath);
                progressWd.UpdateProgress(5, "Loading common actions...");
                commonActionsPkg = new(commonActionsPath);
                soundWps = [];
                for (int i = 0; i < soundPath.Length; i++)
                {
                    string soundT = Path.GetFileName(soundPath[i]);
                    progressWd.UpdateProgress(6 + i, $"Loading {soundT}...");
                    soundWps.Add(soundT, new(GetAovBytesFrom(soundPath[i])));
                }
            });
            progressWd.ShowDialog();
        }

        public void ResetCustomFields()
        {
            iconWp = null;
            labelWp = null;
            infoElement = null;
            assetRefElement = null;
            soundWps = [];
            heroActionsPkg = null;
            commonActionsPkg = null;
            FolderGenerated = false;
            CustomHeroId.Text = "";
            CustomSkinName.Text = "";
            FolderSavePath.Text = "";
            IconNewId.Text = "";
            SoundNewIds.Text = "";
            LabelCombobox.SelectedIndex = -1;
            CustomModControl.Visibility = Visibility.Collapsed;
        }

        private byte[] GetAovBytesFrom(string path)
        {
            byte[] outputBytes = File.ReadAllBytes(path);
            return AovTranslation.Decompress(outputBytes) ?? outputBytes;
        }

        private void CustomIcon(int iconId, bool swapIcon)
        {
            if (iconWp == null)
            {
                MessageBox.Show("IconWrapper bằng null!");
                return;
            }
            // mod icon
            Skin? NewSkin = heroes?.SelectMany((h) => h.Skins ?? []).ToList().Find((s) => s.Id == iconId);
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
                foreach (Skin skin in OldSkins)
                {
                    int baseId = int.Parse((OldSkins[0].Id.ToString() ?? "")[..3]) * 100
                            + int.Parse((skin.Id.ToString() ?? "")[3..]) - 1;
                    if (iconBytes == null)
                    {
                        iconWp.CopyIcon(baseId, targetId, swapIcon);
                    }
                    else
                    {
                        iconWp.SetIcon(baseId, iconBytes);
                        iconWp.SetIcon(targetId, iconBytes);
                    }
                }
            }
        }

        private void CustomLabel(string label)
        {
            if (labelWp == null)
            {
                MessageBox.Show("LabelWrapper bằng null!");
                return;
            }
            // mod label
            {
                if (string.IsNullOrEmpty(label))
                {
                    return;
                }
                byte[]? iconBytes = null;
                if (int.TryParse(label, out int skinId))
                {
                    iconBytes = modSources?.SpecialLabelElements[skinId];
                }
                foreach (Skin skin in OldSkins)
                {
                    int baseId = int.Parse((skin.Id.ToString() ?? "")[..3]) * 100
                            + int.Parse((skin.Id.ToString() ?? "")[3..]) - 1;
                    if (iconBytes == null)
                    {
                        int result = 2;
                        while (result == 2)
                        {
                            foreach (Hero hero in heroes ?? [])
                            {
                                if (hero.Skins == null)
                                    continue;
                                foreach (Skin skin2 in hero.Skins)
                                {
                                    if (skin2.Label == label)
                                    {
                                        int targetId = int.Parse((skin2.Id.ToString() ?? "")[..3]) * 100
                                                + int.Parse((skin2.Id.ToString() ?? "")[3..]) - 1;
                                        result = labelWp.CopyLabel(baseId, targetId);
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
                        labelWp.SetLabel(baseId, iconBytes);
                    }
                }
            }
        }

        private void CustomSound(int heroId, int[] soundIds)
        {
            if (soundWps == null)
                return;
            foreach (var pair in soundWps)
            {
                for (int i = 0; i < soundIds.Length; i++)
                {
                    int soundId = soundIds[i];
                    SoundWrapper? targetSounds = null;
                    if (modSources?.SpecialSoundElements.ContainsKey(soundId) ?? false)
                    {
                        targetSounds = new(modSources?.SpecialSoundElements[soundId][pair.Key.ToLower()] ?? []);
                    }
                    if (targetSounds == null)
                    {
                        int targetId = int.Parse(soundId.ToString()[..3]) * 100 + int.Parse(soundId.ToString()[3..]) - 1;
                        soundWps[pair.Key].copySound(heroId * 100, targetId, false);
                    }
                    else
                    {
                        soundWps[pair.Key].setSound(heroId * 100, targetSounds.soundElements);
                    }
                    if (SoundKeyAcceptMulti.FindIndex((key) => key.Equals(pair.Key, StringComparison.CurrentCultureIgnoreCase)) == -1)
                    {
                        break;
                    }
                }
            }
        }

        private void CustomCompletedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!FolderGenerated)
                return;
            int iconId, c = 0, heroId = int.Parse(CustomHeroId.Text);
            bool checkIcon = int.TryParse(IconNewId.Text, out iconId),
                checkLabel = LabelCombobox.SelectedIndex > -1,
                swapIcon = SwapIndexCheckBox.IsChecked ?? true;
            List<int> soundIds = [];
            string[] soundCheckIds = SoundNewIds.Text.Split(';');
            bool checkSound = true;
            while (checkSound && c < soundCheckIds.Length)
            {
                checkSound = checkSound & int.TryParse(soundCheckIds[c], out int soundId);
                if (checkSound)
                {
                    soundIds.Add(soundId);
                }
                c++;
            }
            var result = MessageBox
                .Show("Sau khi hoàn thành mod sẽ reset toàn bộ custom fields! Xác nhận không?"
                , "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No)
            {
                return;
            }
            string InfosPath = infoPath,
                AssetRefPath = assetrefPath,
                HeroActionsPath = heroActionsPath,
                label = LabelCombobox.SelectedValue?.ToString() ?? "";
            MainWindow.SetCustomizingState(false);
            ProgressWindow progressWd = new()
            {
                Owner = Window.GetWindow(this)
            };
            progressWd.SetCancelable(false);
            progressWd.SetProgressMaxium(6 + soundPath.Length);
            progressWd.Execute(() =>
            {
                progressWd.UpdateProgress(0, "Saving icon&label changes...");
                if (checkIcon) CustomIcon(iconId, swapIcon);
                if (checkLabel) CustomLabel(label);
                progressWd.UpdateProgress(0, "Saving icon...");
                if (iconWp != null) File.WriteAllBytes(iconPath, AovTranslation.Compress(iconWp.getBytes()));
                progressWd.UpdateProgress(1, "Saving label...");
                if (labelWp != null) File.WriteAllBytes(labelPath, AovTranslation.Compress(labelWp.GetBytes()));
                progressWd.UpdateProgress(2, "Saving infos...");
                if (infoElement != null) infoElement.SaveTo(InfosPath);
                progressWd.UpdateProgress(3, "Saving assetRef...");
                if (assetRefElement != null) File.WriteAllBytes(AssetRefPath, AovTranslation.Compress(PackageSerializer.Serialize(assetRefElement)));
                progressWd.UpdateProgress(4, "Saving hero actions...");
                if (heroActionsPkg != null) heroActionsPkg.SaveTo(HeroActionsPath);
                progressWd.UpdateProgress(5, "Saving common actions...");
                if (commonActionsPkg != null) commonActionsPkg.SaveTo(commonActionsPath);
                progressWd.UpdateProgress(5, "Saving sound changes...");
                if (checkSound) CustomSound(heroId, [.. soundIds]);
                if (soundWps != null)
                {
                    for (int i = 0; i < soundPath.Length; i++)
                    {
                        string soundT = Path.GetFileName(soundPath[i]);
                        if (soundWps[soundT] != null)
                        {
                            progressWd.UpdateProgress(6 + i, $"Saving {soundT}...");
                            File.WriteAllBytes(soundPath[i], AovTranslation.Compress(soundWps[soundT].getBytes()));
                        }
                    }
                }
            });
            progressWd.ShowDialog();
            ResetCustomFields();
            CustomHeroId.SetReadOnly(false);
            CustomSkinName.SetReadOnly(false);
            FolderSavePath.SetReadOnly(false);
        }

        private void CustomCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!FolderGenerated)
                return;
            var result = MessageBox
                .Show("Đang chỉnh sửa dở, sẽ reset toàn bộ custom fields! Xác nhận không?"
                , "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No)
            {
                return;
            }
            ResetCustomFields();
        }

        public List<Skin>? GetSkinLevelA(int heroId)
        {
            Hero? hero = heroes?.Find((hero) => hero.Id == heroId);
            if (hero == null || skinLvWp == null)
                return null;
            List<Skin> skin = [new Skin(heroId * 10 + 1, "Default")];
            skin.AddRange(hero?.Skins?.Where((skin) => skinLvWp.GetSkinLevel(skin) <= (int)DefaultLevel.A) ?? []);
            return skin;
        }
    }
}
