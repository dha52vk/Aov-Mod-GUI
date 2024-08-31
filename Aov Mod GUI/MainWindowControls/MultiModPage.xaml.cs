using Aov_Mod_GUI.Models;
using Aov_Mod_GUI.Models.DataSave;
using AovClass;
using AovClass.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aov_Mod_GUI.MainWindowControls
{
    /// <summary>
    /// Interaction logic for MultiModPage.xaml
    /// </summary>
    public partial class MultiModPage : UserControl
    {
        ObservableCollection<MultiSkinPack> MultiSkinPacks { get => MainWindow.GetMultiSkinPacks(); }

        public MultiModPage()
        {
            InitializeComponent();

            SelectSkinPackGrid.Visibility = Visibility.Collapsed;
            ConfigSkinPacksGrid.Visibility = Visibility.Collapsed;

            Loaded += MultiModPage_Loaded;
            AddHeroButton.Click += AddHeroButton_Click;
            StartMultiModButton.Click += StartMultiModButton_Click;
            ConfigMultiModPackButton.Click += ConfigMultiModPackButton_Click;
            AddMultiModPackButton.Click += AddMultiModPackButton_Click;
            AddPackBtn.Click += AddPackBtn_Click;
            CommitSkinPackButton.Click += CommitAddSkinPackButton_Click;
            SelectSkinPackBackground.MouseDown += SelectSkinPackBackground_MouseDown;
            ConfigSkinPacksBackground.MouseDown += ConfigSkinPacksBackground_MouseDown;
        }

        private void CommitAddSkinPackButton_Click(object sender, RoutedEventArgs e)
        {
            if (SkinPacksCbb.SelectedItem is MultiSkinPack pack)
            {
                List<Skin>? skins = MainWindow.GetHeroList()?.SelectMany(h => h.Skins ?? []).ToList();
                if (skins != null)
                {
                    var multiSkins = pack.MultiSkins;
                    foreach (var multiSkin in multiSkins)
                    {
                        bool exists = false;
                        foreach (HeroMultiModField field in HeroFieldContainer.Children)
                        {
                            if (field.hero.Id == multiSkin.heroId)
                            {
                                exists = true;
                                break;
                            }
                        }
                        Hero? hero = MainWindow.GetHeroList()?.Find(hero => hero.Id == multiSkin.heroId);
                        if (!exists && hero != null)
                        {
                            HeroMultiModField field = new(hero);
                            field.RemoveClick = (sender, e) =>
                            {
                                HeroFieldContainer.Children.Remove(field);
                            };
                            field.TrySetSkinIdPair(multiSkin.SkinChanges);
                            HeroFieldContainer.Children.Add(field);
                        }
                    }
                }
            }
            SelectSkinPackGrid.Visibility = Visibility.Collapsed;
        }

        private void AddPackBtn_Click(object sender, RoutedEventArgs e)
        {
            InputFieldWindow inputWd = new() { Owner = Window.GetWindow(this), Label = "Nhập tên pack:" };
            if (inputWd.ShowDialog() == true)
            {
                if (MultiSkinPacks.Any(pack => pack.PackName == inputWd.InputResult))
                {
                    MessageBox.Show("Pack '" + inputWd.InputResult + "' đã tồn tại");
                }
                else
                {
                    List<MultiSkin> multiSkins = [];
                    foreach (HeroMultiModField heroField in HeroFieldContainer.Children)
                    {
                        multiSkins.Add(new(heroField.hero.Id, heroField.SkinIdChanges));
                    }
                    MultiSkinPack skinPack = new(inputWd.InputResult, multiSkins);
                    SkinPackField field = new()
                    {
                        FieldLabel = skinPack.PackName,
                        Tag = skinPack.PackName,
                        FontSize = 18,
                        TextColor = Brushes.White
                    };
                    field.SkinListTxt.Text = String.Join("; ", skinPack.MultiSkins);
                    field.RemoveButtonClick = (sender, e) =>
                    {
                        SkinPackContainer.Children.Remove(field);
                        MultiSkinPacks.Remove(skinPack);
                        MainWindow.SaveMultiSkinPacks();
                    };
                    SkinPackContainer.Children.Add(field);
                    MultiSkinPacks.Add(skinPack);
                    MainWindow.SaveMultiSkinPacks();
                }
            }
        }

        private void AddMultiModPackButton_Click(object sender, RoutedEventArgs e)
        {
            SelectSkinPackGrid.Visibility = Visibility.Visible;
        }

        private void ConfigMultiModPackButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigSkinPacksGrid.Visibility = Visibility.Visible;
        }

        private void ConfigSkinPacksBackground_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfigSkinPacksGrid.Visibility = Visibility.Collapsed;
        }

        private void SelectSkinPackBackground_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectSkinPackGrid.Visibility = Visibility.Collapsed;
        }

        private void StartMultiModButton_Click(object sender, RoutedEventArgs e)
        {
            ModController? modController = MainWindow.GetModController();
            if (modController == null)
            {
                return;
            }
            string content = "Skins Selected: \n";
            List<ModMultiInfo> modMultiInfos = [];
            foreach (HeroMultiModField field in HeroFieldContainer.Children)
            {
                modMultiInfos.Add(new(field.SkinChanges, ModSettings.AllEnable));
            }
            ProgressWindow progressWindow = new() { Owner = Window.GetWindow(this), IsIndeterminate = true };
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
                    string packname = "PackMultiMod_" + DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss");
                    modController.ModMultiSkin(modMultiInfos, packname);
                    File.WriteAllText(Path.Combine(MainWindow.GetModSources()?.SaveModPath ?? "", ModController.MakeSimpleString(packname), "packinfo.txt"), content);
                }
            });
            progressWindow.ShowDialog();
            MessageBox.Show("Saved");
        }

        private void AddHeroButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddHeroComboBox.SelectedItem is Hero hero)
            {
                HeroMultiModField field = new(hero);
                field.RemoveClick = (sender, e) =>
                {
                    HeroFieldContainer.Children.Remove(field);
                };
                HeroFieldContainer.Children.Add(field);
            }
        }

        private void MultiModPage_Loaded(object sender, RoutedEventArgs e)
        {
            AddHeroComboBox.ItemsSource = MainWindow.GetHeroList();
            SkinPacksCbb.ItemsSource = MultiSkinPacks;
            foreach (MultiSkinPack skinPack in MultiSkinPacks)
            {
                SkinPackField field = new()
                {
                    FieldLabel = skinPack.PackName,
                    Tag = skinPack.PackName,
                    FontSize = 18,
                    TextColor = Brushes.White
                };
                field.SkinListTxt.Text = String.Join("; ", skinPack.MultiSkins);
                field.RemoveButtonClick = (sender, e) =>
                {
                    SkinPackContainer.Children.Remove(field);
                    MultiSkinPacks.Remove(skinPack);
                    MainWindow.SaveMultiSkinPacks();
                };
                SkinPackContainer.Children.Add(field);
            }
        }
    }
}
