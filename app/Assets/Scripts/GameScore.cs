using UnityEngine;

public static class GameScore
{
    public static int zombiesKilled = 0;
    public static float survivalTime = 0f;
    private static float gameStartTime = 0f;
    private static bool gameStarted = false;
    
    public static void StartGame()
    {
        gameStartTime = Time.time;
        gameStarted = true;
        Debug.Log("GameScore: Game started");
    }
    
    public static void AddKill()
    {
        zombiesKilled++;
        Debug.Log($"GameScore: Zombie killed! Total kills: {zombiesKilled}");
    }
    
    public static void UpdateSurvivalTime()
    {
        if (gameStarted)
        {
            survivalTime = Time.time - gameStartTime;
        }
    }
    
    public static void Reset()
    {
        zombiesKilled = 0;
        survivalTime = 0f;
        gameStartTime = Time.time;
        gameStarted = true;
        Debug.Log("GameScore: Score reset");
    }
    
    public static string GetScoreText()
    {
        UpdateSurvivalTime();
        return $"Zombies Killed: {zombiesKilled}\nSurvival Time: {survivalTime:F1}s";
    }
    
    public static int GetTotalScore()
    {
        // Simple scoring: 10 points per kill + 1 point per second survived
        return (zombiesKilled * 10) + Mathf.FloorToInt(survivalTime);
    }
}
