using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CandyCrazeGame
{
    public sealed partial class HowToPlayPage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private double _windowHeight, _windowWidth;
        private double _scale;

        private readonly int _gameSpeed = 8;

        private int _markNum;

        private Uri[] _clouds;
        private Uri[] _collectibles;

        private readonly IBackendService _backendService;

        #endregion

        #region Ctor

        public HowToPlayPage()
        {
            this.InitializeComponent();
            _backendService = (Application.Current as App).Host.Services.GetRequiredService<IBackendService>();

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            this.Loaded += HowToPlayPage_Loaded;
            this.Unloaded += HowToPlayPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void HowToPlayPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetLocalization();

            SizeChanged += GamePlayPage_SizeChanged;
            StartAnimation();
        }

        private void HowToPlayPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePlayPage_SizeChanged;
            StopAnimation();
        }

        private void GamePlayPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            SetViewSize();

#if DEBUG
            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
#endif
        }

        #endregion

        #region Buttons

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameProfileHelper.HasUserLoggedIn() ? await GenerateSession() : true)
                NavigateToPage(typeof(GamePlayPage));
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsCount = InstructionsContainer.Items.Count - 1;

            // once the last instruction is reached, make the start game button visible and hide the next button
            if (InstructionsContainer.SelectedIndex == itemsCount)
            {
                // traverse back to first instruction
                for (int i = 0; i < itemsCount; i++)
                {
                    InstructionsContainer.SelectedIndex--;
                }

                NextButton.Visibility = Visibility.Collapsed;
                PlayButton.Visibility = Visibility.Visible;
            }
            else
            {
                InstructionsContainer.SelectedIndex++;
            }

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(StartPage));
        }

        #endregion

        #endregion

        #region Methods

        #region Logic

        private async Task<bool> GenerateSession()
        {
            (bool IsSuccess, string Message) = await _backendService.GenerateUserSession();

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

            UnderView.Width = _windowWidth;
            UnderView.Height = _windowHeight;
        }

        private void NavigateToPage(Type pageType)
        {
            if (pageType == typeof(GamePlayPage))
                SoundHelper.StopSound(SoundType.INTRO);

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        private void SetLocalization()
        {
            PageExtensions.SetLocalization(this);

            LocalizationHelper.SetLocalizedResource(PlayerInstructionsHeader);
            LocalizationHelper.SetLocalizedResource(PlayerInstructionsDetail);

            LocalizationHelper.SetLocalizedResource(CollectiblesInstructionsHeader);
            LocalizationHelper.SetLocalizedResource(CollectiblesInstructionsDetail);

            LocalizationHelper.SetLocalizedResource(PowerUpsInstructionsHeader);
            LocalizationHelper.SetLocalizedResource(PowerUpsInstructionsDetail);

            LocalizationHelper.SetLocalizedResource(HealthsInstructionsHeader);
            LocalizationHelper.SetLocalizedResource(HealthsInstructionsDetail);
        }

        #endregion

        #region Animation

        #region Game

        private void PopulateGameViews()
        {
#if DEBUG
            Console.WriteLine("INITIALIZING GAME");
#endif
            SetViewSize();
            PopulateUnderView();
        }

        private void LoadGameElements()
        {
            _clouds = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.CLOUD).Select(x => x.Value).ToArray();
            _collectibles = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.COLLECTIBLE).Select(x => x.Value).ToArray();
        }

        private void PopulateUnderView()
        {
            // add some clouds
            for (int i = 0; i < 15; i++)
                SpawnCloud();

            // add some collectibles
            for (int i = 0; i < 10; i++)
                SpawnCollectible();
        }

        private void StartAnimation()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif      
            RecycleGameObjects();
            RunGame();
        }

        private void RecycleGameObjects()
        {
            foreach (GameObject x in UnderView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.COLLECTIBLE:
                        RecyleCollectible(x);
                        break;
                    case ElementType.CLOUD:
                        RecyleCloud(x);
                        break;
                    default:
                        break;
                }
            }
        }

        private async void RunGame()
        {
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
                GameViewLoop();
        }

        private void GameViewLoop()
        {
            UpdateGameObjects();
        }

        private void UpdateGameObjects()
        {
            foreach (GameObject x in UnderView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.CLOUD:
                        UpdateCloud(x);
                        break;
                    case ElementType.COLLECTIBLE:
                        UpdateCollectible(x);
                        break;
                    default:
                        break;
                }
            }
        }

        private void StopAnimation()
        {
            _gameViewTimer?.Dispose();
        }

        #endregion

        #region Cloud

        private void SpawnCloud()
        {
            Cloud cloud = new(_scale);
            UnderView.Children.Add(cloud);
        }

        private void UpdateCloud(GameObject cloud)
        {
            cloud.SetTop(cloud.GetTop() + _gameSpeed);

            if (cloud.GetTop() > UnderView.Height)
                RecyleCloud(cloud);
        }

        private void RecyleCloud(GameObject cloud)
        {
            _markNum = _random.Next(0, _clouds.Length);
            cloud.SetContent(_clouds[_markNum]);
            RandomizeCloudPosition(cloud);
        }

        private void RandomizeCloudPosition(GameObject cloud)
        {
            cloud.SetPosition(
                left: _random.Next(0, (int)UnderView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)UnderView.Height) * -1);
        }

        #endregion

        #region Collectible

        private void SpawnCollectible()
        {
            Collectible collectible = new(_scale);
            RandomizeCollectiblePosition(collectible);

            UnderView.Children.Add(collectible);
        }

        private void UpdateCollectible(GameObject collectible)
        {
            collectible.SetTop(collectible.GetTop() + _gameSpeed);

            if (collectible.GetTop() > UnderView.Height)
                RecyleCollectible(collectible);
        }

        private void RecyleCollectible(GameObject collectible)
        {
            _markNum = _random.Next(0, _collectibles.Length);
            collectible.SetContent(_collectibles[_markNum]);
            RandomizeCollectiblePosition(collectible);
        }

        private void RandomizeCollectiblePosition(GameObject collectible)
        {
            collectible.SetPosition(
                left: _random.Next(0, (int)UnderView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)UnderView.Height) * -1);
        }

        #endregion

        #region Sound

        private void StartGameSounds()
        {
            SoundHelper.RandomizeSound(SoundType.INTRO);
            SoundHelper.PlaySound(SoundType.INTRO);
        }

        #endregion        

        #endregion

        #endregion
    }
}
