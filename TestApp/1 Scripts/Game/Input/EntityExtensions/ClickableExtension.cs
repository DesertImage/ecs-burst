using DesertImage.ECS;
using UnityEngine.EventSystems;

namespace Game.Input
{
    public class ClickableExtension : EntityExtension, IPointerDownHandler, IPointerUpHandler
    {
        private Entity _entity;

        public override void Link(Entity entity) => _entity = entity;

        public void OnPointerDown(PointerEventData eventData) => _entity.Replace<Click>();
        public void OnPointerUp(PointerEventData eventData) => _entity.Replace<ClickEndTag>();
    }
}