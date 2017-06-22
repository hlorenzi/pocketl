using System;
using System.Collections.Generic;


namespace pocketl
{
    public class H<T> where T: class
    {
        public uint id;
    }


    public class HandleManager<T> where T: class
    {
        uint nextId;
        Dictionary<uint, T> items = new Dictionary<uint, T>();


        public H<T> Reserve()
        {
            var handle = new H<T> { id = this.nextId };
            this.nextId++;

            this.items.Add(handle.id, null);
            return handle;
        }


        public void Remove(H<T> handle)
        {
            this.items.Remove(handle.id);
        }


        public T this[H<T> handle]
        {
            get
            {
                return this.items[handle.id];
            }

            set
            {
                this.items[handle.id] = value;
            }
        }



        public H<T> Add(T item)
        {
            var handle = this.Reserve();
            this[handle] = item;
            return handle;
        }
    }
}
