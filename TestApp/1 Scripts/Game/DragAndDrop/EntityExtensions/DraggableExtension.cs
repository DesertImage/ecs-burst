using DesertImage.ECS;
using UnityEngine.EventSystems;

namespace Game.DragAndDrop
{
    public class DraggableExtension : EntityExtension, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Entity _entity;

        public override void Link(Entity entity) => _entity = entity;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _entity.Replace<DragStartTag>();
            _entity.Replace<Drag>();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _entity.Replace<DropTag>();
        }

        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}