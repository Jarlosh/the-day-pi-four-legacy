using UnityEngine;

namespace Game.Shared
{
	public static class AnimatorUtils
	{
		public static bool HasParameter(this Animator animator, int parameterHash)
		{
			if (!animator.runtimeAnimatorController)
			{
				return false;
			}
			
			var parameters = animator.parameters;
	
			foreach (var param in parameters)
			{
				if (param.nameHash == parameterHash)
				{
					return true;
				}
			}
			
			return false;
		}

		public static bool HasParameter(this Animator animator, string parameterName)
		{
			return animator.HasParameter(Animator.StringToHash(parameterName));
		}
		
		public static bool IsInState(Animator animator, int layer, string stateName)
		{
			if (layer < 0 || layer >= animator.layerCount)
			{
				return false;
			}
			
			var s = animator.GetCurrentAnimatorStateInfo(layer);
			return s.IsName(stateName);
		}
		
		public static bool IsInState(Animator animator, int layer, int stateHash)
		{
			if (layer < 0 || layer >= animator.layerCount)
			{
				return false;
			}
			
			var s = animator.GetCurrentAnimatorStateInfo(layer);
			return s.shortNameHash == stateHash;
		}
		
		public static float GetNormalizedTime(Animator animator, int layer)
		{
			if (layer < 0 || layer >= animator.layerCount)
			{
				return 0f;
			}
			
			var s = animator.GetCurrentAnimatorStateInfo(layer);
			return s.normalizedTime;
		}
	}
}