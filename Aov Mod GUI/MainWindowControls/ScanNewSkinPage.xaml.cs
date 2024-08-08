using Aov_Mod_GUI.Models;
using Aov_Mod_GUI.Models.DataSave;
using AovClass;
using AovClass.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Aov_Mod_GUI.MainWindowControls
{
    /// <summary>
    /// Interaction logic for ScanNewSkinPage.xaml
    /// </summary>
    public partial class ScanNewSkinPage : UserControl
    {
        List<Hero>? heroList { get => MainWindow.GetHeroList(); }
        ModSources? modSources { get => MainWindow.GetModSources(); }
        SkinLevelWrapper? lvWrapper { get => MainWindow.GetSkinLevelWp(); }

        public ScanNewSkinPage()
        {
            InitializeComponent();

            StartScanBtn.Click += StartScanBtn_Click;
            CommitLabelBtn.Click += CommitLabelBtn_Click;
            SaveListBtn.Click += SaveListBtn_Click;
            AddCommitedSkinBtn.Click += AddCommitedSkinBtn_Click;
        }

        private void AddCommitedSkinBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (PathTextedit field in FieldContainer.Children)
            {
                if (field.Tag is Skin skin)
                    MainWindow.AddSkinSelected(skin);
            }
        }

        private void SaveListBtn_Click(object sender, RoutedEventArgs e)
        {
            HeroList list = new() { Heros = heroList };
            File.WriteAllText(DataPaths.HeroList, JsonConvert.SerializeObject(list));
            MessageBox.Show("Saved");
        }

        private void CommitLabelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (heroList == null || modSources == null || lvWrapper == null)
            {
                return;
            }
            foreach (PathTextedit field in FieldContainer.Children)
            {
                if (string.IsNullOrEmpty(field.Text))
                    continue;
                SkinLabel? checkLabel = lvWrapper.SkinLabelLevels.Find(l => l.label.Equals(field.Text, StringComparison.CurrentCultureIgnoreCase));
                if (checkLabel == null)
                {
                    MessageBoxResult result = MessageBox.Show("Label " + field.Text + " not found! Bạn có muốn thêm label vào dữ liệu không?"
                        , "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        InputFieldWindow inputWd = new()
                        {
                            Label = $"Nhập label level cho label '{field.Text}'",
                            IsNumberField = true,
                            Owner = Window.GetWindow(this)
                        };
                        if (inputWd.ShowDialog() == true)
                        {
                            lvWrapper.SkinLabelLevels.Add(new(field.Text, int.Parse(inputWd.InputResult)));
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    field.Text = checkLabel.label;
                }
                if (field.Tag is not KeyValuePair<int, string> pair)
                {
                    if (field.Tag is Skin s)
                    {
                        s.Label = field.Text;
                        TextBlock? txt = CommitedSkinContainer.Children.Cast<TextBlock>().Where(t => s.Id == (int)t.Tag).FirstOrDefault();
                        if (txt != null)
                        {
                            txt.Text = $"- {s.Name}({s.Id}) - {s.Label}";
                        }
                    }
                    continue;
                }
                Skin skin = new()
                {
                    Id = pair.Key,
                    Name = pair.Value,
                    Label = field.Text
                };
                heroList.Find(h => h.Id == int.Parse(skin.Id.ToString()[..3]))?.Skins?.Add(skin);
                TextBlock textBlock = new()
                {
                    Text = $"- {skin.Name}({skin.Id}) - {skin.Label}",
                    Tag = skin.Id
                };
                CommitedSkinContainer.Children.Add(textBlock);
                LogExtension.Log($"Added {skin.Name}({skin.Id}) - {skin.Label}");
                field.Tag = skin;
            }
        }

        private void StartScanBtn_Click(object sender, RoutedEventArgs e)
        {
            if (heroList == null || modSources == null)
            {
                return;
            }
            List<int> skinIdList = heroList.SelectMany(h => h.Skins ?? []).Select(s => s.Id).ToList();
            List<KeyValuePair<int, string>> skinPair = [];
            ProgressWindow progressWd = new() { Owner = Window.GetWindow(this), IsIndeterminate = true };
            progressWd.SetCancelable(false);
            progressWd.Execute(() =>
            {
                Dictionary<string, string> languageMap = [];
                byte[] languageBytes1 = File.ReadAllBytes(
                    Path.Combine(modSources.ResourcesPath, "Languages", modSources.LanguageFolder, "languageMap_Xls.txt"));
                byte[] languageBytes2 = File.ReadAllBytes(
                    Path.Combine(modSources.ResourcesPath, "Languages", modSources.LanguageFolder, "lanMapIncremental.txt"));
                byte[] iconBytes = File.ReadAllBytes(Path.Combine(modSources.DatabinPath, "Actor/heroSkin.bytes"));
                languageBytes1 = AovTranslation.Decompress(languageBytes1) ?? languageBytes1;
                languageBytes2 = AovTranslation.Decompress(languageBytes2) ?? languageBytes2;
                iconBytes = AovTranslation.Decompress(iconBytes) ?? iconBytes;
                IconWrapper iconWrapper = new(iconBytes);
                string language = Encoding.UTF8.GetString(languageBytes1) + Encoding.UTF8.GetString(languageBytes2);
                string[] lines = language.Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] split = line.Split(" = ");
                    if (split.Length < 2 || split[1].Contains("[ex]"))
                        continue;
                    languageMap[split[0].Trim()] = split[1].Trim();
                }

                string[] infoFiles = Directory.GetFiles(modSources.InfosParentPath).Where((filepath) => Path.GetFileName(filepath).StartsWith("Actor_")).ToArray();
                foreach (string info in infoFiles)
                {
                    InfosPackage infoPackage = new(info);
                    string? heroCode = Path.GetFileName(Path.GetDirectoryName(infoPackage.Elements.Keys.ElementAt(0)));
                    if (heroCode == null) continue;
                    foreach (var pair in infoPackage.Elements)
                    {
                        if (pair.Key.EndsWith($"{heroCode}_actorinfo.bytes", StringComparison.CurrentCultureIgnoreCase))
                        {
                            PackageElement element = pair.Value;
                            PackageElement? SkinPrefabs = element.Children?.Find((c) => c._Name.Equals("SkinPrefab", StringComparison.CurrentCultureIgnoreCase));
                            if (SkinPrefabs == null || SkinPrefabs.Children == null)
                                break;
                            foreach (PackageElement skinPrefab in SkinPrefabs.Children)
                            {
                                PackageElement? PrefabLod = skinPrefab?.Children?.Find((c) => c._Name.Equals("ArtSkinPrefabLOD", StringComparison.CurrentCultureIgnoreCase));
                                if (PrefabLod == null || PrefabLod.Children == null)
                                    continue;
                                string lod0 = Path.GetFileName(PrefabLod.Children[0]._Value);
                                string idStr = lod0.Split("_")[0];
                                if (!int.TryParse(idStr, out int id) || skinIdList.Contains(id) || idStr[3..] == "1" || idStr[..3] == "999")
                                {
                                    continue;
                                }
                                id = int.Parse(idStr[..3]) * 100 + int.Parse(idStr[3..]) - 1;
                                string key = iconWrapper.GetIcon(id)?.skinnamecode ?? "";
                                if (languageMap.TryGetValue(key, out string? value) && !value.Contains("[ex]"))
                                {
                                    skinPair.Add(KeyValuePair.Create(int.Parse(idStr), value));
                                    LogExtension.Log(id + ":" + value);
                                }
                            }

                            break;
                        }
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    FieldContainer.Children.Clear();
                    foreach (var pair in skinPair)
                    {
                        PathTextedit text = new()
                        {
                            PathLabel = pair.Key + " - " + pair.Value + ": ",
                            Tag = pair,
                            ToolTip = heroList.Find(h => h.Id == int.Parse(pair.Key.ToString()[..3]))?.Name + pair.Value
                        };
                        FieldContainer.Children.Add(text);

                    }
                });
            });
            progressWd.ShowDialog();
        }
    }
}
