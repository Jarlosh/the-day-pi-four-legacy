using System.Collections.Generic;
using UnityEngine;

namespace Game.Client
{
	[CreateAssetMenu(fileName = "WaveSettings", menuName = "Game/Configs/Wave Settings")]
	public class WaveSettings: ScriptableObject
	{
		[field:SerializeField]
		public List<WaveData> Waves { get; private set; }= new List<WaveData>();
	}
}