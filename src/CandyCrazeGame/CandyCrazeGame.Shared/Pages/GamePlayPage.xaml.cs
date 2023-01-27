using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.System;

namespace CandyCrazeGame
{
    public sealed partial class GamePlayPage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();
        private int _markNum;

        private double _gameSpeed;
        private readonly double _gameSpeedDefault = 5;

        private int _cloudCount;
        private readonly int _cloudSpawnLimit = 15;

        private double _cloudSpawnCounter;
        private readonly double _cloudSpawnCounterDefault = 60;

        private readonly int _cloudMovementDirectionXSpeedDivider = 8;
        private readonly double _cloudSpeedFactor = 0.8;

        private int _landedCloudEffectCounter;
        private readonly int _landedCloudEffectCounterDefault = 5;

        private bool _landedCloudEffectActive;
        private readonly double _cloudFlightPlayerScaleModifier = 1.3;

        private bool _landedCloudEffectReverseActive;
        private int _landedCloudEffectReverseCounter;
        private readonly int _landedCloudEffectReverseCounterDefault = 5;

        private int _powerUpCount;
        private readonly int _powerUpSpawnLimit = 1;

        private int _powerUpSpawnCounter = 600;

        private int _powerModeDurationCounter;
        private readonly int _powerModeDuration = 1000;

        private double _enemySpawnCounter;
        private readonly double _enemySpawnCounterDefault = 240;

        private int _enemyCount;
        private readonly int _enemySpawnLimit = 3;

        private double _score;
        private double _scoreCap;
        private double _difficultyMultiplier;

        private bool _moveLeft;
        private bool _moveRight;
        private bool _moveUp;
        private bool _moveDown;

        private bool _isGameOver;
        private bool _isPowerMode;

        private bool _isPointerActivated;
        private Point _pointerPosition;

        private double _windowHeight, _windowWidth;
        private double _scale;

        private Player _player;
        private Rect _playerHitBox;
        private Rect _playerStandingHitBox;
        private Rect _playerPlatformHitBox;
        private Rect _playerDistantHitBox;

        private int _collectibleCollected;

        private Uri[] _clouds;
        private Uri[] _collectibles;
        private Uri[] _powerUps;
        private Uri[] _enemies;

        private PowerUpType _powerUpType;

        private double _playerHealth;
        private int _playerHealthLossPoints;

        private int _damageRecoveryOpacityFrameSkip;
        private int _damageRecoveryCounter = 300;
        private readonly int _damageRecoveryDelay = 300;

        private readonly double _playerMovementSpeedMultiplier = 1.2;

        private bool _isPlayerRecoveringFromDamage;

        private double _jumpDurationCounter;
        private readonly double _jumpDurationCounterDefault = 40;

        private int _idleDurationCounter;
        private readonly int _idleDurationCounterDefault = 20;

        private double _jumpingEaseCounter;
        private readonly double _jumpEaseCounterDefault = 5;

        private double _fallingEaseCounter = 0;
        private readonly double _fallingEaseCounterDefault = 0;

        #endregion

        #region Ctor

        public GamePlayPage()
        {
            InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            Loaded += GamePlayPage_Loaded;
            Unloaded += GamePlayPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void GamePlayPage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += GamePlayPage_SizeChanged;
        }

