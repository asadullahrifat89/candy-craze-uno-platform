namespace CandyCrazeGame
{
    public class Cloud : GameObject
    {
        #region Ctor

        public Cloud(double scale)
        {
            Tag = ElementType.CLOUD;

            Height = Constants.CLOUD_HEIGHT * scale;
            Width = Constants.CLOUD_WIDTH * scale;
        }

        #endregion

        #region Properties

        public MovementDirectionX MovementDirectionX { get; set; }

        #endregion
    }
}
