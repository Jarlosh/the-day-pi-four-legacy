using System.Collections.Generic;

namespace Interactables.Conditions
{
    public sealed class OrCondition : ICondition
    {
        private readonly ICondition[] _conditions;

        public OrCondition(IEnumerable<ICondition> conditions)
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
                    continue;

                if (c.IsMet())
                    return true;
            }

            return false;
        }
    }
}