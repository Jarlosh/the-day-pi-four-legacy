namespace Interactables.Conditions
{
    public sealed class ConstantCondition : ICondition
    {
        private readonly bool Value;

        public ConstantCondition(bool value)
        {
            Value = value;
        }

        public bool IsMet() => Value;
    }
}