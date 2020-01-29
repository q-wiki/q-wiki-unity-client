namespace Minigame.Helpers
{
    public class MinigameHelpers
    {
        private static bool isNextUpdateBlocked;

        public static void BlockNextMiniGameUpdate()
        {
            isNextUpdateBlocked = true;
        }

        public static void UnblockUpdates()
        {
            isNextUpdateBlocked = false;
        }

        public static bool IsNextUpdateBlocked()
        {
            return isNextUpdateBlocked;
        }
    }
}