        private void GamePlayPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePlayPage_SizeChanged;
            StopGame();
        }

        private void GamePlayPage_SizeChanged(object sender, SizeChangedEventArgs args)
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

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left)
            {
                _moveLeft = true;
                _moveRight = false;
            }
            if (e.Key == VirtualKey.Right)
            {
                _moveRight = true;
                _moveLeft = false;
            }
            if (e.Key == VirtualKey.Up)
            {
                _moveUp = true;
                _moveDown = false;
            }
            if (e.Key == VirtualKey.Down)
            {
                _moveDown = true;
                _moveUp = false;
            }
        }

        private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        {
            // when the player releases the left or right key it will set the designated boolean to false
            if (e.Key == VirtualKey.Left)
            {
                _moveLeft = false;
            }
            if (e.Key == VirtualKey.Right)
            {
                _moveRight = false;
            }
            if (e.Key == VirtualKey.Up)
            {
                _moveUp = false;
            }
            if (e.Key == VirtualKey.Down)
            {
                _moveDown = false;
            }
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
            GameOver();
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
            _powerUps = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.POWERUP).Select(x => x.Value).ToArray();
            _enemies = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.ENEMY).Select(x => x.Value).ToArray();
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
            // add some collectibles
            for (int i = 0; i < 10; i++)
            {
                SpawnCollectible();
            }
        }

        private void StartGame()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif
            HideInGameTextMessage();
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            _gameSpeed = _gameSpeedDefault * _scale;

            ResetControls();

            _isGameOver = false;
            _isPowerMode = false;

            _powerModeDurationCounter = _powerModeDuration;
            _powerUpCount = 0;

            _score = 0;
            _scoreCap = 50;
            _difficultyMultiplier = 1;

            _collectibleCollected = 0;
            ScoreText.Text = "0";

            _playerHealth = 100;
            _playerHealthLossPoints = 20;

            PlayerHealthBarPanel.Visibility = Visibility.Visible;

            _jumpDurationCounter = _jumpDurationCounterDefault;
            _idleDurationCounter = _idleDurationCounterDefault;

            _jumpingEaseCounter = _jumpEaseCounterDefault;
            _fallingEaseCounter = _fallingEaseCounterDefault;

            _cloudSpawnCounter = _cloudSpawnCounterDefault;
            _enemySpawnCounter = _enemySpawnCounterDefault;

            foreach (GameObject x in GameView.GetGameObjects<PowerUp>())
            {
                GameView.AddDestroyableGameObject(x);
            }

            RecycleGameObjects();
            RemoveGameObjects();
            StartGameSounds();

            for (int i = 0; i < 6; i++)
            {
                SpawnCloud(multiplierY: i);
            }

            SpawnPlayer(GameView.GetGameObjects<Cloud>().First());

            RunGame();
#if DEBUG
            Console.WriteLine($"GAME SPEED: {_gameSpeed}");
#endif
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

            _playerHitBox = _player.GetHitBox();
            _playerStandingHitBox = _player.GetStandingHitBox(_scale);
            _playerPlatformHitBox = _player.GetPlatformHitBox(_scale);
            _playerDistantHitBox = _player.GetDistantHitBox();

            SpawnGameObjects();
            UpdateGameObjects();
            RemoveGameObjects();

            if (_isPowerMode)
            {
                PowerUpCoolDown();
                if (_powerModeDurationCounter <= 0)
                    PowerDown();
            }
#if DEBUG
            GameElementsCount.Text = GameView.Children.Count.ToString();
