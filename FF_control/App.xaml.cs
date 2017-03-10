using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FF_control
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public enum Themes { Dark_Theme, Light_Theme };
        public enum Languages { English, German };

        public Uri[] themes_uri = new Uri[] 
        {
            new Uri(@"/Visual/Themes/Theme_black.xaml", UriKind.Relative), 
            new Uri(@"/Visual/Themes/Theme_white.xaml", UriKind.Relative) 
        };
        public Uri[] languages_uri = new Uri[] 
        {
            new Uri(@"/Languages/Strings.xaml", UriKind.Relative), 
            new Uri(@"/Languages/Strings_de.xaml", UriKind.Relative) 
        };

        public App()
        {
            Theme_used = Themes.Light_Theme;
            Language_used = Languages.English;
        }

        public Themes Theme_used { get; set; }
        public Languages Language_used { get; set; }

        // Place in App.xaml.cs
        public void ChangeDynamicResources()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            ResourceDictionary resourceDict = Application.LoadComponent(themes_uri[(int)Theme_used]) as ResourceDictionary;
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            resourceDict = Application.LoadComponent(languages_uri[(int)Language_used]) as ResourceDictionary;
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);

        }

        // Example Usage (anywhere in app)
        public void ChangeToDarkTheme()
        {
            Theme_used = Themes.Dark_Theme;
            ChangeDynamicResources();
        }
        // Example Usage (anywhere in app)
        public void ChangeToLightTheme()
        {
            Theme_used = Themes.Light_Theme;
            ChangeDynamicResources();
        }
    }
}
