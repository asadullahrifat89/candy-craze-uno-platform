using System.Linq;

namespace CandyCrazeGame
{
    public class Player : GameObject
    {
        #region Ctor

        public Player(double scale)
        {
            Tag = ElementType.PLAYER;

            Width = Constants.PLAYER_WIDTH * scale;
            Height = Constants.PLAYER_HEIGHT * scale;

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
        }

        #endregion

        #region Properties

        public PlayerState PlayerState { get; set; } = PlayerState.Idle;

        public Cloud LandedCloud { get; set; }

        #endregion

        #region Methods

        public void PlaceOnCloud(Cloud cloud)
        {
            LandedCloud = cloud;
            this.PlaceRelativeToCloud(cloud);
        }

        public void SetState(PlayerState playerState)
        {
            PlayerState = playerState;

            switch (PlayerState)
            {
                case PlayerState.Idle:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
                    break;
                case PlayerState.Jumping:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_JUMP).Value);
                    break;
                case PlayerState.Falling:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_FALL).Value);
                    break;
                case PlayerState.Flying:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_FLY).Value);
                    break;
                default:
                    break;
            }
        }

        public void SetFacingDirectionX(MovementDirectionX movementDirectionX, double scaleX = 1)
        {
            switch (movementDirectionX)
            {
                case MovementDirectionX.Left:
                    SetScaleX(scaleX * -1);
                    break;
                case MovementDirectionX.Right:
                    SetScaleX(scaleX);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }

    public enum PlayerState
    {
        Idle,
        Jumping,
        Falling,
        Flying
    }

    public enum MovementDirectionY
    {
        Up,
        Down,
    }

    public enum MovementDirectionX
    {
        None,
        Left,
        Right,
    }
}
