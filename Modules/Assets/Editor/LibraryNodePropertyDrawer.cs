using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DesertImage.Assets.Editor
{
    [CustomPropertyDrawer(typeof(LibraryNode<,>))]
    public class LibraryNodePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var id = property.FindPropertyRelative(nameof(LibraryNode<uint, Object>.Id));
            var obj = property.FindPropertyRelative(nameof(LibraryNode<uint, Object>.Value));

            var container = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                }
            };

            var arguments = fieldInfo.FieldType.GenericTypeArguments[0];

            var propertyField = new PropertyField(id, string.Empty)
            {
                style =
                {
                    width = 100f,
                    maxWidth = 200f
                }
            };

            var objectField = new ObjectField
            {
                objectType = arguments.GenericTypeArguments[1],
                allowSceneObjects = false,
                value = obj?.objectReferenceValue,
                style =
                {
                    flexGrow = 1f
                }
            };

            objectField.RegisterValueChangedCallback
            (
                evt =>
                {
                    if (evt.newValue == evt.previousValue) return;
                    objectField.value = evt.newValue;

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
            );

            container.Add(propertyField);
            container.Add(objectField);


            return container;
        }
    }
}