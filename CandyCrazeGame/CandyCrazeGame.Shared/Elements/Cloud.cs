namespace CandyCrazeGame
{
    public class Cloud : GameObject
    {
        #region Ctor

        public Cloud(double scale)
        {
            Tag = ElementType.CLOUD;

            Height = Constants.CLOUD_SIZE * scale;
            Width = Constants.CLOUD_SIZE * scale;
        }

        #endregion
    }
}
