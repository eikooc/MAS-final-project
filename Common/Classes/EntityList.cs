using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Classes
{
    public class EntityList<T> where T : IEntity
    {
        private Dictionary<int, T> entityById;
        private Dictionary<int, T> entityByPosition;
        private int maxCol;
        private int maxRow;

        public EntityList(int maxCol, int maxRow)
        {
            this.maxCol = maxCol;
            this.maxRow = maxRow;
            this.entityById = new Dictionary<int, T>();
            this.entityByPosition = new Dictionary<int, T>();
        }

        public int Count { get { return this.entityById.Count == this.entityByPosition.Count ? this.entityById.Count : -1; } }
        public IEnumerable<int> Ids { get { return this.entityById.Keys; } }
        // Slow
        public IEnumerable<dynamic> Positions { get { return this.entityById.Values.Select(x => new { Col = x.col, Row = x.row }); } }
        public IEnumerable<T> Entities { get { return this.entityById.Values; } }
        public T this[int index]
        {
            get
            {
                if (this.entityById.ContainsKey(index))
                {
                    return this.entityById[index];
                }
                return default(T);
            }
        }
        public T this[int col, int row]
        {
            get
            {
                int index = this.CalculateIndex(col, row);
                if (this.entityByPosition.ContainsKey(index))
                {
                    return this.entityByPosition[index];
                }
                return default(T);
            }
        }

        public bool Add(T entity)
        {
            int index = this.CalculateIndex(entity.col, entity.row);
            if (!this.entityByPosition.ContainsKey(index) && !this.entityById.ContainsKey(entity.uid))
            {
                this.entityByPosition.Add(index, entity);
                this.entityById.Add(entity.uid, entity);
                return true;
            }

            return false;
        }
        public bool Replace(T entity) // Replace
        {
            if (this.entityById.ContainsKey(entity.uid))
            {
                T _entity = this.entityById[entity.uid];
                int currentIndex = this.CalculateIndex(_entity.col, _entity.row);
                int newIndex = this.CalculateIndex(entity.col, entity.row);
                if (this.entityByPosition.ContainsKey(currentIndex))
                {
                    if (currentIndex != newIndex)
                    {
                        this.entityByPosition.Remove(currentIndex);
                        this.entityByPosition.Add(newIndex, default(T));
                    }
                    this.entityByPosition[newIndex] = entity;
                    this.entityById[entity.uid] = entity;
                    return true;
                }
            }

            return false;
        }
        public bool UpdatePosition(int currentCol, int currentRow, int newCol, int newRow)
        {
            int currentIndex = this.CalculateIndex(currentCol, currentRow);
            if (this.entityByPosition.ContainsKey(currentIndex))
            {
                T _entity = this.entityByPosition[currentIndex];
                _entity.col = newCol;
                _entity.row = newRow;
                int newIndex = this.CalculateIndex(newCol, newRow);
                this.entityByPosition.Remove(currentIndex);
                this.entityByPosition.Add(newIndex, _entity);
                return true;
            }

            return false;
        }
        public bool UpdatePosition(int id, int newCol, int newRow)
        {
            if (this.entityById.ContainsKey(id))
            {
                T _entity = this.entityById[id];
                int currentIndex = this.CalculateIndex(_entity.col, _entity.row);
                _entity.col = newCol;
                _entity.row = newRow;
                int newIndex = this.CalculateIndex(newCol, newRow);
                this.entityByPosition.Remove(currentIndex);
                this.entityByPosition.Add(newIndex, _entity);
                return true;
            }

            return false;
        }
        public EntityList<T> Clone()
        {
            EntityList<T> clone = new EntityList<T>(this.maxCol, this.maxRow);
            foreach (T entity in this.Entities)
            {
                clone.Add((T)entity.Clone());
            }
            return clone;
        }
        public bool Remove(int col, int row)
        {
            int index = this.CalculateIndex(col, row);
            if (this.entityByPosition.ContainsKey(index))
            {
                T entity = this.entityByPosition[index];
                if (this.entityById.ContainsKey(entity.uid))
                {
                    this.entityByPosition.Remove(index);
                    this.entityById.Remove(entity.uid);
                    return true;
                }
            }
            return false;
        }
        public bool Remove(int id)
        {
            if (this.entityById.ContainsKey(id))
            {
                T entity = this.entityById[id];
                int index = this.CalculateIndex(entity.col, entity.row);
                if (this.entityByPosition.ContainsKey(index))
                {
                    this.entityByPosition.Remove(index);
                    this.entityById.Remove(entity.uid);
                    return true;
                }
            }
            return false;
        }

        private int CalculateIndex(int col, int row)
        {
            return (row * this.maxCol) + col;
        }
    }
}
