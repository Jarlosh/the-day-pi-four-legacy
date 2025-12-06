using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Client
{
	[Serializable]
	public class StyleRank
	{
		[field: SerializeField] public string RankName { get; private set; } = "C";
		[field: SerializeField] public float Multiplier { get; private set; } = 1f;

		public StyleRank(string rankName, float multiplier)
		{
			RankName = rankName;
			Multiplier = multiplier;
		}
	}

	
	[CreateAssetMenu(fileName = "StyleRank_Settings", menuName = "Game/Configs/Style Rank Settings")]
	public class StyleRankSettings : ScriptableObject
	{
		[field: Header("Style Ranks")]
		[field: SerializeField] public List<StyleRank> Ranks { get; private set; } = new()
		{
			new StyleRank("C", 1f),
			new StyleRank("B", 1.5f),
			new StyleRank("A", 1.75f),
			new StyleRank("S", 2f),
			new StyleRank("BadASS", 2.51f)
		};
        
		public StyleRank GetRank(int index)
		{
			if (index < 0 || index >= Ranks.Count)
				return Ranks[0];
            
			return Ranks[index];
		}
        
		public int GetRankCount()
		{
			return Ranks.Count;
		}
	}
}