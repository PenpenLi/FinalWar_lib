using System.Collections.Generic;
using System.Collections;

namespace stepTools
{
    public class StepTools<T>
    {
        private LinkedList<IEnumerator> list;

        public bool isOver { private set; get; }

        public StepTools(IEnumerator _ie)
        {
            isOver = false;

            list = new LinkedList<IEnumerator>();

            list.AddLast(_ie);
        }

        public T Step()
        {
            if (isOver)
            {
                return default(T);
            }

            IEnumerator ie = list.Last.Value;

            while (ie.MoveNext())
            {
                if (ie.Current is IEnumerator)
                {
                    list.AddLast(ie.Current as IEnumerator);

                    return Step();
                }
                else
                {
                    return (T)ie.Current;
                }
            }

            list.RemoveLast();

            if (list.First == null)
            {
                isOver = true;
            }

            return Step();
        }

        public void Done()
        {
            while (!isOver)
            {
                Step();
            }
        }
    }
}
