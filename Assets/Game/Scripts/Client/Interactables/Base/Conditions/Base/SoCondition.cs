using UnityEngine;

namespace Interactables.Conditions
{
    public abstract class SoCondition : ScriptableObject, ICondition
    {
        public abstract bool IsMet();
    }
}