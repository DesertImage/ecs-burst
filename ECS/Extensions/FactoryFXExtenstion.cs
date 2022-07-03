using DesertImage.ECS;
using DesertImage.Enums;
using DesertImage.FX;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DesertImage
{
    public static class FactoryFXExtenstion
    {
        private static FXService FXService => _fxService ??= Core.Instance.Get<FXService>();

        private static FXService _fxService;

        static FactoryFXExtenstion()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _fxService = null;
        }

        #region SPAWN

        public static EffectBase Spawn(this object sender, EffectsId id, Vector3 position)
        {
            return FXService.Spawn(id, position, Quaternion.identity, null);
        }

        public static EffectBase Spawn(this object sender, EffectsId id, Transform parent)
        {
            return FXService.Spawn(id, parent.position, parent.rotation, parent);
        }

        public static EffectBase Spawn(this object sender, EffectsId id, Vector3 position, Quaternion rotation)
        {
            return FXService.Spawn(id, position, rotation, null);
        }

        #endregion
    }
}