using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Linq;

namespace CandyCrazeGame
{
    public class Player : GameObject
    {
        #region Fields



        #endregion

        #region Ctor

        public Player(double scale)
        {
            Tag = ElementType.PLAYER_IDLE;

            Width = Constants.PLAYER_SIZE * scale;
            Height = Constants.PLAYER_SIZE * scale;

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_IDLE).Value);
        }

        #endregion

        #region Methods

        public void SetPose(PlayerPose playerPose)
        {
            switch (playerPose)
            {
                case PlayerPose.Idle:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_IDLE).Value);
                    break;
                case PlayerPose.Jumping:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_JUMP).Value);
                    break;
                case PlayerPose.Falling:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_FALL).Value);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }

    public enum PlayerPose
    {
        Idle,
        Jumping,
        Falling
    }
}
