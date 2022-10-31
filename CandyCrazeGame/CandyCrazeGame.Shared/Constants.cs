using System;
using System.Collections.Generic;

namespace CandyCrazeGame
{
    public static class Constants
    {
        public const string GAME_ID = "candy-craze";

        #region Measurements

        public const double DEFAULT_FRAME_TIME = 18;

        public const double PLAYER_WIDTH = 80;
        public const double PLAYER_HEIGHT = 80;

        public const double ENEMY_WIDTH = 90;
        public const double ENEMY_HEIGHT = 85;

        public const double COLLECTIBLE_SIZE = 70;

        public const double POWERUP_SIZE = 70;

        public const double CLOUD_WIDTH = 150;
        public const double CLOUD_HEIGHT = 100;

        public const double HEALTH_WIDTH = 80;
        public const double HEALTH_HEIGHT = 80;

        #endregion

        #region Images

        public static KeyValuePair<ElementType, Uri>[] ELEMENT_TEMPLATES = new KeyValuePair<ElementType, Uri>[]
        {
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player_idle.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_JUMP, new Uri("ms-appx:///Assets/Images/player_jump.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_FALL, new Uri("ms-appx:///Assets/Images/player_fall.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_FLY, new Uri("ms-appx:///Assets/Images/player_fly.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.ENEMY, new Uri("ms-appx:///Assets/Images/enemy1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.ENEMY, new Uri("ms-appx:///Assets/Images/enemy2.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.ENEMY, new Uri("ms-appx:///Assets/Images/enemy3.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.ENEMY_ATTACK, new Uri("ms-appx:///Assets/Images/enemy1_attack.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.ENEMY_ATTACK, new Uri("ms-appx:///Assets/Images/enemy2_attack.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.ENEMY_ATTACK, new Uri("ms-appx:///Assets/Images/enemy3_attack.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.POWERUP, new Uri("ms-appx:///Assets/Images/powerup1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.POWERUP, new Uri("ms-appx:///Assets/Images/powerup2.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.CLOUD, new Uri("ms-appx:///Assets/Images/cloud1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CLOUD, new Uri("ms-appx:///Assets/Images/cloud2.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CLOUD, new Uri("ms-appx:///Assets/Images/cloud3.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible2.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible3.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible4.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible5.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible6.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible7.png")),
        };

        #endregion

        #region Sounds

        public static KeyValuePair<SoundType, string>[] SOUND_TEMPLATES = new KeyValuePair<SoundType, string>[]
        {
            new KeyValuePair<SoundType, string>(SoundType.MENU_SELECT, "Assets/Sounds/menu-select.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro2.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background3.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.GAME_OVER, "Assets/Sounds/game-over.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.POWER_UP, "Assets/Sounds/power-up.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.POWER_DOWN, "Assets/Sounds/power-down.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.PLAYER_YAY, "Assets/Sounds/player_yay.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.PLAYER_JUMP, "Assets/Sounds/jump.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.COLLECTIBLE, "Assets/Sounds/food-bite1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.COLLECTIBLE, "Assets/Sounds/food-bite2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.COLLECTIBLE, "Assets/Sounds/food-bite3.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.HEALTH_LOSS, "Assets/Sounds/health_loss.mp3"),
        };

        #endregion

        #region Web Api Base Urls
#if DEBUG
        public const string GAME_API_BASEURL = "https://localhost:7238";
#else
        public const string GAME_API_BASEURL = "https://astro-odyssey-web-api.herokuapp.com";
#endif
        #endregion

        #region Web Api Endpoints

        public const string Action_Ping = "/api/Query/Ping";

        public const string Action_Authenticate = "/api/Command/Authenticate";
        public const string Action_SignUp = "/api/Command/SignUp";
        public const string Action_SubmitGameScore = "/api/Command/SubmitGameScore";
        public const string Action_GenerateSession = "/api/Command/GenerateSession";
        public const string Action_ValidateSession = "/api/Command/ValidateSession";

        public const string Action_GetGameProfile = "/api/Query/GetGameProfile";
        public const string Action_GetGameProfiles = "/api/Query/GetGameProfiles";
        public const string Action_GetGameScores = "/api/Query/GetGameScores";
        public const string Action_GetUser = "/api/Query/GetUser";
        public const string Action_CheckIdentityAvailability = "/api/Query/CheckIdentityAvailability";

        #endregion

        #region Session Keys

        public const string CACHE_SESSION_KEY = "Session";
        public const string CACHE_LANGUAGE_KEY = "Language";

        #endregion

        #region Cookie Keys

        public const string COOKIE_KEY = "Cookie";
        public const string COOKIE_ACCEPTED_KEY = "Accepted";

        #endregion
    }
}
