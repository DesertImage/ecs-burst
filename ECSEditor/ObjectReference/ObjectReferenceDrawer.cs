using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DesertImage.ECS.Editor
{
    [CustomPropertyDrawer(typeof(ObjectReference<>))]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var id = property.FindPropertyRelative(nameof(ObjectReference<Object>.Id));
            var idValue = id.uintValue;
            var storage = ObjectsReferenceRegistry.GetStorage();
            var obj = storage.Get<Object>(ref idValue, null);

            var container = new VisualElement();
            var objectField = new ObjectField(property.displayName)
            {
                objectType = fieldInfo.FieldType.GenericTypeArguments[0],
                allowSceneObjects = false,
                value = obj
            };

            objectField.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue == evt.previousValue) return;

                var tempId = 0u;
                storage.Get<Object>(ref tempId, evt.newValue);

                var prop = property.serializedObject.FindProperty(id.propertyPath);
                // prop.serializedObject.Update();
                id.uintValue = tempId;
                prop.serializedObject.ApplyModifiedProperties();
                prop.serializedObject.Update();
            });

            return container;
        }
    }
}