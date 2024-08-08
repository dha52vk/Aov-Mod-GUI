using Aov_Mod_GUI.CustomModWd;
using Aov_Mod_GUI.Models;
using AovClass;
using AovClass.Models;
using Microsoft.Win32;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

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
        bool FolderGenerated
        {
            get => _FolderGenerated; set
            {
                _FolderGenerated = value; MainWindow.SetCustomizingState(value);
                IconFieldRow.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                LabelFieldRow.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                SoundNewIds.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                UpdateFromOlderBtn.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                CustomBtnResetRow.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                FolderSavePath.SetReadOnly(value);
                CustomHeroId.SetReadOnly(value);
                CustomSkinName.SetReadOnly(value);
            }
        }
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

            FolderGenerated = false;

            CustomInfoBtn.Click += CustomInfoBtn_Click;
            CustomActionBtn.Click += CustomActionBtn_Click;
            CustomCommonActionsBtn.Click += CustomCommonActionsBtn_Click;
            UpdateFromOlderBtn.Click += UpdateFromOlderBtn_Click;
        }

        private void UpdateFromOlderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!FolderGenerated)
                return;
            OpenFolderDialog openFolderDialog = new() { Multiselect = false, Title = "Select folder that contains \"Ages\", \"Prefab_Characters\" " };
            if (openFolderDialog.ShowDialog() == false)
                return;
            string parentPath = openFolderDialog.FolderName;
            string infoPath = PathExtension.Combine(parentPath, $"Prefab_Characters\\Actor_{CustomHeroId.GetText()}_Infos.pkg.bytes");
            string actionPath = PathExtension.Combine(parentPath, $"Ages\\Prefab_Characters\\Prefab_Hero\\Actor_{CustomHeroId.GetText()}_Actions.pkg.bytes");
            string commonActionPath = PathExtension.Combine(parentPath, "Ages\\Prefab_Characters\\Prefab_Hero\\CommonActions.pkg.bytes");
            ProgressWindow progressWindow = new() { Owner = Window.GetWindow(this) };
            progressWindow.SetCancelable(false);
            progressWindow.SetProgressMaxium(3);
            progressWindow.Execute(() =>
            {
                #region UpdateInfo
                progressWindow.UpdateProgress(0, "Updating Info package");
                if (File.Exists(infoPath) && infoElement != null)
                {
                    try
                    {
                        InfosPackage tempInfoPkg = new(infoPath);
                        foreach (var pair in infoElement.Elements)
                        {
                            if (tempInfoPkg.Elements.ContainsKey(pair.Key))
                            {
                                UpdateTempInfo(pair.Value, tempInfoPkg.Elements[pair.Key]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Update Info Error: " + ex.Message);
                    }
                }
                #endregion
                #region UpdateAction
                progressWindow.UpdateProgress(1, "Updating Hero Action package");
                if (File.Exists(actionPath) && heroActionsPkg != null)
                {
                    try
                    {
                        ProjectPackage tempPackage = new(actionPath);
                        UpdateTempProject(heroActionsPkg, tempPackage);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Update Action Error: " + ex.Message);
                    }
                }
                #endregion
                #region Update Common action
                progressWindow.UpdateProgress(2, "Updating Common Action package");
                if (File.Exists(commonActionPath) && commonActionsPkg != null)
                {
                    try
                    {
                        ProjectPackage tempPackage = new(commonActionPath);
                        List<string> KeyUpdates = [
                            @"commonresource\Back.xml",
                            @"commonresource\Born.xml",
                            @"commonresource\Dance.xml",
                            @"commonresource\HasteE1.xml",
                            @"commonresource\HasteE1_leave.xml",
                        ];
                        UpdateTempProject(commonActionsPkg, tempPackage, (key) => KeyUpdates.FindIndex((k) => k.Equals(key, StringComparison.CurrentCultureIgnoreCase)) > -1);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Update Common Action Error: " + ex.Message);
                    }
                }
                #endregion
            });
            progressWindow.ShowDialog();
        }
        private void UpdateTempInfo(PackageElement info, PackageElement tempInfo)
        {
            if (info._Name == tempInfo._Name)
            {
                if (info._JtType == "JTArr" || info._Type.Contains("list", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (info.Children == null || tempInfo.Children == null)
                        return;
                    int maxLength = Math.Min(info.Children.Count, tempInfo.Children.Count);
                    for (int i = 0; i < maxLength; i++)
                    {
                        UpdateTempInfo(info.Children[i], tempInfo.Children[i]);
                    }
                    if (maxLength < tempInfo.Children.Count)
                    {
                        for (int i = maxLength; i < tempInfo.Children.Count; i++)
                        {
                            info.AddChild(tempInfo.Children[i]);
                        }
                    }
                }
                else
                {
                    foreach (var tempChild in tempInfo.Children ?? [])
                    {
                        PackageElement? child = info.Children?.Find((c) => c._Name == tempChild._Name);
                        if (child != null)
                        {
                            UpdateTempInfo(child, tempChild);
                        }
                        else
                        {
                            info.AddChild(tempChild);
                        }
                    }
                    if (info.Value != null && tempInfo.Value != null)
                    {
                        info.Value = [.. tempInfo.Value];
                    }
                }
            }
        }
        private void UpdateTempProject(ProjectPackage package, ProjectPackage tempPackage, Func<string, bool>? keyFilter = null)
        {
            foreach (var pair in tempPackage.Projects)
            {
                if ((keyFilter != null && !keyFilter(pair.Key)) || !package.Projects.ContainsKey(pair.Key))
                {
                    LogExtension.Log("Skipped " + pair.Key);
                    continue;
                }
                LogExtension.Log("Updating " + pair.Key);
                var tempActions = pair.Value.GetActionNodes();
                var project = package.Projects[pair.Key];
                var actions = project.GetActionNodes();
                if (tempActions == null || actions == null)
                {
                    continue;
                }
                int i, j;
                for (i = 0, j = 0; i < tempActions.Count && j < actions.Count; i++, j++)
                {
                    if (tempActions[i].Attributes == null)
                    {
                        continue;
                    }
                    var tempAction = tempActions[i];
                    int find = actions.FindIndex((track) => track.Attributes != null && track.GetAttribute("guid") == tempAction.GetAttribute("guid"));
                    if (find > j)
                    {
                        j = find;
                        Trace.WriteLine("Updated action index to " + j);
                    }
                    var action = actions[j];
                    LogExtension.Log($"Comparing {j} - {i}: " + action.GetAttribute("trackName") + " - " + tempAction.GetAttribute("trackName"));
                    if (action.GetAttribute("guid") != tempAction.GetAttribute("guid"))
                    {
                        project.InsertActionNode(j, tempAction);
                        var list = project.GetActionNodes();
                        if (list != null) actions = list;
                        continue;
                    }
                    UpdateTempAction(action, tempAction);
                }
                while (i < tempActions.Count)
                {
                    if (tempActions[i].Attributes != null)
                    {
                        LogExtension.Log("Added New Action Temp: " + tempActions[i].GetAttribute("trackName"));
                        project.AppendActionNode(tempActions[i]);
                    }
                    i++;
                }
                //foreach (XmlNode tempTrack in tempActions)
                //{
                //    if (tempTrack.Attributes == null)
                //    {
                //        continue;
                //    }
                //    XmlNode? track = actions.Find((node) => node.GetAttribute("guid") == tempTrack.GetAttribute("guid"));
                //    if (track != null)
                //    {
                //        UpdateTempAction(track, tempTrack);
                //    }
                //    else
                //    {
                //        project.AppendActionNode(tempTrack);
                //        Trace.WriteLine("Added action " + tempTrack.GetAttribute("trackName"));
                //        LogExtension.Log("Added action " + tempTrack.GetAttribute("trackName"));
                //    }
                //}
            }
        }
        private void UpdateTempAction(XmlNode node, XmlNode tempNode)
        {
            if (node.Name == "Track" && tempNode.Name == "Track")
            {
                if (node.GetAttribute("guid") == tempNode.GetAttribute("guid"))
                {
                    List<XmlNode> children = node.ChildNodes.Cast<XmlNode>().Where((node) => node.Name == "Condition").ToList();
                    foreach (XmlNode tempChild in tempNode.ChildNodes)
                    {
                        if (tempChild.Name == "Condition" && children.Find((cond) => cond.GetAttribute("guid") == tempChild.GetAttribute("guid")) == null)
                        {
                            XmlNode? condi = node.OwnerDocument?.ImportNode(tempChild, true);
                            if (condi != null)
                            {
                                node.InsertChild(0,condi);
                                LogExtension.Log("Added condition " + condi.GetAttribute("guid") + " to " + node.GetAttribute("trackName") + $"({node.GetAttribute("guid")})");
                            }
                        }
                    }
                    XmlNode? eventNode = node.GetChildrenByName("Event")?[0],
                        tempEventNode = tempNode.GetChildrenByName("Event")?[0];
                    if (eventNode != null && tempEventNode != null)
                        UpdateTempAction(eventNode, tempEventNode);
                }
            }
            else if (node.Name == "Event" && tempNode.Name == "Event")
            {
                foreach (XmlNode tempParam in tempNode.ChildNodes)
                {
                    XmlNode? param = node.ChildNodes.Cast<XmlNode>().ToList().Find((p) => p.GetAttribute("name") == tempParam.GetAttribute("name"));
                    if (param != null)
                    {
                        UpdateTempAction(param, tempParam);
                    }
                    else
                    {
                        XmlNode? clone = node.OwnerDocument?.ImportNode(tempParam, true);
                        if (clone != null)
                        {
                            Trace.WriteLine("Appended param " + clone.GetAttribute("name") + ": " + clone.GetAttribute("value"));
                            LogExtension.Log("Appended param " + clone.GetAttribute("name") + ": " + clone.GetAttribute("value"));
                            node.AppendChild(clone);
                        }
                    }
                }
            }
            else if (node.GetAttribute("name") == tempNode.GetAttribute("name"))
            {
                if (node.GetAttribute("value") != tempNode.GetAttribute("value"))
                {
                    Trace.WriteLine("Updated " + node.GetAttribute("name") + " from " + node.GetAttribute("value") + " to "
                        + tempNode.GetAttribute("value"));
                    LogExtension.Log("Updated " + node.GetAttribute("name") + " from " + node.GetAttribute("value") + " to "
                        + tempNode.GetAttribute("value"));
                }
                XmlNode? clone = node.OwnerDocument?.ImportNode(tempNode, true);
                if (clone != null)
                    node.ParentNode?.InsertBefore(clone, node);
                node.ParentNode?.RemoveChild(node);
            }
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
            if (FolderGenerated)
            {
                if (heroActionsPkg == null)
                {
                    MessageBox.Show("Hero Action Package bằng null!!", "Lỗi");
                    return;
                }
                CusProjectXml cusProject = new(heroActionsPkg);
                cusProject.Show();
            }
            else
            {
                CusProjectXml cusProject = new();
                try
                {
                    cusProject.Show();
                }
                catch
                {

                }
            }
        }

        private void CustomInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (FolderGenerated)
            {
                if (infoElement == null)
                {
                    MessageBox.Show("Info Element bằng null!!", "Lỗi");
                    return;
                }
                CusInfos cusInfos = new(infoElement);
                cusInfos.Show();
            }
            else
            {
                CusInfos cusInfos = new();
                try
                {
                    cusInfos.Show();
                }
                catch
                {

                }
            }
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
            OldSkins = [new(){Id=int.Parse(CustomHeroId.Text+"1"), Label = "Default", Name=""}];
            OldSkins.AddRange(heroes?.Find((hero) => hero.Id == int.Parse(CustomHeroId.GetText()))?.Skins ?? []);
            string InfosPath = infoPath,
                AssetRefPath = assetrefPath,
                HeroActionsPath = heroActionsPath;
            ProgressWindow progressWd = new() { Owner = Window.GetWindow(this) };
            progressWd.SetProgressMaxium(6 + soundPath.Length);
            progressWd.SetCancelable(false);
            progressWd.Execute(() =>
            {
                progressWd.UpdateProgress(0, "Loading icon...");
                if (File.Exists(iconPath)) iconWp = new(GetAovBytesFrom(iconPath));
                progressWd.UpdateProgress(1, "Loading label...");
                if (File.Exists(labelPath)) labelWp = new(GetAovBytesFrom(labelPath));
                progressWd.UpdateProgress(2, "Loading infos...");
                infoElement = new(InfosPath);
                progressWd.UpdateProgress(3, "Loading assetref...");
                if (File.Exists(AssetRefPath)) assetRefElement = PackageSerializer.Deserialize(GetAovBytesFrom(AssetRefPath));
                progressWd.UpdateProgress(4, "Loading hero actions...");
                heroActionsPkg = new(HeroActionsPath);
                progressWd.UpdateProgress(5, "Loading common actions...");
                commonActionsPkg = new(commonActionsPath);
                soundWps = [];
                for (int i = 0; i < soundPath.Length; i++)
                {
                    string soundT = Path.GetFileName(soundPath[i]);
                    progressWd.UpdateProgress(6 + i, $"Loading {soundT}...");
                    if (File.Exists(soundPath[i])) soundWps.Add(soundT, new(GetAovBytesFrom(soundPath[i])));
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
                    foreach (Skin oldSkin in OldSkins)
                    {
                        int oldSkinId = (int)oldSkin.Id;
                        int oldSoundId = int.Parse(oldSkinId.ToString()[..3]) * 100 + int.Parse(oldSkinId.ToString()[3..]) - 1;
                        if (targetSounds == null)
                        {
                            int targetId = int.Parse(soundId.ToString()[..3]) * 100 + int.Parse(soundId.ToString()[3..]) - 1;
                            soundWps[pair.Key].copySound(oldSoundId, targetId, i == 0);
                        }
                        else
                        {
                            soundWps[pair.Key].setSound(oldSoundId, targetSounds.soundElements);
                        }
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
            //var result = MessageBox
            //    .Show("Sau khi hoàn thành mod sẽ reset toàn bộ custom fields! Xác nhận không?"
            //    , "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            //if (result == MessageBoxResult.No)
            //{
            //    return;
            //}
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
                if (iconWp != null) File.WriteAllBytes(iconPath, AovTranslation.Compress(iconWp.GetBytes()));
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
            //ResetCustomFields();
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
    }
}
