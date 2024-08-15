using AovClass;
using AovClass.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace Aov_Mod_GUI.CustomModWd
{
    /// <summary>
    /// Interaction logic for EditSkinInfoWindow.xaml
    /// </summary>
    public partial class EditSkinInfoWindow : Window
    {
        readonly Skin skin;
        SkinLevelWrapper? levelWrapper = MainWindow.GetSkinLevelWrapper();
        ModSources? modSources = MainWindow.GetModSources();

        public EditSkinInfoWindow(Skin skin)
        {
            InitializeComponent();
            this.skin = skin;

            AddParticleNotModBtn.Click += AddParticleNotModBtn_Click;
            AddFileNameNotModBtn.Click += AddFileNameNotModBtn_Click;
            AddFileNameNotCheckIdModBtn.Click += AddFileNameNotCheckIdModBtn_Click;
            CancelButton.Click += CancelButton_Click;
            CommitButton.Click += CommitButton_Click;

            ContentRendered += EditSkinInfoWindow_ContentRendered;
            KeyDown += EditSkinInfoWindow_KeyDown;
        }

        private void EditSkinInfoWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitButton_Click(new(), new());
            }
            else if (e.Key == Key.Escape)
            {
                CancelButton_Click(new(), new());
            }
        }

        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(IdTextedit.Text)) skin.Id = int.Parse(IdTextedit.Text);
            if (!string.IsNullOrEmpty(NameTextedit.Text)) skin.Name = NameTextedit.Text;
            if (!string.IsNullOrEmpty(LabelTextedit.Text))
            {
                if (!string.IsNullOrEmpty(LabelTextedit.Text))
                {
                    SkinLabel? checkLabel = levelWrapper?.SkinLabelLevels.Find(l => l.label.Equals(LabelTextedit.Text, StringComparison.CurrentCultureIgnoreCase));
                    if (checkLabel == null)
                    {
                        MessageBoxResult result = MessageBox.Show("Label " + LabelTextedit.Text + " not found! Bạn có muốn thêm label vào dữ liệu không?"
                            , "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                        if (result == MessageBoxResult.Yes)
                        {
                            InputFieldWindow inputWd = new()
                            {
                                Label = $"Nhập label level cho label '{LabelTextedit.Text}'",
                                IsNumberField = true,
                                Owner = Window.GetWindow(this)
                            };
                            if (inputWd.ShowDialog() == true)
                            {
                                MainWindow.AddLabelLevel(new(LabelTextedit.Text, int.Parse(inputWd.InputResult)), true);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        LabelTextedit.Text = checkLabel.label;
                    }
                    skin.Label = LabelTextedit.Text;
                }
            }
            int index = modSources?.SkinNotSwapIcon.FindIndex(id => id == skin.Id) ?? -1;
            if (SwapIconCheckbox.IsChecked == false && index == -1)
            {
                modSources?.SkinNotSwapIcon.Add(skin.Id);
            }
            else if (SwapIconCheckbox.IsChecked == true && index != -1)
            {
                modSources?.SkinNotSwapIcon.Remove(skin.Id);
            }
            skin.ChangeAnim = ChangeAnimCheckbox.IsChecked ?? false;
            skin.HasDeathEffect = HasDeathEffectCheckbox.IsChecked ?? false;
            skin.IsAwakeSkin = IsAwakeSkinCheckbox.IsChecked ?? false;
            if (!string.IsNullOrEmpty(AwakeSfxLevelTextedit.Text)) skin.LevelSFXUnlock = int.Parse(AwakeSfxLevelTextedit.Text);
            if (!string.IsNullOrEmpty(AwakeVoxLevelTextedit.Text)) skin.LevelVOXUnlock = int.Parse(AwakeVoxLevelTextedit.Text);
            skin.NotAddExtraBack = NotAddExtraBackCheckbox.IsChecked ?? false;
            skin.IsComponentSkin = IsComponentSkinCheckbox.IsChecked ?? false;
            if (!string.IsNullOrEmpty(ComponentEffectIdTextedit.Text)) skin.ComponentEffectId = ComponentEffectIdTextedit.Text;
            if (!string.IsNullOrEmpty(ComponentLevelTextedit.Text)) skin.ComponentLevel = int.Parse(ComponentLevelTextedit.Text);
            if (!string.IsNullOrEmpty(SpecialBackAnimTextedit.Text)) skin.SpecialBackAnim = SpecialBackAnimTextedit.Text;
            if (!string.IsNullOrEmpty(HasteNameTextedit.Text)) skin.HasteName = HasteNameTextedit.Text;
            if (!string.IsNullOrEmpty(HasteNameRunTextedit.Text)) skin.HasteNameRun = HasteNameRunTextedit.Text;
            if (!string.IsNullOrEmpty(HasteNameEndTextedit.Text)) skin.HasteNameEnd = HasteNameEndTextedit.Text;

            if (ParticleNotModContainer.Children.Count != 0)
            {
                skin.ParticleNotMod = [];
                foreach (PathTextedit textedit in ParticleNotModContainer.Children)
                {
                    skin.ParticleNotMod.Add(textedit.Text);
                }
            }
            else
            {
                skin.ParticleNotMod = null;
            }

            if (FileNameNotModContainer.Children.Count != 0)
            {
                skin.FilenameNotMod = [];
                foreach (PathTextedit textedit in FileNameNotModContainer.Children)
                {
                    skin.FilenameNotMod.Add(textedit.Text);
                }
            }
            else
            {
                skin.FilenameNotMod = null;
            }

            if (FileNameNotModCheckIdContainer.Children.Count != 0)
            {
                skin.FilenameNotModCheckId = [];
                foreach (PathTextedit textedit in FileNameNotModCheckIdContainer.Children)
                {
                    skin.FilenameNotModCheckId.Add(textedit.Text);
                }
            }
            else
            {
                skin.FilenameNotModCheckId = null;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddFileNameNotCheckIdModBtn_Click(object sender, RoutedEventArgs e)
        {
            PathTextedit textedit = new();
            textedit.PathLabel = "File:";
            FileNameNotModCheckIdContainer.Children.Add(textedit);
        }

        private void AddFileNameNotModBtn_Click(object sender, RoutedEventArgs e)
        {
            PathTextedit textedit = new();
            textedit.PathLabel = "File:";
            FileNameNotModContainer.Children.Add(textedit);
        }

        private void AddParticleNotModBtn_Click(object sender, RoutedEventArgs e)
        {
            PathTextedit textedit = new();
            textedit.PathLabel = "Particle:";
            ParticleNotModContainer.Children.Add(textedit);
        }

        private void EditSkinInfoWindow_ContentRendered(object? sender, EventArgs e)
        {
            IdTextedit.Text = skin.Id.ToString();
            NameTextedit.Text = skin.Name;
            LabelTextedit.Text = skin.Label;
            SwapIconCheckbox.IsChecked = modSources?.SkinNotSwapIcon.FindIndex(id => id == skin.Id) == -1;
            ChangeAnimCheckbox.IsChecked = skin.ChangeAnim;
            HasDeathEffectCheckbox.IsChecked = skin.HasDeathEffect;
            IsAwakeSkinCheckbox.IsChecked = skin.IsAwakeSkin;
            AwakeSfxLevelTextedit.Text = skin.LevelSFXUnlock.ToString() ?? "";
            AwakeVoxLevelTextedit.Text = skin.LevelVOXUnlock.ToString() ?? "";
            NotAddExtraBackCheckbox.IsChecked = skin.NotAddExtraBack;
            IsComponentSkinCheckbox.IsChecked = skin.IsComponentSkin;
            ComponentEffectIdTextedit.Text = skin.ComponentEffectId ?? "";
            ComponentLevelTextedit.Text = skin.ComponentLevel.ToString() ?? "";
            SpecialBackAnimTextedit.Text = skin.SpecialBackAnim ?? "";
            HasteNameTextedit.Text = skin.HasteName ?? "";
            HasteNameRunTextedit.Text = skin.HasteNameRun ?? "";
            HasteNameEndTextedit.Text = skin.HasteNameEnd ?? "";

            foreach (string particle in skin.ParticleNotMod ?? [])
            {
                PathTextedit textedit = new();
                textedit.PathLabel = "Particle:";
                textedit.Text = particle;
                ParticleNotModContainer.Children.Add(textedit);
            }
            foreach (string file in skin.FilenameNotMod ?? [])
            {
                PathTextedit textedit = new();
                textedit.PathLabel = "File:";
                textedit.Text = file;
                FileNameNotModContainer.Children.Add(textedit);
            }
            foreach (string file in skin.FilenameNotModCheckId ?? [])
            {
                PathTextedit textedit = new();
                textedit.PathLabel = "File:";
                textedit.Text = file;
                FileNameNotModCheckIdContainer.Children.Add(textedit);
            }
        }
    }
}
