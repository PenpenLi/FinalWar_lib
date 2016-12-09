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
            internal int priority;

            internal SuperEventListenerUnit(int _index, string _eventName, Action<SuperEvent> _callBack, int _priority)
            {
                index = _index;
                eventName = _eventName;
                callBack = _callBack;
                priority = _priority;
            }
        }

        internal const int MAX_PRIORITY = 16;

        private Dictionary<int, SuperEventListenerUnit> dicWithID = new Dictionary<int, SuperEventListenerUnit>();
        private Dictionary<string, Dictionary<Action<SuperEvent>, SuperEventListenerUnit>> dicWithEvent = new Dictionary<string, Dictionary<Action<SuperEvent>, SuperEventListenerUnit>>();

        private int nowIndex;

        internal int AddListener(string _eventName, Action<SuperEvent> _callBack)
        {
            return AddListener(_eventName, _callBack, 0);
        }

        internal int AddListener(string _eventName, Action<SuperEvent> _callBack, int _priority)
        {
            SuperEventListenerUnit unit = new SuperEventListenerUnit(nowIndex, _eventName, _callBack, _priority);

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

        internal void DispatchEvent(string _eventName, params object[] _objs)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Action<SuperEvent>, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                Queue<KeyValuePair<Action<SuperEvent>, SuperEvent>>[] arr = new Queue<KeyValuePair<Action<SuperEvent>, SuperEvent>>[MAX_PRIORITY];

                Dictionary<Action<SuperEvent>, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    KeyValuePair<Action<SuperEvent>, SuperEventListenerUnit> pair = enumerator.Current;

                    SuperEvent ev = new SuperEvent(pair.Value.index, _objs);

                    int priority = pair.Value.priority;

                    Queue<KeyValuePair<Action<SuperEvent>, SuperEvent>> queue;

                    if (arr[priority] == null)
                    {
                        queue = new Queue<KeyValuePair<Action<SuperEvent>, SuperEvent>>();

                        arr[priority] = queue;
                    }
                    else
                    {
                        queue = arr[priority];
                    }

                    queue.Enqueue(new KeyValuePair<Action<SuperEvent>, SuperEvent>(pair.Key, ev));
                }

                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    Queue<KeyValuePair<Action<SuperEvent>, SuperEvent>> queue = arr[i];

                    if (queue != null)
                    {
                        Queue<KeyValuePair<Action<SuperEvent>, SuperEvent>>.Enumerator enumerator2 = queue.GetEnumerator();

                        while (enumerator2.MoveNext())
                        {
                            KeyValuePair<Action<SuperEvent>, SuperEvent> pair = enumerator2.Current;

                            pair.Key(pair.Value);
                        }
                    }
                }
            }
        }

        internal void Clear()
        {
            dicWithID.Clear();
            dicWithEvent.Clear();
        }

        internal void LogNum()
        {
            Log.Write("SuperEventListener:" + dicWithID.Count);
        }
    }
}



