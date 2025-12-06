using Game.Shared;
using UnityEngine;
using UnityEditor;

namespace Game.Editor
{
    [CustomPropertyDrawer(typeof(ExposedScriptableObjectAttribute))]
    public class ExposedScriptableObjectAttributeDrawer : PropertyDrawer
    {
	    private UnityEditor.Editor _editor = null;

	    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	    {
			var referenceValue = property.objectReferenceValue;
		    EditorGUI.PropertyField(position, property, label, true);

		    if (referenceValue != null)
		    {
			    property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
		    }
			
			if (property.isExpanded && referenceValue != null)
			{
				EditorGUI.indentLevel++;
				var exposedAttribute = attribute as ExposedScriptableObjectAttribute;
				using (new EditorGUI.DisabledScope(exposedAttribute?.Readonly ?? false))
				{
					if (_editor == null)
					{
						UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _editor);
					}
					_editor.OnInspectorGUI();
				}
				EditorGUI.indentLevel--;
			}
	    }
    }
}
