﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace CandyCrazeGame
{
    public static class AssetHelper
    {
        #region Fields

        private static string _baseUrl;

        private static bool _assetsPreloaded;

        #endregion

        #region Methods

        public static string GetBaseUrl()
        {
            if (_baseUrl.IsNullOrBlank())
            {
                var indexUrl = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.href;");
                var appPackageId = Environment.GetEnvironmentVariable("UNO_BOOTSTRAP_APP_BASE");
                _baseUrl = $"{indexUrl}{appPackageId}";

#if DEBUG
                Console.WriteLine(_baseUrl);
#endif 
            }
            return _baseUrl;
        }

        public static async void PreloadAssets(ProgressBar progressBar, TextBlock messageBlock)
        {
            if (!_assetsPreloaded)
            {
                progressBar.IsIndeterminate = false;
                progressBar.ShowPaused = false;
                progressBar.Value = 0;
                progressBar.Minimum = 0;
                progressBar.Maximum = Constants.ELEMENT_TEMPLATES.Length;

                messageBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                messageBlock.Foreground = App.Current.Resources["ProgressBarOkColor"] as SolidColorBrush;
                messageBlock.Text = LocalizationHelper.GetLocalizedResource("LOADING_GAME_ASSETS");

                foreach (var uri in Constants.ELEMENT_TEMPLATES.Select(x => x.Value).ToArray())
                {
                    await GetFileAsync(uri, progressBar);
                }

                _assetsPreloaded = true;

                messageBlock.Text = string.Empty;
                messageBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
        }

        private static async Task GetFileAsync(Uri uri, ProgressBar progressBar)
        {
            await StorageFile.GetFileFromApplicationUriAsync(uri);
            progressBar.Value++;
        }

        #endregion
    }
}
