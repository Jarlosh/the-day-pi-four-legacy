using System.Collections.Generic;

namespace Interactables.Conditions
{
    public sealed class AndCondition : ICondition
    {
        private readonly ICondition[] _conditions;

        public AndCondition(IEnumerable<ICondition> conditions)
        {
            _conditions = conditions != null
                ? new List<ICondition>(conditions).ToArray()
                : System.Array.Empty<ICondition>();
        }

        public bool IsMet()
        {
            if (_conditions.Length == 0)
                return false;

            foreach (var c in _conditions)
            {
                if (c == null)
                    return false;

                if (!c.IsMet())
                    return false;
            }

            return true;
        }
    }
}