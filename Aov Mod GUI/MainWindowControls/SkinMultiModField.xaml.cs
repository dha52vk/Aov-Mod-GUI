using AovClass.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Aov_Mod_GUI.MainWindowControls
{
    /// <summary>
    /// Interaction logic for SkinMultiModField.xaml
    /// </summary>
    public partial class SkinMultiModField : UserControl
    {
        Hero hero;
        List<Skin> skins;
        
        public Skin? OldSkin { get => (Skin?)OldSkinCbb.SelectedItem; }
        public Skin? NewSkin { get => (Skin?)NewSkinCbb.SelectedItem; }

        public RoutedEventHandler? RemoveClick;

        public SkinMultiModField(Hero hero)
        {
            InitializeComponent();
            this.hero = hero;
            skins = [new Skin(){ Id=hero.Id * 10 + 1, Name="Mặc định", Label="Default"}];
            skins.AddRange(hero.Skins ?? []);

            Loaded += SkinMultiModField_Loaded;
            RemoveSkinButton.Click += RemoveSkinButton_Click;
        }

        private void RemoveSkinButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveClick?.Invoke(sender, e);
        }

        private void SkinMultiModField_Loaded(object sender, RoutedEventArgs e)
        {
            OldSkinCbb.ItemsSource = skins;
            NewSkinCbb.ItemsSource = skins;
        }

        public void SetOldSkinId(int id)
        {
            int index = skins.FindIndex(skin => skin.Id == id);
            if (index == -1)
                throw new Exception($"Id {id} not found for this hero");
            OldSkinCbb.SelectedIndex = index;
        }

        public void SetNewSkinId(int id)
        {
            int index = skins.FindIndex(skin => skin.Id == id);
            if (index == -1)
                throw new Exception($"Id {id} not found for this hero");
            NewSkinCbb.SelectedIndex = index;
        }
    }
}
