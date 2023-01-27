using System.Linq;

namespace CandyCrazeGame
{
    public class Enemy : GameObject
    {
        #region Ctor

        public Enemy(double scale)
        {
            Tag = ElementType.ENEMY;

            Height = Constants.ENEMY_HEIGHT * scale;
            Width = Constants.ENEMY_WIDTH * scale;
        }

        #endregion

        #region Properties

        public EnemyState EnemyState { get; set; }

        public Cloud LandedCloud { get; set; }

        public int EnemyMark { get; set; } = 0;

        #endregion

        #region Methods

        public void PlaceOnCloud(Cloud cloud)
        {
            LandedCloud = cloud;
            this.PlaceRelativeToCloud(cloud);
        }

        public void SetState(EnemyState enemyState)
        {
            EnemyState = enemyState;

            switch (EnemyState)
            {
                case EnemyState.Idle:
                    SetContent(Constants.ELEMENT_TEMPLATES.Where(x => x.Key is ElementType.ENEMY).Select(x => x.Value).ToArray()[EnemyMark]);
                    break;
                case EnemyState.Attacking:
                    SetContent(Constants.ELEMENT_TEMPLATES.Where(x => x.Key is ElementType.ENEMY_ATTACK).Select(x => x.Value).ToArray()[EnemyMark]);
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

    public enum EnemyState
    {
        Idle,
        Attacking,
    }
}
