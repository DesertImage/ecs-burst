using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct MatcherBuilder
    {
        private static ushort _matcherIdCounter;

        private UnsafeList<uint> _all;
        private UnsafeList<uint> _none;
        private UnsafeList<uint> _any;

        private MatcherBuilder(UnsafeList<uint> all, UnsafeList<uint> none, UnsafeList<uint> any)
        {
            _all = all;
            _none = none;
            _any = any;
        }

        public static MatcherBuilder Create()
        {
            //TODO:pool lists
            return new MatcherBuilder
            (
                new UnsafeList<uint>(10, Allocator.Persistent, default),
                new UnsafeList<uint>(10, Allocator.Persistent, default),
                new UnsafeList<uint>(10, Allocator.Persistent, default)
            );
        }

        public Matcher Build()
        {
            var matcher = new Matcher(++_matcherIdCounter, _all, _none, _any);

            return matcher;
        }

        public MatcherBuilder With<T>() where T : struct
        {
            _all.Add(ComponentTools.GetComponentId<T>());
            return this;
        }

        public MatcherBuilder None<T>() where T : struct
        {
            _none.Add(ComponentTools.GetComponentId<T>());
            return this;
        }

        #region AllOf

        public MatcherBuilder AllOf<T1, T2>() where T1 : struct where T2 : struct
        {
            _all.Add(ComponentTools.GetComponentId<T1>());
            _all.Add(ComponentTools.GetComponentId<T2>());

            _all.Add(ComponentTools.GetComponentId<T1>());
            _all.Add(ComponentTools.GetComponentId<T2>());

            return this;
        }

        public MatcherBuilder AllOf<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
        {
            AllOf<T1, T2>();
            _all.Add(ComponentTools.GetComponentId<T3>());

            return this;
        }

        public MatcherBuilder AllOf<T1, T2, T3, T4>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
        {
            AllOf<T1, T2, T3>();
            _all.Add(ComponentTools.GetComponentId<T4>());
            return this;
        }

        public MatcherBuilder AllOf<T1, T2, T3, T4, T5>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
        {
            AllOf<T1, T2, T3, T4>();
            _all.Add(ComponentTools.GetComponentId<T5>());
            return this;
        }

        public MatcherBuilder AllOf<T1, T2, T3, T4, T5, T6>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
        {
            AllOf<T1, T2, T3, T4, T5>();
            _all.Add(ComponentTools.GetComponentId<T6>());
            return this;
        }

        #endregion

        #region NoneOf

        public MatcherBuilder NoneOf<T1, T2>() where T1 : struct where T2 : struct
        {
            _none.Add(ComponentTools.GetComponentId<T1>());
            _none.Add(ComponentTools.GetComponentId<T2>());
            return this;
        }

        public MatcherBuilder NoneOf<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
        {
            NoneOf<T1, T2>();
            _none.Add(ComponentTools.GetComponentId<T3>());
            return this;
        }

        public MatcherBuilder NoneOf<T1, T2, T3, T4>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
        {
            NoneOf<T1, T2, T3>();
            _none.Add(ComponentTools.GetComponentId<T4>());
            return this;
        }

        public MatcherBuilder NoneOf<T1, T2, T3, T4, T5>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
        {
            NoneOf<T1, T2, T3, T4>();
            _none.Add(ComponentTools.GetComponentId<T5>());
            return this;
        }

        public MatcherBuilder NoneOf<T1, T2, T3, T4, T5, T6>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
        {
            NoneOf<T1, T2, T3, T4, T5>();
            _none.Add(ComponentTools.GetComponentId<T6>());
            return this;
        }

        #endregion

        #region AnyOf

        public MatcherBuilder AnyOf<T1, T2>() where T1 : struct where T2 : struct
        {
            _any.Add(ComponentTools.GetComponentId<T1>());
            _any.Add(ComponentTools.GetComponentId<T2>());
            return this;
        }

        public MatcherBuilder AnyOf<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
        {
            AnyOf<T1, T2>();
            _any.Add(ComponentTools.GetComponentId<T3>());
            return this;
        }

        public MatcherBuilder AnyOf<T1, T2, T3, T4>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
        {
            AnyOf<T1, T2, T3>();
            _any.Add(ComponentTools.GetComponentId<T4>());
            return this;
        }

        public MatcherBuilder AnyOf<T1, T2, T3, T4, T5>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
        {
            AnyOf<T1, T2, T3, T4>();
            _any.Add(ComponentTools.GetComponentId<T5>());
            return this;
        }

        public MatcherBuilder AnyOf<T1, T2, T3, T4, T5, T6>() where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            where T5 : struct
            where T6 : struct
        {
            AnyOf<T1, T2, T3, T4, T5>();
            _any.Add(ComponentTools.GetComponentId<T6>());
            return this;
        }

        #endregion
    }
}