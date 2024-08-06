using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;
using System.IO;

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

        public static void PlayStoryBoard(this Window w, string storyboardKey)
        {
            Storyboard? sb = w.FindResource(storyboardKey) as Storyboard;
            if (sb != null)
                sb.Begin();
        }
    }

    internal static class LogExtension
    {
        private static bool FirstLog = true;
        public static void Log(string logMessage)
        {
            if (FirstLog)
            {
                if (File.Exists("log.txt"))
                    File.Delete("log.txt");
                FirstLog = false;
            }
            //string logFilePath = "log.txt";
            //if (!File.Exists(logFilePath))
            //    File.Create(logFilePath);
            File.AppendAllText("log.txt", DateTime.Now.ToString("[MM/dd/yyyy HH:mm:ss]") + "   " + logMessage.TrimStart().TrimEnd() + "\n");
        }
    }

    internal static class PathExtension
    {
        /// <summary>
        /// Combine paths to one and remove if end of this path equal start of another
        /// </summary>
        /// <returns>Path after combine</returns>
        public static string Combine(params string[] paths)
        {
            string path = paths[0];
            for (int i = 1; i < paths.Length - 1; i++)
            {
                path = Combine(path, paths[i + 1]);
            }
            return path;
        }

        public static string Combine(string str1, string str2)
        {
            str1 = str1.TrimEnd('/').TrimEnd('\\');
            str2 = str2.TrimStart('/').TrimStart('\\');
            int maxLength = Math.Min(str1.Length, str2.Length);

            // Iterate to find the longest matching substring
            for (int i = maxLength; i > 0; i--)
            {
                if (str1.EndsWith(str2.Substring(0, i)))
                {
                    str2 = str2[i..];
                    break;
                }
            }

            // If no matching part, return the strings concatenated
            return Path.Combine(str1, str2);
        }
    }
}
