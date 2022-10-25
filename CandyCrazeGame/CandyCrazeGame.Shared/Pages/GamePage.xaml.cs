using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Threading;
using Windows.Foundation;

namespace CandyCrazeGame
{
    public sealed partial class GamePage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();
        private int _markNum;

        private double _gameSpeed = 6;
        private readonly double _gameSpeedDefault = 6;
        private readonly double _gameSpeedfactor = 0.05;

        private int _powerUpCount;
        private readonly int _powerUpSpawnLimit = 1;
        private int _powerUpSpawnCounter = 800;
        private int _powerModeDurationCounter;
        private readonly int _powerModeDuration = 800;

        private double _score;
        private double _scoreCap;
        private double _difficultyMultiplier;

        private bool _isGameOver;
        private bool _isPowerMode;

        private bool _isPointerActivated;
        private Point _pointerPosition;

        private double _windowHeight, _windowWidth;
        private double _scale;

        private Player _player;
        private Rect _playerHitBox;

        private int _collectibleSpawnCounter;
        private int _collectibleSpawnLimit;
        private int _collectibleCount;
        private int _collectibleCollected;

        private Uri[] _clouds;
        private Uri[] _collectibles;

        private double _playerHealth;

        private readonly double _healthDepletePointDefault = 0.5;
        private readonly double _healthGainPointDefault = 10;

        #endregion

        #region Ctor

        public GamePage()
        {
            InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            Loaded += GamePage_Loaded;
            Unloaded += GamePage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += GamePage_SizeChanged;
        }

