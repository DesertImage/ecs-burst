﻿using System.Collections.Generic;using DesertImage.ECS;using DesertImage.Pools;using Framework.FX;using UnityEngine;namespace DesertImage{    [CreateAssetMenu(fileName = "FXLibrary", menuName = "Factories/FX Library")]    public class ScriptableFXLibrary : ScriptableObject, IAwake, IInitial    {        public List<FXSpawnNode> Nodes;        private PoolMonoBehaviour<EffectBase> _effectPool;        public void OnAwake()        {            var factoryFx = Core.Instance.Get<ServiceFx>();            foreach (var fxSpawnNode in Nodes)            {                factoryFx.Register(fxSpawnNode);            }        }    }}