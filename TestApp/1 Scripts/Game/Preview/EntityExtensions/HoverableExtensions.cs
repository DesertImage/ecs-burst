using DesertImage.ECS;
using UnityEngine.EventSystems;

namespace Game
{
    public class HoverableExtensions : EntityExtension, IPointerEnterHandler, IPointerExitHandler
    {
        private Entity _entity;

        public override void Link(Entity entity) => _entity = entity;


        public void OnPointerEnter(PointerEventData eventData)
        {
            _entity.Replace<Hover>();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _entity.Replace<Unhover>();
        }
    }
}