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
	
	public record EnemySpawnedEvent(Enemy Enemy)
	{
	}
    
	public record EnemyDeathEvent(Enemy Enemy)
	{
	}
	
	public record StylePointsAddedEvent(float Points, float TotalPoints)
	{
	}
    
	public record StyleRankChangedEvent(int RankIndex, string RankName, float Multiplier)
	{
	}
	
	public record StyleScoreChangedEvent(float TotalScore)
	{
	}
    
	public record StyleMeterChangedEvent(float MeterValue, int CurrentRankIndex)
	{
	}
	
	public record VacuumStartedEvent()
	{
	}
    
	public record VacuumStoppedEvent()
	{
	}
    
	public record VacuumSuccessEvent()
	{
	}
    
	public record ShootEvent()
	{
	}
}