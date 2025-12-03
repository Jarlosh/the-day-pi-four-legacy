using UnityEngine;

namespace Interactables.Conditions
{
    [CreateAssetMenu(fileName = "ComplexConditionSo", menuName = "SO/Conditions/ComplexConditionSo")]
    public class ComplexConditionSo : ScriptableObject
    {
        [SerializeField] private ConditionNode _root = new ConditionNode();

        public ICondition Build()
        {
            if (_root == null)
            {
                Debug.LogError($"ConditionPrototype '{name}': root is null");
                return new ConstantCondition(false);
            }

            return _root.Build();
        }
    }
}