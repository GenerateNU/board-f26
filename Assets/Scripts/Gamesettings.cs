
public static class GameSettings
{
    // 0 means "not chosen" (e.g. you pressed Play directly in Level_01 in the editor).
    public static int PlayerCount = 0;

    public static bool HasPlayerCount => PlayerCount > 0;
}