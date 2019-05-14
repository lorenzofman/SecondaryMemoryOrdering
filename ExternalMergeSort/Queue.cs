/// <summary>
/// Generic Abstraction of C# queue
/// </summary>
namespace ExternalMergeSort
{
    public class Queue<T>
    {
        private System.Collections.Queue queue;
        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);
        }
        public T Peek()
        {
            return (T)queue.Peek();
        }
        public T Dequeue()
        {
            return (T)queue.Dequeue();
        }
        public int Count
        {
            get
            {
                return queue.Count;
            }
        }
        public Queue()
        {
            queue = new System.Collections.Queue();
        }
    }
}
