using System;
using System.Collections.Generic;

namespace superEvent
{
    internal class SuperEventListener
    {
        private class SuperEventListenerUnit
        {
            internal int index;
            internal string eventName;
            internal Action<SuperEvent> callBack;

            internal SuperEventListenerUnit(int _index, string _eventName, Action<SuperEvent> _callBack)
            {
                index = _index;
                eventName = _eventName;
                callBack = _callBack;
            }
        }

        private Dictionary<int, SuperEventListenerUnit> dicWithID = new Dictionary<int, SuperEventListenerUnit>();
        private Dictionary<string, Dictionary<Action<SuperEvent>, SuperEventListenerUnit>> dicWithEvent = new Dictionary<string, Dictionary<Action<SuperEvent>, SuperEventListenerUnit>>();

        private int nowIndex;

        internal int AddListener(string _eventName, Action<SuperEvent> _callBack)
        {
            SuperEventListenerUnit unit = new SuperEventListenerUnit(nowIndex, _eventName, _callBack);

            nowIndex++;

            dicWithID.Add(unit.index, unit);

            Dictionary<Action<SuperEvent>, SuperEventListenerUnit> dic;

            if (dicWithEvent.ContainsKey(_eventName))
            {
                dic = dicWithEvent[_eventName];
            }
            else
            {
                dic = new Dictionary<Action<SuperEvent>, SuperEventListenerUnit>();

                dicWithEvent.Add(_eventName, dic);
            }

            dic.Add(_callBack, unit);

            return unit.index;
        }

        internal void RemoveListener(int _index)
        {
            if (dicWithID.ContainsKey(_index))
            {
                SuperEventListenerUnit unit = dicWithID[_index];

                dicWithID.Remove(_index);

                Dictionary<Action<SuperEvent>, SuperEventListenerUnit> dic = dicWithEvent[unit.eventName];

                dic.Remove(unit.callBack);

                if (dic.Count == 0)
                {
                    dicWithEvent.Remove(unit.eventName);
                }
            }
        }

        internal void RemoveListener(string _eventName, Action<SuperEvent> _callBack)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Action<SuperEvent>, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                if (dic.ContainsKey(_callBack))
                {
                    SuperEventListenerUnit unit = dic[_callBack];

                    dicWithID.Remove(unit.index);

                    dic.Remove(_callBack);

                    if (dic.Count == 0)
                    {
                        dicWithEvent.Remove(_eventName);
                    }
                }
            }
        }

        internal void DispatchEvent(string _eventName,params object[] _objs)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Action<SuperEvent>, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<Action<SuperEvent>, SuperEvent>[] arr = new KeyValuePair<Action<SuperEvent>, SuperEvent>[dic.Count];

                Dictionary<Action<SuperEvent>, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int i = 0;

                while (enumerator.MoveNext())
                {
                    KeyValuePair<Action<SuperEvent>, SuperEventListenerUnit> pair = enumerator.Current;

                    SuperEvent ev = new SuperEvent(pair.Value.index, _objs);

                    arr[i] = new KeyValuePair<Action<SuperEvent>, SuperEvent>(pair.Key, ev);

                    i++;
                }

                for (i = 0; i < arr.Length; i++)
                {
                    KeyValuePair<Action<SuperEvent>, SuperEvent> pair = arr[i];

                    pair.Key(pair.Value);
                }
            }
        }
    }
}



