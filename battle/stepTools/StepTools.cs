using System.Collections.Generic;
using System.Collections;

namespace stepTools
{
    public class StepTools<T>
    {
        private List<IEnumerator> list;

        public bool isOver { private set; get; }

        public StepTools(IEnumerator _ie)
        {
            isOver = false;

            list = new List<IEnumerator>();

            list.Add(_ie);
        }

        public T Step()
        {
            if (isOver)
            {
                return default(T);
            }

            IEnumerator ie = list[list.Count - 1];

            while (ie.MoveNext())
            {
                if (ie.Current is IEnumerator)
                {
                    list.Add(ie.Current as IEnumerator);

                    return Step();
                }
                else
                {
                    return (T)ie.Current;
                }
            }

            list.RemoveAt(list.Count - 1);

            if (list.Count == 0)
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
