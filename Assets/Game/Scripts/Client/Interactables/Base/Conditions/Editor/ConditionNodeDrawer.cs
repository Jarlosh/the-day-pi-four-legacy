#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Interactables.Conditions.Editor
{
    [CustomPropertyDrawer(typeof(ConditionNode))]
    public class ConditionNodeDrawer : PropertyDrawer
    {
        private const float LineHeight = 18f; // EditorGUIUtility.singleLineHeight
        private const float Space = 3f;
        
        private const float FoldoutWidth = 6f;
        private const float EnumWidth = 100f;
        
        private const float FooterHeight = LineHeight + Space; // rough

        private const int InnerListRectOffset = -10;
        private const int OuterListRectOffset = -16;
        
        private readonly Regex _childRegex = new Regex(@"\._children\.Array\.data\[\d+\]$");

        private static readonly Dictionary<string, ReorderableList> _lists = new Dictionary<string, ReorderableList>();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (IsRootNode(property))
            {
                var labelRect = new Rect(position.x, position.y, position.width, LineHeight);
                EditorGUI.LabelField(labelRect, property.displayName);
                position.yMin += LineHeight;
                position.xMin -= OuterListRectOffset;
            }

            var kindProp = property.FindPropertyRelative("_kind");
            var leafProp = property.FindPropertyRelative("_leafCondition");
            var childrenProp = property.FindPropertyRelative("_children");

            var kind = (ConditionNodeKind) kindProp.enumValueIndex;

            if (kind == ConditionNodeKind.Leaf)
            {
                DrawLeaf(position, kindProp, leafProp);
                EditorGUI.EndProperty();
                return;
            }

            if (!property.isExpanded)
            {
                var lineRect = new Rect(position.x, position.y, position.width, LineHeight);
                DrawListHeaderBack(lineRect);
                DrawFoldout(lineRect, property, kindProp);
                EditorGUI.EndProperty();
                return;
            }

            if (kind == ConditionNodeKind.Not)
            {
                EnsureSingleChild(childrenProp);
            }
            
            switch (kind)
            {
                case ConditionNodeKind.Not:
                case ConditionNodeKind.And:
                case ConditionNodeKind.Or:
                    DrawGroup(position, kind, childrenProp, kindProp, property);
                    break;
            }

            EditorGUI.EndProperty();
        }

        private void DrawGroup(Rect position, ConditionNodeKind kind,
            SerializedProperty childrenProp, SerializedProperty kindProp, SerializedProperty property)
        {
            var rect = new Rect(position.x, position.y, position.width, LineHeight);
            DrawGroup(rect, property.serializedObject, childrenProp, kindProp, property, 
                drawControls: kind != ConditionNodeKind.Not);
        }
        
        private void DrawListHeaderBack(Rect rect)
        {
            rect.min += new Vector2(OuterListRectOffset, -1);
            ReorderableList.defaultBehaviours.DrawHeaderBackground(rect);
        }

        private void DrawFoldout(Rect rect, SerializedProperty property, SerializedProperty kindProp)
        {
            var foldRect = new Rect(rect.x, rect.y, FoldoutWidth, LineHeight); 
            var enumRect = new Rect(rect.x, rect.y, EnumWidth, LineHeight);

            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, GUIContent.none, true);
            EditorGUI.PropertyField(enumRect, kindProp, GUIContent.none);
        }

        private void DrawLeaf(Rect pos, SerializedProperty kindProp, SerializedProperty leafProp)
        {
            var typeRect = new Rect(pos.x, pos.y, 100, LineHeight);
            var propRect = new Rect(typeRect.xMax, pos.y, pos.width - 100, LineHeight);
            
            EditorGUI.PropertyField(typeRect, kindProp, GUIContent.none);
            EditorGUI.PropertyField(propRect, leafProp, GUIContent.none);
        }

        private void EnsureSingleChild(SerializedProperty children)
        {
            if (children.arraySize != 1)
            {
                children.arraySize = 1;
            }
        }

        private void DrawGroup(Rect pos, SerializedObject obj, SerializedProperty children, SerializedProperty kind, SerializedProperty expandable, bool drawControls)
        {
            if(expandable.isExpanded)
            {
                pos.min += new Vector2(OuterListRectOffset, -1);
                GetOrAdd(obj, children, kind, expandable).DoList(pos);
            }
            else
            {
                DrawFoldout(pos, expandable, kind);
            }
        }

        private ReorderableList GetOrAdd(SerializedObject obj, SerializedProperty children,
            SerializedProperty kind, SerializedProperty expandable)
        {
            string key = $"{children.serializedObject.targetObject.GetInstanceID()}/{children.propertyPath}";
            
            if (_lists.TryGetValue(key, out var list) && list.serializedProperty.serializedObject == obj)
            {
                return list;
            }

            // Debug.Log("make list");
            list = _lists[key] = new ReorderableList(obj, children, true, true, false, false);
            list.drawHeaderCallback = DrawHeader;
            list.drawElementCallback = DrawElement;
            list.elementHeightCallback = GetElementHeight;
            list.multiSelect = false;
            
            void DrawHeader(Rect rect)
            {
                rect.xMin -= InnerListRectOffset;
                DrawFoldout(rect, expandable, kind);
                
                if (((ConditionNodeKind) kind.enumValueIndex) == ConditionNodeKind.Not)
                {
                    return;
                }

                var size = rect.height;
                var addRect = new Rect(rect.xMax - size, rect.y, size, size);
                DrawAdd(addRect);
                
                var removeRect = addRect;
                removeRect.x -= size + Space;
                DrawRemove(removeRect);
            }

            void DrawElement(Rect rect, int i, bool isActive, bool isFocused)
            {
                var element = children.GetArrayElementAtIndex(i);
                var kindProp = element.FindPropertyRelative("_kind");
                var elementKind = (ConditionNodeKind) kindProp.enumValueIndex;
                
                if (DoesConditionHasOperands(elementKind))
                {
                    rect.xMin += 16;
                }
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            }

            void DrawAdd(Rect rect)
            {
                if(GUI.Button(rect, "+"))
                {
                    children.arraySize = list.count + 1;
                    list.index = children.arraySize - 1;
                }
            }

            void DrawRemove(Rect rect)
            {
                if(GUI.Button(rect, "-"))
                {
                    RemoveElement(children, list);
                }
                // list.sele
            }
            
            float GetElementHeight(int index)
            {
                var element = children.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, GUIContent.none);
            }
            
            return list;
        }

        private bool DoesConditionHasOperands(ConditionNodeKind kind)
        {
            return kind == ConditionNodeKind.And || 
                   kind == ConditionNodeKind.Or || 
                   kind == ConditionNodeKind.Not;
        }

        private static void RemoveElement(SerializedProperty children, ReorderableList list)
        {
            // copied from ReorderableList's implementation
            
            int[] numArray1;
            if (list.selectedIndices.Count <= 0)
            {
                numArray1 = new int[1]{ list.index};
            }
            else
            {
                numArray1 = list.selectedIndices.Reverse().ToArray();
            }
            int[] numArray2 = numArray1;
            int num = -1;
            foreach (int index1 in numArray2)
            {
                if (index1 < list.count)
                {
                    children.DeleteArrayElementAtIndex(index1);
                    if (index1 < list.count - 1)
                    {
                        SerializedProperty serializedProperty = children.GetArrayElementAtIndex(index1);
                        for (int index2 = index1 + 1; index2 < list.count; ++index2)
                        {
                            SerializedProperty arrayElementAtIndex = children.GetArrayElementAtIndex(index2);
                            serializedProperty.isExpanded = arrayElementAtIndex.isExpanded;
                            serializedProperty = arrayElementAtIndex;
                        }
                    }
                    num = index1;
                }
            }
            list.index = Mathf.Clamp(num - 1, 0, list.count - 1);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var labelOffset = IsRootNode(property) ? LineHeight + Space : 0;
            var kindProp = property.FindPropertyRelative("_kind");
            var childrenProp = property.FindPropertyRelative("_children");

            var kind = (ConditionNodeKind)kindProp.enumValueIndex;

            if (kind == ConditionNodeKind.Leaf)
            {
                return labelOffset + LineHeight + Space;
            }

            if (!property.isExpanded)
            {
                return labelOffset + LineHeight + Space;
            }

            float height = 0f;
            switch (kind)
            {
                case ConditionNodeKind.And:
                case ConditionNodeKind.Or:
                case ConditionNodeKind.Not:
                    var list = GetOrAdd(property.serializedObject, childrenProp, kindProp, property);
                    height += list.GetHeight() - FooterHeight;
                    break;
            }

            return labelOffset + height;
        }

        private bool IsRootNode(SerializedProperty property)
        {
            // u can't use property.depth, because "root" condition node can be nested in other [serializable]
            return !_childRegex.IsMatch(property.propertyPath);
        }
    }
}
#endif