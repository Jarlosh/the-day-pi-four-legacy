using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactables.Conditions
{
    [Serializable]
    public class ConditionNode : ICondition
    {
        [SerializeField] private ConditionNodeKind _kind = ConditionNodeKind.Leaf;
        [SerializeField] private SoCondition _leafCondition;
        [SerializeField] private ConditionNode[] _children;

        private ICondition _cache;
        public ICondition Built => _cache ??= Build();

        public bool IsMet()
        {
            return Built.IsMet();
        }

        public ICondition Build()
        {
            switch (_kind)
            {
                case ConditionNodeKind.Leaf:
                    if (_leafCondition == null)
                    {
                        Debug.LogError("ConditionNode Leaf: leafCondition is null");
                        return new ConstantCondition(false);
                    }

                    return _leafCondition;

                case ConditionNodeKind.Not:
                    if (_children == null || _children.Length == 0 || _children[0] == null)
                    {
                        Debug.LogError("ConditionNode Not: child missing");
                        return new ConstantCondition(false);
                    }

                    return new NotCondition(_children[0].Build());

                case ConditionNodeKind.And:
                    return BuildGroup(isAnd: true);

                case ConditionNodeKind.Or:
                    return BuildGroup(isAnd: false);

                default:
                    Debug.LogError($"ConditionNode: unknown kind {_kind}");
                    return new ConstantCondition(false);
            }
        }

        private ICondition BuildGroup(bool isAnd)
        {
            if (_children == null || _children.Length == 0)
            {
                Debug.LogError($"ConditionNode {(isAnd ? "And" : "Or")}: no children");
                return new ConstantCondition(false);
            }

            var operands = new List<ICondition>(_children.Length);
            foreach (var child in _children)
            {
                if (child == null)
                {
                    Debug.LogError("ConditionNode group: child is null, skipping");
                    continue;
                }

                var cond = child.Build();
                if (cond != null)
                {
                    operands.Add(cond);
                }
            }

            if (operands.Count == 0)
            {
                Debug.LogError("ConditionNode group: all children invalid");
                return new ConstantCondition(false);
            }

            return isAnd ? new AndCondition(operands) : new OrCondition(operands);
        }
    }
}