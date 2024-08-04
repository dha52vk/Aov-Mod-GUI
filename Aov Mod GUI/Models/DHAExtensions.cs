using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;

namespace Aov_Mod_GUI.Models
{
    internal static class DHAExtensions
    {
        public static T? GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T? child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static void PlayStoryBoard(this Window w,string storyboardKey)
        {
            Storyboard? sb = w.FindResource(storyboardKey) as Storyboard;
            if (sb != null)
                sb.Begin();
        }
    }
}
