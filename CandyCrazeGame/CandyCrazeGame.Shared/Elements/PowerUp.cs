using System.Linq;

namespace CandyCrazeGame
{
    public class PowerUp : GameObject
    {
        public PowerUp(double scale)
        {
            Tag = ElementType.POWERUP;

            Width = Constants.POWERUP_SIZE * scale;
            Height = Constants.POWERUP_SIZE * scale;
        }

        public PowerUpType PowerUpType { get; set; }
    }

    public enum PowerUpType
    {
        MagnetPull,
        CloudRide,
    }
}