#endif
        }

        private void ResetControls()
        {
            _moveLeft = false;
            _moveRight = false;
            _moveUp = false;
            _moveDown = false;
            _isPointerActivated = false;
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

            NavigateToPage(typeof(GameOverPage));
        }

        #endregion

        #region GameObject

        private void SpawnGameObjects()
        {
            if (_powerUpCount < _powerUpSpawnLimit)
            {
                _powerUpSpawnCounter--;

                if (_powerUpSpawnCounter < 1)
                {
                    SpawnPowerUp();
                    _powerUpSpawnCounter = _random.Next(600, 1000);
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

            if (_enemyCount < _enemySpawnLimit)
            {
                _enemySpawnCounter--;

                if (_enemySpawnCounter < 1)
                {
                    SpawnEnemy(GameView.GetGameObjects<Cloud>().Last());
                    _enemySpawnCounter = _enemySpawnCounterDefault;
                }
            }
        }

        private void UpdateGameObjects()
        {
            // do a bounce effect for the cloud on which the player just landed
            if (_landedCloudEffectActive)
                LandedCloudEffect();

            foreach (GameObject x in GameView.GetGameObjects<GameObject>())
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
                    case ElementType.ENEMY:
                        {
                            UpdateEnemy(x as Enemy);
                        }
                        break;
                    default:
                        break;
                }
            }

            // do a bounce back effect for the cloud for which bounce effect was active
            if (_landedCloudEffectReverseActive)
                LandedCloudEffectReverse();
        }

        private void RemoveGameObjects()
        {
            GameView.RemoveDestroyableGameObjects();
        }

        private void RecycleGameObjects()
        {
            foreach (GameObject x in GameView.GetGameObjects<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.COLLECTIBLE:
                        {
                            RecyleCollectible(x);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Player

        private void SpawnPlayer(Cloud cloud)
        {
            _player = new Player(_scale)
            {
                Opacity = 1
            };

            // place the player on a cloud
            _player.PlaceOnCloud(cloud);
            _player.SetZ(1);

            GameView.Children.Add(_player);
        }

        private void ReSpawnPlayer()
        {
            // if fell outside the viewport, respawn from the top middle point
            _fallingEaseCounter = _fallingEaseCounterDefault;
            _idleDurationCounter = _idleDurationCounterDefault;

            if (GameView.GetGameObjects<Cloud>().FirstOrDefault(x => x.GetTop() < 0) is Cloud cloud)
            {
                _player.SetState(PlayerState.Idle);
                _player.PlaceOnCloud(cloud);
            }
            else
            {
                _player.SetState(PlayerState.Falling);
                _player.SetPosition(
                      left: GameView.Width / 2 - _player.Width / 2,
                      top: _player.Height / 2);
            }
        }

        private void UpdatePlayer()
        {
            // animate damange recovery
            if (_isPlayerRecoveringFromDamage)
                DamageRecovering();

            // if rocket power up enabled move in any direction with pointer
            if (_isPowerMode && _powerUpType == PowerUpType.CloudRide)
            {
                CloudRide();
            }
            else
            {
                switch (_player.PlayerState)
                {
                    case PlayerState.Idle:
                        {
                            PlayerIdle();
                        }
                        break;
                    case PlayerState.Jumping:
                        {
                            PlayerJumping();
                        }
                        break;
                    case PlayerState.Falling:
                        {
                            PlayerFalling();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void PlayerIdle()
        {
            _player.SetTop(_player.GetTop() + _player.LandedCloud.Speed);

            // only move the player with cloud x axis when in view port
            if (_player.LandedCloud.GetTop() + _player.LandedCloud.Height > 10)
            {
                var inCloud = _player.LandedCloud as Cloud;

                switch (inCloud.MovementDirectionX)
                {
                    case MovementDirectionX.Left:
                        _player.SetLeft(_player.GetLeft() - _player.LandedCloud.Speed / _cloudMovementDirectionXSpeedDivider);
                        break;
                    case MovementDirectionX.Right:
                        _player.SetLeft(_player.GetLeft() + _player.LandedCloud.Speed / _cloudMovementDirectionXSpeedDivider);
                        break;
                    default:
                        break;
                }
            }

            // only initiate jump sequence after reaching a certain height
            if (_playerHitBox.Top > _windowHeight / 4)
            {
                _fallingEaseCounter = _fallingEaseCounterDefault;
                _idleDurationCounter--;

                if (_idleDurationCounter <= 0)
                {
                    SoundHelper.PlaySound(SoundType.PLAYER_JUMP);
                    _player.SetState(PlayerState.Jumping);
                    _idleDurationCounter = _idleDurationCounterDefault;
                }
            }
        }

        private void PlayerJumping()
        {
            _jumpDurationCounter--;

            // move up auto
            if (_playerHitBox.Top > 0)
                MovePlayerY(MovementDirectionY.Up);

            // move left
            if (ShouldMoveleft())
                MovePlayerX(MovementDirectionX.Left);

            // move right
            if (ShouldMoveRight())
                MovePlayerX(MovementDirectionX.Right);

            // if any cloud is detected which is up from the player and is not the landed and has a little distance from player
            var nearbyCloudDetected = GameView.GetGameObjects<Cloud>().
                Any(c => c.Uid != _player.LandedCloud.Uid
                    && c.GetPlatformHitBox(_scale) is Rect cloudHitBox
                    && cloudHitBox.Top < _playerPlatformHitBox.Top
                    && cloudHitBox.IntersectsWith(_playerStandingHitBox));

            // initiate falling sequence after staying airborne for a limited time or if a nearby cloud is detected
            if (_jumpDurationCounter <= 0 || nearbyCloudDetected)
            {
                _jumpingEaseCounter = _jumpEaseCounterDefault;
                _jumpDurationCounter = _jumpDurationCounterDefault;

                _player.SetState(PlayerState.Falling);
            }
        }

        private void PlayerFalling()
        {
            // move down auto
            MovePlayerY(MovementDirectionY.Down);

            // move left
            if (ShouldMoveleft())
                MovePlayerX(MovementDirectionX.Left);

            // move right
            if (ShouldMoveRight())
                MovePlayerX(MovementDirectionX.Right);

            // loose health if player falls outside bottom of viewport
            if (_playerHitBox.Top > _windowHeight)
            {
                if (!_isPlayerRecoveringFromDamage)
                    LooseHealth();

                ReSpawnPlayer();
            }
        }

        private bool ShouldMoveleft()
        {
            return _moveLeft || _isPointerActivated && _pointerPosition.X < _playerHitBox.Left;
        }

        private bool ShouldMoveRight()
        {
            return _moveRight || _isPointerActivated && _pointerPosition.X > _playerHitBox.Right;
        }      

        private void DamageRecovering()
        {
            _damageRecoveryOpacityFrameSkip--;

            if (_damageRecoveryOpacityFrameSkip < 0)
            {
                _player.Opacity = 0.33;
                _damageRecoveryOpacityFrameSkip = 5;
            }
            else
            {
                _player.Opacity = 1;
            }

            _damageRecoveryCounter--;

            if (_damageRecoveryCounter <= 0)
            {
                _player.Opacity = 1;
                _isPlayerRecoveringFromDamage = false;
            }
        }

        private void MovePlayerX(MovementDirectionX movementDirectionX)
        {
            switch (movementDirectionX)
            {
                case MovementDirectionX.Left:
                    {
                        _player.SetFacingDirectionX(MovementDirectionX.Left);
                        _player.SetLeft(_player.GetLeft() - (_gameSpeed * _playerMovementSpeedMultiplier));
                    }
                    break;
                case MovementDirectionX.Right:
                    {
                        _player.SetFacingDirectionX(MovementDirectionX.Right);
                        _player.SetLeft(_player.GetLeft() + (_gameSpeed * _playerMovementSpeedMultiplier));
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
                    {
                        if (_jumpingEaseCounter > 0)
                            _jumpingEaseCounter -= 0.1;

                        _player.SetTop(_player.GetTop() - ((_gameSpeed * _playerMovementSpeedMultiplier) + _jumpingEaseCounter));
                    }
                    break;
                case MovementDirectionY.Down:
                    {
                        _fallingEaseCounter += 0.3;
                        _player.SetTop(_player.GetTop() + ((_gameSpeed * _playerMovementSpeedMultiplier) + _fallingEaseCounter));
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Enemy

        private void SpawnEnemy(Cloud cloud)
        {
            Enemy enemy = new(_scale);

            _markNum = _random.Next(0, _enemies.Length);

            enemy.EnemyMark = _markNum;
            enemy.SetState(EnemyState.Idle);
            enemy.PlaceOnCloud(cloud);

            GameView.Children.Add(enemy);
            _enemyCount++;
        }

        private void UpdateEnemy(Enemy enemy)
        {
            enemy.SetTop((enemy.LandedCloud.GetTop() + enemy.LandedCloud.Height / 2) - enemy.Height);
            enemy.SetLeft((enemy.LandedCloud.GetLeft() + enemy.LandedCloud.Width / 2) - enemy.Width / 2);

            var enemyHitBox = enemy.GetHitBox();
            var enemyDistantHitHox = enemy.GetDistantHitBox();

            // enable enemy attacking state if player is near
            if (enemyDistantHitHox.IntersectsWith(_playerDistantHitBox))
            {
                if (enemy.EnemyState != EnemyState.Attacking)
                    enemy.SetState(EnemyState.Attacking);
            }
            else
            {
                if (enemy.EnemyState != EnemyState.Idle)
                    enemy.SetState(EnemyState.Idle);
            }

            if (_playerHitBox.Left < enemyHitBox.Left)
                enemy.SetFacingDirectionX(MovementDirectionX.Left);

            if (_playerHitBox.Right > enemyHitBox.Right)
                enemy.SetFacingDirectionX(MovementDirectionX.Right);

            // if player is not recovering from damage and hits an enemy then loose health and fall
            if (!_isPlayerRecoveringFromDamage && enemyHitBox.IntersectsWith(_playerHitBox))
            {
                LooseHealth();

                if (!_isPowerMode || _powerUpType != PowerUpType.CloudRide)
                {
                    _fallingEaseCounter = _fallingEaseCounterDefault;
                    _idleDurationCounter = _idleDurationCounterDefault;

                    _player.SetState(PlayerState.Falling);
                }
            }
        }

        #endregion

        #region Cloud

        private void SpawnCloud(int multiplierY = -1)
        {
            Cloud cloud = new(_scale);
            RecyleCloud(cloud: cloud, multiplierY: multiplierY);
            GameView.Children.Add(cloud);
            _cloudCount++;
        }

        private void UpdateCloud(GameObject cloud)
        {
            cloud.SetTop(cloud.GetTop() + cloud.Speed);

            // only move the cloud side ways when in view port
            if (cloud.GetTop() + cloud.Height > 10)
            {
                var inCloud = cloud as Cloud;

                switch (inCloud.MovementDirectionX)
                {
                    case MovementDirectionX.Left:
                        cloud.SetLeft(cloud.GetLeft() - cloud.Speed / _cloudMovementDirectionXSpeedDivider);
                        break;
                    case MovementDirectionX.Right:
                        cloud.SetLeft(cloud.GetLeft() + cloud.Speed / _cloudMovementDirectionXSpeedDivider);
                        break;
                    default:
                        break;
                }

                var cloudHitBox = cloud.GetPlatformHitBox(_scale);

                // only land on a cloud when it's on a suitable height in viewport
                if (_playerStandingHitBox.Top > _windowHeight / 5 && _player.PlayerState == PlayerState.Falling && _playerStandingHitBox.IntersectsWith(cloudHitBox))
                {
                    _player.LandedCloud = inCloud;

                    _landedCloudEffectCounter = _landedCloudEffectCounterDefault;
                    _landedCloudEffectActive = true;

                    _idleDurationCounter = _idleDurationCounterDefault;
                    _player.SetState(PlayerState.Idle);
                }

                if (cloud.GetTop() > GameView.Height)
                    RecyleCloud(cloud as Cloud);
            }
        }

        private void RecyleCloud(Cloud cloud, int multiplierY = -1)
        {
            _markNum = _random.Next(0, _clouds.Length);
            cloud.SetContent(_clouds[_markNum]);

            cloud.Speed = _gameSpeed * _cloudSpeedFactor;
            cloud.MovementDirectionX = (MovementDirectionX)_random.Next(0, Enum.GetNames<MovementDirectionX>().Length);

            // reposition standing enemy on this cloud if so found
            if (GameView.GetGameObjects<Enemy>().FirstOrDefault(x => x.LandedCloud.Uid == cloud.Uid) is Enemy standingEnemy)
                standingEnemy.PlaceOnCloud(cloud);

            RandomizeCloudPosition(cloud: cloud, multiplierY: multiplierY);
        }

        private void RandomizeCloudPosition(GameObject cloud, int multiplierY = -1)
        {
            cloud.SetPosition(
                left: _random.Next((int)(50 * _scale), (int)(GameView.Width - (50 * _scale))),
                top: (GameView.Height / 4) * multiplierY);
        }

        private void LandedCloudEffect()
        {
            _landedCloudEffectCounter--;

            if (_landedCloudEffectCounter > 0)
            {
                _player.LandedCloud.SetTop(_player.LandedCloud.GetTop() + _player.LandedCloud.Speed * 0.9);
                _player.SetTop(_player.GetTop() + _player.LandedCloud.Speed * 0.9);
            }

            if (_landedCloudEffectCounter <= 0)
            {
                _landedCloudEffectActive = false;

                _landedCloudEffectReverseCounter = _landedCloudEffectReverseCounterDefault;
                _landedCloudEffectReverseActive = true;
            }
        }

        private void LandedCloudEffectReverse()
        {
            _landedCloudEffectReverseCounter--;

            if (_landedCloudEffectReverseCounter > 0)
            {
                _player.LandedCloud.SetTop(_player.LandedCloud.GetTop() - _player.LandedCloud.Speed * 0.9);
                _player.SetTop(_player.GetTop() - _player.LandedCloud.Speed * 0.9);
            }

            if (_landedCloudEffectReverseCounter <= 0)
            {
                _landedCloudEffectReverseActive = false;
            }
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

            // only consider player intersection after appearing in viewport
            if (collectible.GetTop() + collectible.Height > 10)
            {
                if (_playerHitBox.IntersectsWith(collectible.GetHitBox()))
                    Collectible(collectible);

                // if magnet power up received then pull collectibles to player
                if (_isPowerMode && _powerUpType == PowerUpType.MagnetPull)
                    MagnetPull(collectible);
            }

            if (collectible.GetTop() > GameView.Height)
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
                left: _random.Next(0, (int)GameView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
        }

        private void Collectible(GameObject collectible)
        {
            SoundHelper.PlayRandomSound(SoundType.COLLECTIBLE);

            AddScore(5);
            RecyleCollectible(collectible);
            AddHealth(2);

            _collectibleCollected++;
        }

        #endregion

        #region PowerUp

        private void SpawnPowerUp()
        {
            PowerUp powerUp = new(_scale);

            //TODO: set to random powerup
            var powerUpTypes = Enum.GetNames<PowerUpType>();
            powerUp.PowerUpType = (PowerUpType)_random.Next(0, powerUpTypes.Length);

            powerUp.SetContent(_powerUps[(int)powerUp.PowerUpType]);

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
                PowerUp(powerUp as PowerUp);
            }
            else
            {
                if (powerUp.GetTop() > GameView.Height)
                    GameView.AddDestroyableGameObject(powerUp);
            }
        }

        private void PowerUp(PowerUp powerUp)
        {
            _isPowerMode = true;
            _powerModeDurationCounter = _powerModeDuration;
            _powerUpCount++;
            _powerUpType = powerUp.PowerUpType;

            // if rocket power up received then change player to ufo
            if (_powerUpType == PowerUpType.CloudRide)
            {
                _jumpDurationCounter = _jumpDurationCounterDefault;
                _jumpingEaseCounter = _jumpEaseCounterDefault;
                _fallingEaseCounter = _fallingEaseCounterDefault;

                _player.SetState(PlayerState.Flying);
                _player.SetScaleTransform(_cloudFlightPlayerScaleModifier);

                SoundHelper.PlaySound(SoundType.PLAYER_YAY);
            }

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

            // if was in rocket mode set to falling mode
            if (_powerUpType == PowerUpType.CloudRide)
            {
                _player.SetScaleTransform(1);
                _player.SetState(PlayerState.Falling);
            }

            powerUpText.Visibility = Visibility.Collapsed;
            SoundHelper.PlaySound(SoundType.POWER_DOWN);
        }

        private void CloudRide()
        {
            double left = _player.GetLeft();
            double top = _player.GetTop();

            double playerMiddleX = left + _player.Width / 2;
            double playerMiddleY = top + _player.Height / 2;

            if (_isPointerActivated)
            {
                // move up
                if (_pointerPosition.Y < playerMiddleY - _gameSpeed)
                    _player.SetTop(top - _gameSpeed);

                // move left
                if (_pointerPosition.X < playerMiddleX - _gameSpeed)
                {
                    _player.SetLeft(left - _gameSpeed);
                    _player.SetFacingDirectionX(MovementDirectionX.Left, _cloudFlightPlayerScaleModifier);
                }

                // move down
                if (_pointerPosition.Y > playerMiddleY + _gameSpeed)
                    _player.SetTop(top + _gameSpeed * 2);

                // move right
                if (_pointerPosition.X > playerMiddleX + _gameSpeed)
                {
                    _player.SetLeft(left + _gameSpeed);
                    _player.SetFacingDirectionX(MovementDirectionX.Right, _cloudFlightPlayerScaleModifier);
                }
            }
            else
            {
                // move up
                if (_moveUp && top > 0 + (50 * _scale))
                    _player.SetTop(top - _gameSpeed);

                // move left
                if (_moveLeft && left > 0)
                {
                    _player.SetLeft(left - _gameSpeed);
                    _player.SetFacingDirectionX(MovementDirectionX.Left, _cloudFlightPlayerScaleModifier);
                }

                // move down
                if (_moveDown && top < GameView.Height - (100 * _scale))
                    _player.SetTop(top + _gameSpeed * 2);

                // move right
                if (_moveRight && left + _player.Width < GameView.Width)
                {
                    _player.SetLeft(left + _gameSpeed);
                    _player.SetFacingDirectionX(MovementDirectionX.Right, _cloudFlightPlayerScaleModifier);
                }
            }
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

        #endregion

        #region Health

        private void AddHealth(double health)
        {
            if (_playerHealth < 100)
            {
                if (_playerHealth + health > 100)
                    health = _playerHealth + health - 100;

                _playerHealth += health;
            }
        }

        private void LooseHealth()
        {
            SoundHelper.PlaySound(SoundType.HEALTH_LOSS);

            _damageRecoveryCounter = _damageRecoveryDelay;
            _isPlayerRecoveringFromDamage = true;

            _playerHealth -= _playerHealthLossPoints;

            if (_playerHealth <= 0)
                GameOver();
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
                _gameSpeed = (_gameSpeedDefault * _scale) + 0.2 * _difficultyMultiplier;
                _playerHealthLossPoints++;
                _difficultyMultiplier += 0.5;
                _scoreCap += 50;

#if DEBUG
                Console.WriteLine($"GAME SPEED: {_gameSpeed}");
                Console.WriteLine($"SCORE CAP: {_scoreCap}");
#endif
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

            GameView.Width = _windowWidth > 1000 ? 1000 : _windowWidth;
            GameView.Height = _windowHeight;

            if (_player is not null)
            {
                _player.SetSize(
                    width: Constants.PLAYER_WIDTH * _scale,
                    height: Constants.PLAYER_HEIGHT * _scale);
            }

#if DEBUG
            Console.WriteLine($"SCALE: {_scale}");
#endif
        }

        private void NavigateToPage(Type pageType)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region InGameMessage

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
