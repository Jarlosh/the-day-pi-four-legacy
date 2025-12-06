using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Client
{
	[Serializable]
	public class WaveData 
	{
		[field:Header("Enemy Settings")]
		[field:SerializeField]
		[field:Tooltip("Префабы противников для этой волны")]
		public List<Enemy> EnemyPrefabs { get; private set; } = new();
        
		[field:SerializeField]
		[field:Tooltip("Максимальное количество противников в этой волне")]
		public int MaxEnemiesInWave { get; private set; } = 10;
        
		[field:Header("Spawn Settings")]
		[field:SerializeField]
		[field:Tooltip("Максимальное количество спавнов за один кадр")]
		public int MaxSpawnsPerFrame { get; private set; } = 2;
        
		[field:SerializeField]
		[field:Tooltip("Интервал между спавнами (в секундах)")]
		public float SpawnInterval { get; private set; } = 0.5f;
	}
}