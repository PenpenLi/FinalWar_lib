using System.Collections.Generic;

namespace superEvent
{
    internal class SuperEventListenerV
    {
        internal delegate void EventCallBack(SuperEvent e, ref float _value);

        private class SuperEventListenerVUnit
        {
            internal int index;
            internal string eventName;
            internal EventCallBack callBack;

            internal SuperEventListenerVUnit(int _index, string _eventName, EventCallBack _callBack)
            {
                index = _index;
                eventName = _eventName;
                callBack = _callBack;
            }
        }

        private Dictionary<int, SuperEventListenerVUnit> dicWithID = new Dictionary<int, SuperEventListenerVUnit>();
        private Dictionary<string, Dictionary<EventCallBack, SuperEventListenerVUnit>> dicWithEvent = new Dictionary<string, Dictionary<EventCallBack, SuperEventListenerVUnit>>();

        private int nowIndex;

        internal int AddListener(string _eventName, EventCallBack _callBack)
        {
            SuperEventListenerVUnit unit = new SuperEventListenerVUnit(nowIndex, _eventName, _callBack);

            nowIndex++;

            dicWithID.Add(unit.index, unit);

            Dictionary<EventCallBack, SuperEventListenerVUnit> dic;

            if (dicWithEvent.ContainsKey(_eventName))
            {
                dic = dicWithEvent[_eventName];
            }
            else
            {
                dic = new Dictionary<EventCallBack, SuperEventListenerVUnit>();

                dicWithEvent.Add(_eventName, dic);
            }

            dic.Add(_callBack, unit);

            return unit.index;
        }

        internal void RemoveListener(int _index)
        {
            if (dicWithID.ContainsKey(_index))
            {
                SuperEventListenerVUnit unit = dicWithID[_index];

                dicWithID.Remove(_index);

                Dictionary<EventCallBack, SuperEventListenerVUnit> dic = dicWithEvent[unit.eventName];

                dic.Remove(unit.callBack);

                if (dic.Count == 0)
                {
                    dicWithEvent.Remove(unit.eventName);
                }
            }
        }

        internal void RemoveListener(string _eventName, EventCallBack _callBack)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<EventCallBack, SuperEventListenerVUnit> dic = dicWithEvent[_eventName];

                if (dic.ContainsKey(_callBack))
                {
                    SuperEventListenerVUnit unit = dic[_callBack];

                    dicWithID.Remove(unit.index);

                    dic.Remove(_callBack);

                    if (dic.Count == 0)
                    {
                        dicWithEvent.Remove(_eventName);
                    }
                }
            }
        }

        internal void DispatchEvent(string _eventName, ref float _value, params object[] _objs)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<EventCallBack, SuperEventListenerVUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<EventCallBack,SuperEvent>[] arr = new KeyValuePair<EventCallBack, SuperEvent>[dic.Count];

                Dictionary<EventCallBack, SuperEventListenerVUnit>.Enumerator enumerator = dic.GetEnumerator();

                int i = 0;

                while (enumerator.MoveNext())
                {
                    KeyValuePair<EventCallBack, SuperEventListenerVUnit> pair = enumerator.Current;

                    SuperEvent ev = new SuperEvent(pair.Value.index, _objs);

                    arr[i] = new KeyValuePair<EventCallBack, SuperEvent>(pair.Key, ev);

                    i++;
                }

                for (i = 0; i < arr.Length; i++)
                {
                    KeyValuePair<EventCallBack, SuperEvent> pair = arr[i];

                    pair.Key(pair.Value, ref _value);
                }
            }
        }
    }
}



