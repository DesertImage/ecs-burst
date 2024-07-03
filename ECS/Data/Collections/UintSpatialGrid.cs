using System;
using Unity.Collections;
using UnityEngine;

namespace DesertImage.Collections
{
    public struct UintSpatialGrid : IDisposable
    {
        public UnsafeArray<UnsafeUintSparseSet<uint>> Values => _values;
        public int Width => _width;
        public int Height => _height;
        public float CellSizeKoef => _cellSizeKoef;

        private UnsafeArray<UnsafeUintSparseSet<uint>> _values;
        private readonly int _width;
        private readonly int _halfWidth;
        private readonly int _halfHeight;
        private readonly int _height;
        private readonly float _cellSizeKoef;

        public UintSpatialGrid(int width, int height, int cellSize, Allocator allocator)
        {
            _width = width;
            _height = height;
            _halfWidth = width / 2;
            _halfHeight = height / 2;
            _cellSizeKoef = 1f / cellSize;
            _values = new UnsafeArray<UnsafeUintSparseSet<uint>>(width * height, true, allocator);

            for (var i = -_halfWidth; i < _width - _halfWidth; i++)
            {
                for (var j = -_halfHeight; j < _height - _halfHeight; j++)
                {
                    var index = GetCellIndex(i, j);
                    _values[index] = new UnsafeUintSparseSet<uint>(5, allocator);
                }
            }
        }

        public void Add(uint element, Vector2 position)
        {
            var x = (int)(position.x * _cellSizeKoef);
            var y = (int)(position.y * _cellSizeKoef);

            var index = GetCellIndex(x, y);

            if (!IsCellExist(index)) return;

            _values.Get(index).Add(element, element);
        }

        public void Update(uint element, Vector2 oldPosition, Vector2 position)
        {
            var oldX = Mathf.FloorToInt(oldPosition.x * _cellSizeKoef);
            var oldY = Mathf.FloorToInt(oldPosition.y * _cellSizeKoef);

            var oldIndex = GetCellIndex(oldX, oldY);

            var newX = Mathf.FloorToInt(position.x * _cellSizeKoef);
            var newY = Mathf.FloorToInt(position.y * _cellSizeKoef);

            var newIndex = GetCellIndex(newX, newY);

            if (!IsCellExist(newIndex)) return;
            if (oldIndex == newIndex) return;

            if (IsPositionValid(oldX, oldY))
            {
#if DEBUG_MODE
                if (!IsCellExist(oldIndex)) throw new NullReferenceException();
#endif
                ref var values = ref _values.Get(oldIndex);
                if (values.Contains(element))
                {
                    values.Remove(element);
                }
            }

            if (!IsPositionValid(newX, newY)) return;

            _values.Get(newIndex).Add(element, element);
        }

        public unsafe UnsafeArray<uint> GetNeighbours(Vector2Int position, Allocator allocator = Allocator.Persistent)
        {
            var x = Mathf.FloorToInt(position.x * _cellSizeKoef);
            var y = Mathf.FloorToInt(position.y * _cellSizeKoef);

            var list = new UnsafeList<uint>(40, allocator);

            if (!IsPositionValid(x, y)) return list.ToUnsafeArray();

            var centerIndex = GetCellIndex(x, y);

            var values = _values[centerIndex];
            list.CopyFrom(values.Values, values.Count);

            var leftIndex = GetCellIndex(x - 1, y);
            var rightIndex = GetCellIndex(x + 1, y);
            var topIndex = GetCellIndex(x, y + 1);
            var bottomIndex = GetCellIndex(x, y - 1);

            if (IsCellExist(leftIndex))
            {
                values = _values[leftIndex];
                list.CopyFrom(values.Values, values.Count);
            }

            if (IsCellExist(rightIndex))
            {
                values = _values[rightIndex];
                list.CopyFrom(values.Values, values.Count);
            }

            if (IsCellExist(topIndex))
            {
                values = _values[topIndex];
                list.CopyFrom(values.Values, values.Count);
            }

            if (IsCellExist(bottomIndex))
            {
                values = _values[bottomIndex];
                list.CopyFrom(values.Values, values.Count);
            }

            return list.ToUnsafeArray();
        }

        private int GetCellIndex(int x, int y)
        {
            x += _halfWidth;
            y += _halfHeight;
            return x + y * _width;
        }

        private bool IsCellExist(int index)
        {
// #if DEBUG_MODE
            if (index < 0 || index >= _values.Length) return false;
// #endif
            return _values[index].IsNotNull;
        }

        private bool IsPositionValid(int x, int y)
        {
            return x > -_halfWidth && x < Width - _halfWidth && y > -_halfHeight && y < Height - _halfHeight;
        }

        public void DrawGizmos()
        {
            var cellSize = 1f / _cellSizeKoef;
            var halfCellSize = cellSize * .5f;

            var halfWidth = _width / 2;
            var halfHeight = _height / 2;

            for (var i = _width - halfWidth - 1; i >= -halfWidth; i--)
            {
                for (var j = _height - halfHeight - 1; j >= -halfHeight; j--)
                {
                    Gizmos.color = i == 0 || j == 0 ? Color.blue : Color.white;
                    Gizmos.color = _values[GetCellIndex(i, j)].Count > 0 ? Color.green : Gizmos.color;

                    var position = new Vector3(i * cellSize + halfCellSize, j * cellSize + halfCellSize, 0f);

                    Gizmos.DrawWireCube
                    (
                        position,
                        new Vector3(cellSize, cellSize, .01f)
                    );
                }
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < _values.Length; i++)
            {
                _values[i].Dispose();
            }

            _values.Dispose();
        }
    }
}