namespace Game.Client
{
	public enum GameResults
	{
		Win,
		Defeat
	}
    
	public record WaveStartedEvent(int WaveNumber, int TotalWaves)
	{
	}

	public record WaveCompletedEvent(int WaveNumber)
	{
	}

	public record GameCancelEvent(GameResults Results)
	{
		
	}

	public record CountdownEvent(int Seconds)
	{
	}
}