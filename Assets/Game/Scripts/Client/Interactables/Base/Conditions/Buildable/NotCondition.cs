namespace Interactables.Conditions
{
    public sealed class NotCondition : ICondition
    {
        private readonly ICondition _inner;

        public NotCondition(ICondition inner)
        {
            _inner = inner;
        }

        public bool IsMet()
        {
            if (_inner == null)
                return false;

            return !_inner.IsMet();
        }
    }
}