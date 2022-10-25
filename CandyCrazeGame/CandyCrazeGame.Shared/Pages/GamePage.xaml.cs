using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Runtime.InteropServices;
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

        private int _cloudCount;
        private readonly int _cloudSpawnLimit = 15;
        private int _cloudSpawnCounter;
        private readonly int _cloudSpawnCounterDefault = 20;

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

        private int _collectibleCollected;

        private Uri[] _clouds;
        private Uri[] _collectibles;

        private double _playerHealth;

        private int _jumpDurationCounter;
        private readonly int _jumpDurationCounterDefault = 40;

        private int _idleDurationCounter;
        private readonly int _idleDurationCounterDefault = 20;

        private double _jumpingEaseDurationCounter;
        private readonly double _jumpEaseDurationCounterDefault = 5;

        private double _fallingEaseDurationCounter = 0;

        private GameObject _landedCloud;

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

        #region Animation

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
            SpawnCloud();

            _landedCloud = GameView.Children.OfType<Cloud>().First();

            // add some collectibles
            for (int i = 0; i < 10; i++)
            {
                SpawnCollectible();
            }

            // add player
            _player = new Player(_scale);

            _player.SetPosition(
                left: (_landedCloud.GetLeft() + _landedCloud.Width / 2) - _player.Width / 2,
                top: (_landedCloud.GetTop() + _landedCloud.Height / 2) - _player.Height / 2);

            _player.SetZ(1);
            GameView.Children.Add(_player);
        }

        private void StartGame()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif
            HideInGameTextMessage();
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

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

            _jumpDurationCounter = _jumpDurationCounterDefault;
            _idleDurationCounter = _idleDurationCounterDefault;
            _jumpingEaseDurationCounter = _jumpEaseDurationCounterDefault;

            _cloudSpawnCounter = _cloudSpawnCounterDefault;

            foreach (GameObject x in GameView.GetGameObjects<PowerUp>())
            {
                GameView.AddDestroyableGameObject(x);
            }

            RecycleGameObjects();
            RemoveGameObjects();
            StartGameSounds();

            RunGame();

            _player.SetSize(
                width: Constants.PLAYER_WIDTH * _scale,
                height: Constants.PLAYER_HEIGHT * _scale);
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
            ScoreText.Text = _score.ToString("#");
            PlayerHealthBar.Value = _playerHealth;

            _playerHitBox = _player.GetStandingHitBox();

            SpawnGameObjects();
            UpdateGameObjects();

            RemoveGameObjects();

            if (_isPowerMode)
            {
                PowerUpCoolDown();
                if (_powerModeDurationCounter <= 0)
                    PowerDown();
            }

            GameElementsCount.Text = GameView.Children.Count().ToString();
        }

        private void SpawnGameObjects()
        {
            if (_powerUpCount < _powerUpSpawnLimit)
            {
                _powerUpSpawnCounter--;

                if (_powerUpSpawnCounter < 1)
                {
                    SpawnPowerUp();
                    _powerUpSpawnCounter = _random.Next(1000, 1200);
                }
            }

            if (_cloudCount < _cloudSpawnLimit)
            {
                _cloudSpawnCounter--;

                if (_cloudSpawnCounter < 1)
                {
                    SpawnCloud();
                    _cloudSpawnCounter = _cloudSpawnCounterDefault;
                }
            }
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
                    case ElementType.POWERUP:
                        {
                            UpdatePowerUp(x);
                        }
                        break;
                    case ElementType.PLAYER:
                        {
                            UpdatePlayer();
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
                    //case ElementType.CLOUD:
                    //    {
                    //        RecyleCloud(x);
                    //    }
                    //    break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Player

        private void UpdatePlayer()
        {
            switch (_player.PlayerState)
            {
                case PlayerState.Idle:
                    {
                        _player.SetTop(_player.GetTop() + _landedCloud.Speed);

                        if (_playerHitBox.Top > _windowHeight / 4)
                        {
                            _fallingEaseDurationCounter = 0;
                            _idleDurationCounter--;

                            if (_idleDurationCounter <= 0)
                            {
                                SoundHelper.PlaySound(SoundType.JUMP);
                                _player.SetState(PlayerState.Jumping);
                                _idleDurationCounter = _idleDurationCounterDefault;
                            }
                        }

                    }
                    break;
                case PlayerState.Jumping:
                    {
                        //TODO: change state to jumping and count cool down

                        _jumpDurationCounter--;

                        if (_playerHitBox.Top > 0)
                        {
                            if (_jumpingEaseDurationCounter > 0)
                                _jumpingEaseDurationCounter -= 0.1;

                            MovePlayerY(MovementDirectionY.Up);
                        }

                        // move left
                        if (_pointerPosition.X < _playerHitBox.Left)
                        {
                            _player.SetJumpDirection(MovementDirectionX.Left);
                            MovePlayerX(MovementDirectionX.Left);
                        }

                        // move right
                        if (_pointerPosition.X > _playerHitBox.Right)
                        {
                            _player.SetJumpDirection(MovementDirectionX.Right);
                            MovePlayerX(MovementDirectionX.Right);
                        }

                        if (_jumpDurationCounter <= 0)
                        {
                            _jumpingEaseDurationCounter = _jumpEaseDurationCounterDefault;
                            _jumpDurationCounter = _jumpDurationCounterDefault;
                            _player.SetState(PlayerState.Falling);
                        }

                    }
                    break;
                case PlayerState.Falling:
                    {
                        //TODO: change state to falling and land on a cloud

                        _fallingEaseDurationCounter += 0.1;
                        MovePlayerY(MovementDirectionY.Down);

                        if (_pointerPosition.X < _playerHitBox.Left)
                            MovePlayerX(MovementDirectionX.Left);

                        if (_pointerPosition.X > _playerHitBox.Right)
                            MovePlayerX(MovementDirectionX.Right);

                        if (_playerHitBox.Top > _windowHeight)
                        {
                            //TODO: loose health
                            //TODO: respawn
                            //TODO: animate health loss

                            _fallingEaseDurationCounter = 0;
                            _idleDurationCounter = _idleDurationCounterDefault;

                            _player.SetState(PlayerState.Falling);
                            _player.SetPosition(
                                  left: GameView.Width / 2 - _player.Width / 2,
                                  top: 0 + _player.Height / 2);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void MovePlayerY(MovementDirectionY movementDirectionY)
        {
            switch (movementDirectionY)
            {
                case MovementDirectionY.Up:
                    _player.SetTop(_player.GetTop() - ((_gameSpeed / 3) + _jumpingEaseDurationCounter));
                    break;
                case MovementDirectionY.Down:
                    _player.SetTop(_player.GetTop() + ((_gameSpeed / 3) + _fallingEaseDurationCounter));
                    break;
                default:
                    break;
            }
        }

        private void MovePlayerX(MovementDirectionX movementDirectionX)
        {
            switch (movementDirectionX)
            {
                case MovementDirectionX.Left:
                    _player.SetLeft(_player.GetLeft() - _gameSpeed * 1.1);
                    break;
                case MovementDirectionX.Right:
                    _player.SetLeft(_player.GetLeft() + _gameSpeed * 1.1);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Cloud

        private void SpawnCloud()
        {
            Cloud cloud = new(_scale)
            {
                Speed = _gameSpeed
            };

            RecyleCloud(cloud);
            GameView.Children.Add(cloud);
            _cloudCount++;
        }

        private void UpdateCloud(GameObject cloud)
        {
            cloud.SetTop(cloud.GetTop() + cloud.Speed);

            var cloudHitBox = cloud.GetPlatformHitBox();

            // only land on a cloud if it's almost in the middle
            if (_playerHitBox.Top > _windowHeight / 3)
            {
                if (_player.PlayerState == PlayerState.Falling && _playerHitBox.IntersectsWith(cloudHitBox))
                {
                    _landedCloud = cloud;
                    _idleDurationCounter = _idleDurationCounterDefault;
                    _player.SetState(PlayerState.Idle);
                }
            }

            if (cloud.GetTop() > GameView.Height)
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
                left: _random.Next((int)(50 * _scale), (int)(GameView.Width - (50 * _scale))),
                //top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
                top: (int)GameView.Height / 4 * -1);
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

            if (_playerHitBox.IntersectsWith(collectible.GetHitBox()))
                Collectible(collectible);
            else
            {
                // in power mode draw the collectible closer
                if (_isPowerMode)
                    MagnetPull(collectible);

                if (collectible.GetTop() > GameView.Height)
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

        private void MagnetPull(GameObject collectible)
        {
            var playerHitBoxDistant = _player.GetDistantHitBox();
            var collectibleHitBoxDistant = collectible.GetDistantHitBox();

            if (playerHitBoxDistant.IntersectsWith(collectibleHitBoxDistant))
            {
                var collectibleHitBox = collectible.GetHitBox();

                if (_playerHitBox.Left < collectibleHitBox.Left)
                    collectible.SetLeft(collectible.GetLeft() - _gameSpeed * 1.5);

                if (collectibleHitBox.Right < _playerHitBox.Left)
                    collectible.SetLeft(collectible.GetLeft() + _gameSpeed * 1.5);

                if (collectibleHitBox.Top > _playerHitBox.Bottom)
                    collectible.SetTop(collectible.GetTop() - _gameSpeed * 1.5);

                if (collectibleHitBox.Bottom < _playerHitBox.Top)
                    collectible.SetTop(collectible.GetTop() + _gameSpeed * 1.5);
            }
        }

        private void Collectible(GameObject collectible)
        {
            SoundHelper.PlayRandomSound(SoundType.COLLECTIBLE);

            AddScore(5);
            RecyleCollectible(collectible);

            _collectibleCollected++;
        }

        #endregion

        #region PowerUp

        private void SpawnPowerUp()
        {
            PowerUp powerUp = new(_scale);
            RandomizePowerUpPosition(powerUp);
            GameView.Children.Add(powerUp);
        }

        private void RandomizePowerUpPosition(GameObject powerUp)
        {
            powerUp.SetPosition(
                left: _random.Next(0, (int)GameView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
        }

        private void UpdatePowerUp(GameObject powerUp)
        {
            powerUp.SetTop(powerUp.GetTop() + _gameSpeed);

            if (_playerHitBox.IntersectsWith(powerUp.GetHitBox()))
            {
                GameView.AddDestroyableGameObject(powerUp);
                PowerUp(powerUp);
            }
            else
            {
                if (powerUp.GetTop() > GameView.Height)
                    GameView.AddDestroyableGameObject(powerUp);
            }
        }

        private void PowerUp(GameObject powerUp)
        {
            _isPowerMode = true;
            _powerModeDurationCounter = _powerModeDuration;
            _powerUpCount++;

            powerUpText.Visibility = Visibility.Visible;
            SoundHelper.PlaySound(SoundType.POWER_UP);
        }

        private void PowerUpCoolDown()
        {
            _powerModeDurationCounter -= 1;
            double remainingPow = (double)_powerModeDurationCounter / (double)_powerModeDuration * 4;

            powerUpText.Text = "";

            for (int i = 0; i < remainingPow; i++)
            {
                powerUpText.Text += "⚡";
            }
        }

        private void PowerDown()
        {
            _isPowerMode = false;
            _powerUpCount--;

            powerUpText.Visibility = Visibility.Collapsed;
            SoundHelper.PlaySound(SoundType.POWER_DOWN);
        }

        #endregion

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
                _player.SetSize(
                    width: Constants.PLAYER_WIDTH * _scale,
                    height: Constants.PLAYER_HEIGHT * _scale);
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
