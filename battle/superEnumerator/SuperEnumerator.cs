using System.Collections.Generic;
using System.Collections;

namespace superEnumerator
{
    public class SuperEnumerator<T>
    {
        private List<IEnumerator> list;

        public T Current { private set; get; }

        public SuperEnumerator(IEnumerator _ie)
        {
            list = new List<IEnumerator>();

            list.Add(_ie);
        }

        public bool MoveNext()
        {
            if (list.Count == 0)
            {
                return false;
            }

            IEnumerator ie = list[list.Count - 1];

            if (ie.MoveNext())
            {
                if (ie.Current is IEnumerator)
                {
                    list.Add(ie.Current as IEnumerator);

                    return MoveNext();
                }
                else
                {
                    Current = (T)ie.Current;

                    return true;
                }
            }
            else
            {
                list.RemoveAt(list.Count - 1);

                return MoveNext();
            }
        }

        public void Done()
        {
            while (MoveNext())
            {

            }
        }
    }
}
