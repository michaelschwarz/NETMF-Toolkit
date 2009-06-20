#if(MF)
using System;

namespace System.Collections
{
    public abstract class CollectionBase : IList, ICollection, IEnumerable
    {
        private ArrayList list;

        public int Count { get { return InnerList.Count; } }

        public IEnumerator GetEnumerator() { return InnerList.GetEnumerator(); }

        public void Clear()
        {
            OnClear();
            InnerList.Clear();
            OnClearComplete();
        }

        public void RemoveAt(int index)
        {
            object objectToRemove;
            objectToRemove = InnerList[index];
            OnValidate(objectToRemove);
            OnRemove(index, objectToRemove);
            InnerList.RemoveAt(index);
            OnRemoveComplete(index, objectToRemove);
        }

        protected CollectionBase()
        {
        }

        //protected CollectionBase(int capacity)
        //{
        //    list = new ArrayList(capacity);
        //}

        public int Capacity
        {
            get
            {
                if (list == null)
                    list = new ArrayList();

                return list.Capacity;
            }

            set
            {
                if (list == null)
                    list = new ArrayList();

                list.Capacity = value;
            }
        }

        protected ArrayList InnerList
        {
            get
            {
                if (list == null)
                    list = new ArrayList();
                return list;
            }
        }

        protected IList List { get { return this; } }

        protected virtual void OnClear() { }
        protected virtual void OnClearComplete() { }

        protected virtual void OnInsert(int index, object value) { }
        protected virtual void OnInsertComplete(int index, object value) { }

        protected virtual void OnRemove(int index, object value) { }
        protected virtual void OnRemoveComplete(int index, object value) { }

        protected virtual void OnSet(int index, object oldValue, object newValue) { }
        protected virtual void OnSetComplete(int index, object oldValue, object newValue) { }

        protected virtual void OnValidate(object value)
        {
            if (null == value)
            {
                throw new System.ArgumentNullException("CollectionBase.OnValidate: Invalid parameter value passed to method: null");
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            InnerList.CopyTo(array, index);
        }

        object ICollection.SyncRoot
        {
            get { return InnerList.SyncRoot; }
        }
        
        bool ICollection.IsSynchronized
        {
            get { return InnerList.IsSynchronized; }
        }

        int IList.Add(object value)
        {
            int newPosition;
            OnValidate(value);
            newPosition = InnerList.Count;
            OnInsert(newPosition, value);
            InnerList.Add(value);
        
            try
            {
                OnInsertComplete(newPosition, value);
            }
            catch
            {
                InnerList.RemoveAt(newPosition);
                throw;
            }

            return newPosition;
        }

        bool IList.Contains(object value)
        {
            return InnerList.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return InnerList.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            OnValidate(value);
            OnInsert(index, value);
            InnerList.Insert(index, value);
            
            try
            {
                OnInsertComplete(index, value);
            }
            catch
            {
                InnerList.RemoveAt(index);
                throw;
            }
        }

        void IList.Remove(object value)
        {
            int removeIndex;
            OnValidate(value);
            removeIndex = InnerList.IndexOf(value);
            
            if (removeIndex == -1)
                throw new ArgumentException("The element cannot be found.", "value");
            
            OnRemove(removeIndex, value);
            InnerList.Remove(value);
            OnRemoveComplete(removeIndex, value);
        }

        bool IList.IsFixedSize
        {
            get { return InnerList.IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return InnerList.IsReadOnly; }
        }

        object IList.this[int index]
        {
            get { return InnerList[index]; }
            set
            {
                if (index < 0 || index >= InnerList.Count)
                    throw new ArgumentOutOfRangeException("index");

                object oldValue;
        
                OnValidate(value);
                
                oldValue = InnerList[index];

                OnSet(index, oldValue, value);
                InnerList[index] = value;
      
                try
                {
                    OnSetComplete(index, oldValue, value);
                }
                catch
                {
                    InnerList[index] = oldValue;
                    throw;
                }
            }
        }
    }
}
#endif