using AovClass.Models;
using Newtonsoft.Json.Linq;
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
    /// Interaction logic for HeroMultiModField.xaml
    /// </summary>
    public partial class HeroMultiModField : UserControl
    {
        public Hero hero { get; private set; }
        public List<KeyValuePair<Skin, Skin>> SkinChanges
        {
            get
            {
                List<KeyValuePair<Skin, Skin>> res = [];
                foreach (SkinMultiModField field in SkinFieldContainer.Children)
                {
                    if (field.OldSkin != null && field.NewSkin != null)
                        res.Add(KeyValuePair.Create(field.OldSkin, field.NewSkin));
                }
                return res;
            }
        }
        public List<KeyValuePair<int, int>> SkinIdChanges
        {
            get
            {
                List<KeyValuePair<int, int>> res = [];
                foreach (SkinMultiModField field in SkinFieldContainer.Children)
                {
                    if (field.OldSkin != null && field.NewSkin != null)
                        res.Add(KeyValuePair.Create(field.OldSkin.Id, field.NewSkin.Id));
                }
                return res;
            }
        }
        public string HeroName { get => hero.Name; }

        public RoutedEventHandler? RemoveClick;

        public HeroMultiModField(Hero hero)
        {
            InitializeComponent();
            DataContext = this;
            this.hero = hero;

            Loaded += HeroMultiModField_Loaded;
            AddSkinButton.Click += AddSkinButton_Click;
            RemoveHeroButton.Click += RemoveHeroButton_Click;
        }

        public void TrySetSkinIdPair(List<KeyValuePair<int,int>> skinPairs)
        {
            SkinFieldContainer.Children.Clear();
            foreach (var pair in skinPairs)
            {
                var field = new SkinMultiModField(hero);
                field.RemoveClick = (sender, e) =>
                {
                    SkinFieldContainer.Children.Remove(field);
                };
                field.SetOldSkinId(pair.Key);
                field.SetNewSkinId(pair.Value);
                SkinFieldContainer.Children.Add(field);
            }
        }

        private void RemoveHeroButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveClick?.Invoke(sender, e);
        }

        private void AddSkinButton_Click(object sender, RoutedEventArgs e)
        {
            var field = new SkinMultiModField(hero);
            field.RemoveClick = (sender, e) =>
            {
                SkinFieldContainer.Children.Remove(field);
            };
            SkinFieldContainer.Children.Add(field);
        }

        private void HeroMultiModField_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
