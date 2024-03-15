using DesertImage.ECS;
using UnityEngine;

namespace Game
{
    public class CardView : EntityView
    {
        [SerializeField] private Transform previewChild;

        public override void Initialize(in Entity entity)
        {
            base.Initialize(in entity);

            entity.Replace(new CardPreviewChild { Value = previewChild });
        }
    }
}