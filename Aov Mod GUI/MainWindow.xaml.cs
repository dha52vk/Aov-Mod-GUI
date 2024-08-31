using Aov_Mod_GUI.MainWindowControls;
using Aov_Mod_GUI.Models;
using Aov_Mod_GUI.Models.DataSave;
using AovClass;
using AovClass.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Aov_Mod_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsMenuOpen = false;

        private static ObservableCollection<Hero>? Heroes;
        private static ObservableCollection<SkinPack> SkinPacks = [];
        private static ObservableCollection<MultiSkinPack> MultiSkinPacks = [];
        private static ModSources? modSources;
        private static SkinLevelWrapper? levelWrapper;
        private static ModController? modController;
        private static readonly ObservableCollection<Skin> SkinsSelected = [];

        public int MaxGridColumn = 150;

        private static Panel? static_LabelLevelFieldContainter;
        private static Panel? static_SkinLevelFieldContainter;
        public UniformGrid? ListItemUniformGrid;
        public UniformGrid? ListSkinSelectedUniformGrid;
        public List<FrameworkElement> UIPages = [];

        public bool IsListLoaded;
        private static bool isCustomizing;
        static bool IsBusy { get => isCustomizing; }

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException; ;
            static_LabelLevelFieldContainter = LabelLevelFieldContainer;
            static_SkinLevelFieldContainter = SkinLevelFieldContainer;

            SaveSettingsButton.Click += SaveSettingsButton_Click;
            AddPackBtn.Click += AddPackBtn_Click;
            AddSkinsBtn.Click += AddSkinsBtn_Click;
            CommitSkinPackButton.Click += CommitSkinPackButton_Click;
            ConfigSkinPackBtn.Click += ConfigSkinPackBtn_Click;
            AddLabelLevelField.Click += AddLabelLevelField_Click;
            AddSkinLevelField.Click += AddSkinLevelField_Click;

            SelectSkinPackBackground.MouseDown += SelectPackBackground_MouseDown;
            ConfigSkinPacksBackground.MouseDown += ConfigSkinPacksBackground_MouseDown;

            SelectSkinPackGrid.Visibility = Visibility.Collapsed;
            ConfigSkinPacksGrid.Visibility = Visibility.Collapsed;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(PageContainer); i++)
            {
                FrameworkElement? child = (FrameworkElement?)VisualTreeHelper.GetChild(PageContainer, i);
                if (child != null)
                {
                    UIPages.Add(child);
                    if (i == 0)
                    {
                        child.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                    }
                }
            }

            OpenFileDialog openFile = new()
            {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true
            };
            Directory.CreateDirectory("data");
            if (!File.Exists(DataPaths.SkinPackList))
            {
                File.WriteAllText(DataPaths.SkinPackList, JsonConvert.SerializeObject(new SkinPackList([.. SkinPacks])));
            }
            if (!File.Exists(DataPaths.MultiSkinPackList))
            {
                File.WriteAllText(DataPaths.MultiSkinPackList, JsonConvert.SerializeObject(new MultiSkinPackList([.. MultiSkinPacks])));
            }
            if (!File.Exists(DataPaths.HeroList))
            {
                JsonMissing:
                MessageBox.Show("Không tìm thấy skin list hoặc đã bị hỏng! Vui lòng nhập lại Skin List Json!"
                    , "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                if (openFile.ShowDialog() == true)
                {
                    if (!CheckJson<HeroList>(File.ReadAllText(openFile.FileName)))
                    {
                        goto JsonMissing;
                    }
                    else
                    {
                        File.Copy(openFile.FileName, DataPaths.HeroList);
                    }
                }
            }
            if (!File.Exists(DataPaths.SkinLevels))
            {
                JsonMissing:
                MessageBox.Show("Không tìm thấy skin level wrapper hoặc đã bị hỏng! Vui lòng nhập lại SkinLevelsWrapper Json!"
                    , "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                if (openFile.ShowDialog() == true)
                {
                    if (!CheckJson<SkinLevelWrapper>(File.ReadAllText(openFile.FileName)))
                    {
                        goto JsonMissing;
                    }
                    else
                    {
                        File.Copy(openFile.FileName, DataPaths.SkinLevels);
                    }
                }
            }
            if (!File.Exists(DataPaths.ModSources))
            {
                JsonMissing:
                MessageBox.Show("Không tìm thấy Mod Sources hoặc đã bị hỏng! Vui lòng nhập lại ModSources Json!"
                    , "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                if (openFile.ShowDialog() == true)
                {
                    if (!CheckJson<ModSources>(File.ReadAllText(openFile.FileName)))
                    {
                        goto JsonMissing;
                    }
                    else
                    {
                        File.Copy(openFile.FileName, DataPaths.ModSources);
                    }
                }
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            LogExtension.Log("Exception " + ex.StackTrace ?? "");
        }

        private void AddSkinLevelField_Click(object sender, RoutedEventArgs e)
        {
            FilterField filterField = new()
            {
                FilterLabel = "Special:",
                IsAttributeField = true,
                IsNumberName = true,
                IsNumberValue = true
            };
            filterField.RemoveClick = () =>
            {
                SkinLevelFieldContainer.Children.Remove(filterField);
            };
            SkinLevelFieldContainer.Children.Add(filterField);
        }

        private void AddLabelLevelField_Click(object sender, RoutedEventArgs e)
        {
            FilterField filterField = new()
            {
                FilterLabel = "Label:",
                IsAttributeField = true,
                IsNumberValue = true
            };
            filterField.RemoveClick = () =>
            {
                LabelLevelFieldContainer.Children.Remove(filterField);
            };
            LabelLevelFieldContainer.Children.Add(filterField);
        }

        private void ConfigSkinPacksBackground_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfigSkinPacksGrid.Visibility = Visibility.Collapsed;
        }

        private void ConfigSkinPackBtn_Click(object sender, RoutedEventArgs e)
        {
            ConfigSkinPacksGrid.Visibility = Visibility.Visible;
        }

        private void CommitSkinPackButton_Click(object sender, RoutedEventArgs e)
        {
            if (SkinPacksCbb.SelectedItem is SkinPack pack)
            {
                List<Skin>? skins = Heroes?.SelectMany(h => h.Skins ?? []).ToList();
                if (skins != null)
                {
                    var skinIdAdds = pack.SkinIds.Where(id => !SkinsSelected.Any(s => s.Id == id));
                    foreach (var skinId in skinIdAdds)
                    {
                        var skin = skins.Find(s => s.Id == skinId);
                        if (skin != null)
                        {
                            SkinsSelected.Add(skin);
                        }
                    }
                }
            }
            SelectSkinPackGrid.Visibility = Visibility.Collapsed;
        }

        private void SelectPackBackground_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectSkinPackGrid.Visibility = Visibility.Collapsed;
        }

        private void AddSkinsBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectSkinPackGrid.Visibility = Visibility.Visible;
        }

        private void AddPackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SkinsSelected.Count < 2)
                return;
            InputFieldWindow inputWd = new() { Owner = this, Label = "Nhập tên pack:" };
            if (inputWd.ShowDialog() == true)
            {
                SkinPack skinPack = new(inputWd.InputResult, SkinsSelected.Select(s => s.Id).ToList());
                SkinPackField field = new()
                {
                    FieldLabel = skinPack.PackName,
                    SkinList = skinPack.SkinIds,
                    Tag = skinPack.PackName,
                    FontSize = 18,
                    TextColor = Brushes.White
                };
                field.RemoveButtonClick = (sender, e) =>
                {
                    SkinPackContainer.Children.Remove(field);
                    SkinPacks.Remove(skinPack);
                    SaveSkinPacks();
                };
                SkinPackContainer.Children.Add(field);
                SkinPacks.Add(skinPack);
                File.WriteAllText(DataPaths.SkinPackList, JsonConvert.SerializeObject(new SkinPackList([.. SkinPacks])));
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (modSources == null || levelWrapper == null)
            {
                return;
            }
            modSources.ChannelName = ChannelNameField.Text;
            modSources.YtbLink = YoutubeLinkField.Text;
            modSources.AovVersion = AovVersionField.Text;
            modSources.LanguageCode = LanguageCodeField.Text;
            modSources.ResourcesPath = ResourcesPathField.Text;
            modSources.SaveModPath = SavePathField.Text;
            foreach (FilterField field in LabelLevelFieldContainer.Children)
            {
                if (field.Tag == null && !string.IsNullOrEmpty(field.Value) && !string.IsNullOrEmpty(field.AttributeName))
                {
                    levelWrapper.SkinLabelLevels.Add(new(field.AttributeName, int.Parse(field.Value)));
                }
            }
            foreach (FilterField field in SkinLevelFieldContainer.Children)
            {
                if (field.Tag == null && !string.IsNullOrEmpty(field.Value) && !string.IsNullOrEmpty(field.AttributeName))
                {
                    levelWrapper.SpecialSkinLevels.Add(new(int.Parse(field.AttributeName), int.Parse(field.Value)));
                }
            }
            File.WriteAllText(DataPaths.SkinLevels, JsonConvert.SerializeObject(levelWrapper));
            File.WriteAllText(DataPaths.ModSources, JsonConvert.SerializeObject(modSources));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadSources();
            ListItem.ItemsSource = Heroes;
            ListViewSkinSelected.ItemsSource = SkinsSelected;

            ItemsPresenter? itemsPresenter = DHAExtensions.GetVisualChild<ItemsPresenter>(ListItem);
            ListItemUniformGrid = VisualTreeHelper.GetChild(itemsPresenter, 0) as UniformGrid;
            IsListLoaded = true;
        }

        private void MainWd_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int width = (int)Math.Round(e.NewSize.Width);
            int col = (int)Math.Ceiling((double)width / MaxGridColumn);
            if (ListItemUniformGrid != null)
            {
                ListItemUniformGrid.Columns = col;
            }
        }

        private void TitleBar_MenuButtonClicked(object sender, System.EventArgs e)
        {
            if (!IsBusy)
            {
                if (IsMenuOpen)
                {
                    this.PlayStoryBoard("CloseMenu");
                    MenuBlurEff.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MenuBlurEff.Visibility = Visibility.Visible;
                    this.PlayStoryBoard("OpenMenu");
                }
                IsMenuOpen = !IsMenuOpen;
            }
            else
            {
                MessageBox.Show("Hiện không thể đổi menu vì đang bận!");
            }
        }

        private void MenuBtn_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var child in UIPages)
            {
                if ((sender as FrameworkElement)?.Tag as string == child.Name)
                {
                    child.Visibility = Visibility.Visible;
                }
                else
                {
                    child.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ListItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                var selectedItem = (FrameworkElement)e.OriginalSource;
                var itemDataContext = selectedItem.DataContext;

                if (itemDataContext is Hero hero)
                {
                    if (hero == null)
                        return;
                    MenuBlurEff.Visibility = Visibility.Visible;
                    if (hero.Skins == null)
                    {
                        MessageBox.Show("Skin này không có skin!", "Thông tin", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    }
                    else
                    {
                        ChooseSkinWindow chooseSkinWindow = new(hero.Skins)
                        {
                            WindowStartupLocation = WindowStartupLocation.CenterOwner,
                            Owner = this
                        };
                        if (chooseSkinWindow.ShowDialog() == true && chooseSkinWindow.Result != null)
                        {
                            //MessageBox.Show("Ban da chon skin " + chooseSkinWindow.Result.name);
                            AddSkinSelected(chooseSkinWindow.Result);
                        }
                        else
                        {
                            //MessageBox.Show("Ban da huy chon skin");
                        }
                        MenuBlurEff.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string result = "";
                string skinListPath = SkinListPathText.GetText(),
                    skinLevelPath = SkinLevelPathText.GetText(),
                    modSourcesPath = ModSourcesPathText.GetText();
                if (!string.IsNullOrEmpty(skinListPath) && File.Exists(skinListPath) && CheckJson<HeroList>(File.ReadAllText(skinListPath)))
                {
                    result = "✅ Hero List".PadRight(40) + "\n" + result;
                    File.Delete(DataPaths.HeroList);
                    File.Copy(skinListPath, DataPaths.HeroList);
                }
                else
                {
                    result += "\n❎ Hero List";
                }

                if (!string.IsNullOrEmpty(skinLevelPath) && File.Exists(skinLevelPath) && CheckJson<HeroList>(File.ReadAllText(skinLevelPath)))
                {
                    result = "✅ Skin Level".PadRight(40) + "\n" + result;
                    File.Delete(DataPaths.SkinLevels);
                    File.Copy(skinLevelPath, DataPaths.SkinLevels);
                }
                else
                {
                    result += "\n❎ Skin Level";
                }

                if (!string.IsNullOrEmpty(modSourcesPath) && File.Exists(modSourcesPath) && CheckJson<HeroList>(File.ReadAllText(modSourcesPath)))
                {
                    result = "✅ Mod Sources".PadRight(40) + "\n" + result;
                    File.Delete(DataPaths.ModSources);
                    File.Copy(modSourcesPath, DataPaths.ModSources);
                }
                else
                {
                    result += "\n❎ Mod Sources";
                }
                ReloadSources();
                MessageBox.Show(result, "Nhập dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Import data failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            string content = "Skins Selected: \n";
            if (SkinsSelected == null)
                return;

            List<ModInfo> modList = [];
            foreach (Skin skin in SkinsSelected)
            {
                content += $"  + {skin.Name}\n";
                var oldSkins = GetSkinLevelA(int.Parse(skin.Id.ToString()?[..3] ?? "-1"))?.Where(s => s.Id != skin.Id);
                if (oldSkins != null)
                    modList.Add(new(oldSkins.ToList(), skin, ModSettings.AllEnable));
            }
            ProgressWindow progressWindow = new() { Owner = this, IsIndeterminate = true };
            progressWindow.SetCancelable(false);
            progressWindow.Execute(() =>
            {
                if (modController != null)
                {
                    modController.UpdateProgress = (status) =>
                    {
                        LogExtension.Log(status);
                        progressWindow.UpdateProgress(0, status);
                    };
                    string packname = "PackMod_" + DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss");
                    modController.ModSkin(modList, packname);
                    File.WriteAllText(Path.Combine(modSources?.SaveModPath ?? "", ModController.MakeSimpleString(packname), "packinfo.txt"), content);
                }
            });
            progressWindow.ShowDialog();
            MessageBox.Show("Saved");
        }

        private void ListViewSkinSelected_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItem = (FrameworkElement)e.OriginalSource;
            var itemDataContext = selectedItem.DataContext;
            if (itemDataContext is Skin skin)
            {
                //MessageBox.Show($"  + {skin.name} ({skin.id})\n");
                SkinsSelected?.Remove(skin);
            }
        }

        private void MainWd_Closing(object sender, CancelEventArgs e)
        {
            if (IsBusy)
            {
                var result = MessageBox
                    .Show("Hiện ứng dụng có công việc đang làm dở! Bạn có chắc muốn thoát không?\nThoát sẽ mất dữ liệu đang làm dở!"
                    , "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private void ReloadSources()
        {
            HeroList? heroList = JsonConvert.DeserializeObject<HeroList>(File.ReadAllText(DataPaths.HeroList));
            levelWrapper = JsonConvert.DeserializeObject<SkinLevelWrapper>(File.ReadAllText(DataPaths.SkinLevels));
            modSources = JsonConvert.DeserializeObject<ModSources>(File.ReadAllText(DataPaths.ModSources));
            SkinPacks = [.. JsonConvert.DeserializeObject<SkinPackList>(File.ReadAllText(DataPaths.SkinPackList))?.SkinPacks];
            MultiSkinPacks = [.. JsonConvert.DeserializeObject<MultiSkinPackList>(File.ReadAllText(DataPaths.MultiSkinPackList))?.PackList];
            #region ModSources Settings Init
            if (modSources != null)
            {
                ChannelNameField.Text = modSources.ChannelName;
                YoutubeLinkField.Text = modSources.YtbLink;
                AovVersionField.Text = modSources.AovVersion;
                LanguageCodeField.Text = modSources.LanguageCode;
                ResourcesPathField.Text = modSources.ResourcesPath;
                SavePathField.Text = modSources.SaveModPath;
            }
            #endregion
            #region Label Wrapper
            if (levelWrapper != null)
            {
                LabelLevelFieldContainer.Children.Clear();
                foreach (var label in levelWrapper.SkinLabelLevels)
                {
                    FilterField filterField = new()
                    {
                        FilterLabel = "Label:",
                        IsAttributeField = true,
                        AttributeName = label.label,
                        Value = label.skinLevel.ToString(),
                        IsNumberValue = true,
                        Tag = label
                    };
                    filterField.RemoveClick = () =>
                    {
                        LabelLevelFieldContainer.Children.Remove(filterField);
                        levelWrapper.SkinLabelLevels.Remove(label);
                    };
                    LabelLevelFieldContainer.Children.Add(filterField);
                }
                SkinLevelFieldContainer.Children.Clear();
                foreach (var specialLevel in levelWrapper.SpecialSkinLevels)
                {
                    FilterField filterField = new()
                    {
                        FilterLabel = "Special:",
                        IsAttributeField = true,
                        AttributeName = specialLevel.id.ToString(),
                        Value = specialLevel.skinLevel.ToString(),
                        IsNumberValue = true,
                        Tag = specialLevel
                    };
                    filterField.RemoveClick = () =>
                    {
                        SkinLevelFieldContainer.Children.Remove(filterField);
                        levelWrapper.SpecialSkinLevels.Remove(specialLevel);
                    };
                    SkinLevelFieldContainer.Children.Add(filterField);
                }

            }
            #endregion
            foreach (SkinPack skinPack in SkinPacks)
            {
                SkinPackField field = new()
                {
                    FieldLabel = skinPack.PackName,
                    SkinList = skinPack.SkinIds,
                    Tag = skinPack.PackName,
                    FontSize = 18,
                    TextColor = Brushes.White
                };
                field.RemoveButtonClick = (sender, e) =>
                {
                    SkinPackContainer.Children.Remove(field);
                    SkinPacks.Remove(skinPack);
                    SaveSkinPacks();
                };
                SkinPackContainer.Children.Add(field);
            }
            if (heroList != null && heroList.Heros != null)
            {
                heroList.Heros.Sort((a, b) => string.Compare(a.Name, b.Name));
                Heroes = new ObservableCollection<Hero>(heroList.Heros);
                if (modSources != null && levelWrapper != null)
                {
                    modController = new(modSources, levelWrapper) { heroList = [.. Heroes] };
                }
            }
            if (SkinPacks != null)
            {
                SkinPacksCbb.ItemsSource = SkinPacks;
            }
        }

        internal static void SaveSkinPacks()
        {
            if (SkinPacks != null)
                File.WriteAllText(DataPaths.SkinPackList, JsonConvert.SerializeObject(new SkinPackList([.. SkinPacks])));
        }

        internal static void SaveMultiSkinPacks()
        {
            if (MultiSkinPacks != null)
                File.WriteAllText(DataPaths.MultiSkinPackList, JsonConvert.SerializeObject(new MultiSkinPackList([.. MultiSkinPacks])));
        }

        public static void SaveHeroList()
        {
            if (Heroes != null)
                File.WriteAllText(DataPaths.HeroList, JsonConvert.SerializeObject(new HeroList() { Heros = [.. Heroes] }));
        }

        public static void SaveModSources()
        {
            if (modSources != null)
                File.WriteAllText(DataPaths.ModSources, JsonConvert.SerializeObject(modSources));
        }

        public static void AddLabelLevel(SkinLabel label, bool save = false)
        {
            FilterField filterField = new()
            {
                FilterLabel = "Label:",
                IsAttributeField = true,
                AttributeName = label.label,
                Value = label.skinLevel.ToString(),
                IsNumberValue = true,
                Tag = label
            };
            filterField.RemoveClick = () =>
            {
                static_LabelLevelFieldContainter?.Children.Remove(filterField);
                levelWrapper?.SkinLabelLevels.Remove(label);
            };
            static_LabelLevelFieldContainter?.Children.Add(filterField);
            levelWrapper?.SkinLabelLevels.Add(label);
            if (save)
            {
                File.WriteAllText(DataPaths.SkinLevels, JsonConvert.SerializeObject(levelWrapper));
            }
        }

        internal static ObservableCollection<MultiSkinPack> GetMultiSkinPacks()
        {
            return MultiSkinPacks;
        }

        public static ModController? GetModController()
        {
            return modController;
        }

        public static List<Hero>? GetHeroList()
        {
            return Heroes?.ToList();
        }

        public static ModSources? GetModSources()
        {
            return modSources;
        }

        public static SkinLevelWrapper? GetSkinLevelWrapper()
        {
            return levelWrapper;
        }

        public static void SetCustomizingState(bool _isCustomizing)
        {
            isCustomizing = _isCustomizing;
        }

        public static bool CheckHeroIdExists(int heroId)
        {
            return Heroes?.Select((h) => h.Id).Contains(heroId) ?? false;
        }

        public static bool CheckSkinIdExists(int skinId)
        {
            return Heroes?.SelectMany(h => h.Skins ?? []).Select((s) => s.Id).Contains(skinId) ?? false;
        }

        public static List<Skin>? GetSkinLevelA(int heroId)
        {
            Hero? hero = Heroes?.ToList().Find((hero) => hero.Id == heroId);
            if (hero == null || levelWrapper == null)
                return null;
            List<Skin> skin = [new Skin() { Id = heroId * 10 + 1, Label = "Default", Name = "" }];
            skin.AddRange(hero?.Skins?.Where((skin) => levelWrapper.GetSkinLevel(skin) <= (int)DefaultLevel.A) ?? []);
            return skin;
        }

        public static void AddSkinSelected(Skin skin)
        {
            Skin? skinHad = SkinsSelected.Where(s => s.Id.ToString()[..3] == skin.Id.ToString()[..3]).FirstOrDefault();
            if (skinHad != null)
                SkinsSelected.Remove(skinHad);
            SkinsSelected.Add(skin);
        }

        static bool CheckJson<T>(string json)
        {
            try
            {
                T? t = JsonConvert.DeserializeObject<T>(json);
                return t != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