        private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePage_SizeChanged;
            StopGame();
        }

        private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            SetViewSize();

            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
        }

        #endregion

        #region Input

        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isGameOver)
            {
                App.EnterFullScreen(true);

                InputView.Focus(FocusState.Programmatic);
                StartGame();
            }
            else
            {
                _isPointerActivated = true;

                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;
            }
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isPointerActivated)
            {
                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;
            }
        }

        private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isPointerActivated = false;
        }

        #endregion

        #region Button

        private void QuitGameButton_Checked(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void QuitGameButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ResumeGame();
        }

        private void ConfirmQuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(StartPage));
        }

        #endregion

        #endregion

        #region Methods

        #region Game

        private void LoadGameElements()
        {
            _clouds = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.CLOUD).Select(x => x.Value).ToArray();
            _collectibles = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.COLLECTIBLE).Select(x => x.Value).ToArray();
        }

        private void PopulateGameViews()
        {
#if DEBUG
            Console.WriteLine("INITIALIZING GAME");
#endif
            SetViewSize();
            PopulateGameView();
        }

        private void PopulateGameView()
        {
            // add some clouds
            for (int i = 0; i < 15; i++)
            {
                SpawnCloud();
            }

            // add some collectibles
            for (int i = 0; i < 10; i++)
            {
                SpawnCollectible();
            }

            // add player
            _player = new Player(_scale);

            _player.SetPosition(
                left: GameView.Width / 2 - _player.Width / 2,
                top: GameView.Height / 2 - _player.Height / 2);

            _player.SetZ(1);

            GameView.Children.Add(_player);

            _playerHitBox = _player.GetHitBox();
        }

        private void StartGame()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif
            HideInGameTextMessage();
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            _collectibleSpawnLimit = 2;
            _collectibleCount = 0;

            _gameSpeed = _gameSpeedDefault;
            _player.Opacity = 1;

            ResetControls();

            _isGameOver = false;
            _isPowerMode = false;

            _powerModeDurationCounter = _powerModeDuration;
            _powerUpCount = 0;

            _score = 0;
            _scoreCap = 20;
            _difficultyMultiplier = 1;

            _collectibleCollected = 0;
            ScoreText.Text = "0";

            PlayerHealthBarPanel.Visibility = Visibility.Visible;

            _playerHealth = 100;

            foreach (GameObject x in GameView.GetGameObjects<PowerUp>())
            {
                GameView.AddDestroyableGameObject(x);
            }

            RemoveGameObjects();
            StartGameSounds();

            RunGame();

            _player.SetSize(width: Constants.PLAYER_SIZE * _scale, height: Constants.PLAYER_SIZE * _scale);
        }

        private async void RunGame()
        {
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
            {
                GameViewLoop();
            }
        }

        private void GameViewLoop()
        {
            UpdateGameObjects();
        }

        private void UpdateGameObjects()
        {
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.CLOUD:
                        {
                            UpdateCloud(x);
                        }
                        break;
                    case ElementType.COLLECTIBLE:
                        {
                            UpdateCollectible(x);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void RemoveGameObjects()
        {
            GameView.RemoveDestroyableGameObjects();
        }

        private void ResetControls()
        {
            _pointerPosition = null;
        }

        private void PauseGame()
        {
            InputView.Focus(FocusState.Programmatic);
            ShowInGameTextMessage("GAME_PAUSED");

            _gameViewTimer?.Dispose();

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            PauseGameSounds();
        }

        private void ResumeGame()
        {
            InputView.Focus(FocusState.Programmatic);
            HideInGameTextMessage();

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            SoundHelper.ResumeSound(SoundType.BACKGROUND);

            RunGame();
        }

        private void StopGame()
        {
            _gameViewTimer?.Dispose();
            StopGameSounds();
        }

        private void GameOver()
        {
            _isGameOver = true;

            PlayerScoreHelper.PlayerScore = new CandyCrazeGameScore()
            {
                Score = Math.Ceiling(_score),
                CollectiblesCollected = _collectibleCollected
            };

            SoundHelper.PlaySound(SoundType.GAME_OVER);

            //TODO: go to game over page
            //NavigateToPage(typeof(GameOverPage));
        }

        #endregion

        #region GameObject

        private void RecycleGameObjects()
        {
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.COLLECTIBLE:
                        {
                            RecyleCollectible(x);
                        }
                        break;
                    case ElementType.CLOUD:
                        {
                            RecyleCloud(x);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Cloud

        private void SpawnCloud()
        {
            Cloud cloud = new(_scale);
            GameView.Children.Add(cloud);
        }

        private void UpdateCloud(GameObject cloud)
        {
            cloud.SetTop(cloud.GetTop() + _gameSpeed);

            if (cloud.GetTop() > GameView.Height)
            {
                RecyleCloud(cloud);
            }
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
                left: _random.Next(0, (int)GameView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
        }

        #endregion

        #region Collectible

        private void SpawnCollectible()
        {
            Collectible collectible = new(_scale);
            RandomizeCollectiblePosition(collectible);

            GameView.Children.Add(collectible);
        }

        private void UpdateCollectible(GameObject collectible)
        {
            collectible.SetTop(collectible.GetTop() + _gameSpeed);

            if (collectible.GetTop() > GameView.Height)
            {
                RecyleCollectible(collectible);
            }
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
                left: _random.Next(0, (int)GameView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
        }

        #endregion

        #region Score

        private void AddScore(double score)
        {
            _score += score;
            ScaleDifficulty();
        }

        #endregion

        #region Difficulty

        private void ScaleDifficulty()
        {
            if (_score > _scoreCap)
            {
                _gameSpeed = _gameSpeedDefault + 0.2 * _difficultyMultiplier;

                _difficultyMultiplier++;
                _scoreCap += 50;
            }
        }

        #endregion

        #region Sound

        private void StartGameSounds()
        {
            SoundHelper.RandomizeSound(SoundType.BACKGROUND);
            SoundHelper.PlaySound(SoundType.BACKGROUND);
        }

        private void StopGameSounds()
        {
            SoundHelper.StopSound(SoundType.BACKGROUND);
        }

        private void PauseGameSounds()
        {
            SoundHelper.PauseSound(SoundType.BACKGROUND);
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

            GameView.Width = _windowWidth;
            GameView.Height = _windowHeight;

            if (_player is not null)
            {
                _player.SetSize(width: Constants.PLAYER_SIZE * _scale, height: Constants.PLAYER_SIZE * _scale);
            }
        }

        private void NavigateToPage(Type pageType)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region In Game Message

        private void ShowInGameTextMessage(string resourceKey)
        {
            InGameMessageText.Text = LocalizationHelper.GetLocalizedResource(resourceKey);
            InGameMessagePanel.Visibility = Visibility.Visible;
        }

        private void HideInGameTextMessage()
        {
            InGameMessageText.Text = "";
            InGameMessagePanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion
    }
}
