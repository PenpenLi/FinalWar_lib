using System.Collections.Generic;

namespace superEvent
{
    internal class SuperEventListener
    {
        private class SuperEventListenerUnit
        {
            internal int index;
            internal string eventName;
            internal SuperFunctionCallBack callBack;
            internal int priority;

            internal SuperEventListenerUnit(int _index, string _eventName, SuperFunctionCallBack _callBack, int _priority)
            {
                index = _index;
                eventName = _eventName;
                callBack = _callBack;
                priority = _priority;
            }
        }

        internal delegate void SuperFunctionCallBack(int _index, object[] _datas);

        internal const int MAX_PRIORITY = 16;

        private Dictionary<int, SuperEventListenerUnit> dicWithID = new Dictionary<int, SuperEventListenerUnit>();
        private Dictionary<string, Dictionary<SuperFunctionCallBack, SuperEventListenerUnit>> dicWithEvent = new Dictionary<string, Dictionary<SuperFunctionCallBack, SuperEventListenerUnit>>();

        private int nowIndex;

        internal int AddListener(string _eventName, SuperFunctionCallBack _callBack)
        {
            return AddListener(_eventName, _callBack, 0);
        }

        internal int AddListener(string _eventName, SuperFunctionCallBack _callBack, int _priority)
        {
            SuperEventListenerUnit unit = new SuperEventListenerUnit(nowIndex, _eventName, _callBack, _priority);

            nowIndex++;

            dicWithID.Add(unit.index, unit);

            Dictionary<SuperFunctionCallBack, SuperEventListenerUnit> dic;

            if (dicWithEvent.ContainsKey(_eventName))
            {
                dic = dicWithEvent[_eventName];
            }
            else
            {
                dic = new Dictionary<SuperFunctionCallBack, SuperEventListenerUnit>();

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

                Dictionary<SuperFunctionCallBack, SuperEventListenerUnit> dic = dicWithEvent[unit.eventName];

                dic.Remove(unit.callBack);

                if (dic.Count == 0)
                {
                    dicWithEvent.Remove(unit.eventName);
                }
            }
        }

        internal void RemoveListener(string _eventName, SuperFunctionCallBack _callBack)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<SuperFunctionCallBack, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

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
                Dictionary<SuperFunctionCallBack, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                LinkedList<KeyValuePair<SuperFunctionCallBack, int>>[] arr = null;

                Dictionary<SuperFunctionCallBack, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    if (arr == null)
                    {
                        arr = new LinkedList<KeyValuePair<SuperFunctionCallBack, int>>[MAX_PRIORITY];
                    }

                    KeyValuePair<SuperFunctionCallBack, SuperEventListenerUnit> pair = enumerator.Current;

                    int priority = pair.Value.priority;

                    LinkedList<KeyValuePair<SuperFunctionCallBack, int>> list;

                    if (arr[priority] == null)
                    {
                        list = new LinkedList<KeyValuePair<SuperFunctionCallBack, int>>();

                        arr[priority] = list;
                    }
                    else
                    {
                        list = arr[priority];
                    }

                    list.AddLast(new KeyValuePair<SuperFunctionCallBack, int>(pair.Key, pair.Value.index));
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        LinkedList<KeyValuePair<SuperFunctionCallBack, int>> list = arr[i];

                        if (list != null)
                        {
                            LinkedList<KeyValuePair<SuperFunctionCallBack, int>>.Enumerator enumerator2 = list.GetEnumerator();

                            while (enumerator2.MoveNext())
                            {
                                KeyValuePair<SuperFunctionCallBack, int> pair = enumerator2.Current;

                                pair.Key(pair.Value, _objs);
                            }
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



