using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
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

        #endregion

        #region Methods

        public void SetState(PlayerState playerState)
        {
            PlayerState = playerState;

            switch (playerState)
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
                default:
                    break;
            }
        }

        public void SetJumpDirection(JumpDirection jumpDirection)
        {
            switch (jumpDirection)
            {
                case JumpDirection.Left:
                    SetScaleX(-1);
                    break;
                case JumpDirection.Right:
                    SetScaleX(1);
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
        Falling
    }

    public enum JumpDirection
    {
        Left,
        Right,
    }
}
