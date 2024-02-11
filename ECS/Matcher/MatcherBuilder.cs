using Unity.Collections;

namespace DesertImage.ECS
{
    public struct MatcherBuilder
    {
        private static int _matcherIdCounter;

        private UnsafeList<int> _all;
        private UnsafeList<int> _none;
        private UnsafeList<int> _any;

        private int _allCounter;
        private int _noneCounter;
        private int _anyCounter;

        private MatcherBuilder(UnsafeList<int> all, UnsafeList<int> none, UnsafeList<int> any) : this()
        {
            _all = all;
            _none = none;
            _any = any;
        }

        public static MatcherBuilder Create()
        {
            return new MatcherBuilder
            (
                new UnsafeList<int>(10, Allocator.Persistent),
                new UnsafeList<int>(10, Allocator.Persistent),
                new UnsafeList<int>(10, Allocator.Persistent)
            );
        }

        public Matcher Build()
        {
            var matcher = new Matcher(++_matcherIdCounter, _all, _none, _any);

            _all.Dispose();
            _any.Dispose();
            _none.Dispose();

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