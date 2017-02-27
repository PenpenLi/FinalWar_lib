using System.Collections.Generic;
using System;

namespace superEvent
{
    internal class SuperEventListenerV
    {
        private class SuperEventListenerUnit
        {
            internal int index;
            internal string eventName;
            internal Delegate callBack;
            internal int priority;

            internal SuperEventListenerUnit(int _index, string _eventName, Delegate _callBack, int _priority)
            {
                index = _index;
                eventName = _eventName;
                callBack = _callBack;
                priority = _priority;
            }
        }

        public delegate void SuperFunctionCallBackV<T>(int _index, ref T v, object[] _datas) where T : struct;

        private Dictionary<int, SuperEventListenerUnit> dicWithID = new Dictionary<int, SuperEventListenerUnit>();
        private Dictionary<string, Dictionary<Delegate, SuperEventListenerUnit>> dicWithEvent = new Dictionary<string, Dictionary<Delegate, SuperEventListenerUnit>>();

        private int nowIndex;

        internal int AddListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack) where T : struct
        {
            return AddListener(_eventName, _callBack, 0);
        }

        internal int AddListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack, int _priority) where T : struct
        {
            SuperEventListenerUnit unit = new SuperEventListenerUnit(nowIndex, _eventName, _callBack, _priority);

            nowIndex++;

            dicWithID.Add(unit.index, unit);

            Dictionary<Delegate, SuperEventListenerUnit> dic;

            if (dicWithEvent.ContainsKey(_eventName))
            {
                dic = dicWithEvent[_eventName];
            }
            else
            {
                dic = new Dictionary<Delegate, SuperEventListenerUnit>();

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

                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[unit.eventName];

                dic.Remove(unit.callBack);

                if (dic.Count == 0)
                {
                    dicWithEvent.Remove(unit.eventName);
                }
            }
        }

        internal void RemoveListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack) where T : struct
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

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

        internal void DispatchEvent<T>(string _eventName, ref T _value, params object[] _objs) where T : struct
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                LinkedList<KeyValuePair<SuperFunctionCallBackV<T>, int>>[] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                    if (pair.Key is SuperFunctionCallBackV<T>)
                    {
                        if (arr == null)
                        {
                            arr = new LinkedList<KeyValuePair<SuperFunctionCallBackV<T>, int>>[SuperEventListener.MAX_PRIORITY];
                        }

                        int priority = pair.Value.priority;

                        LinkedList<KeyValuePair<SuperFunctionCallBackV<T>, int>> list;

                        if (arr[priority] == null)
                        {
                            list = new LinkedList<KeyValuePair<SuperFunctionCallBackV<T>, int>>();

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list.AddLast(new KeyValuePair<SuperFunctionCallBackV<T>, int>(pair.Key as SuperFunctionCallBackV<T>, pair.Value.index));
                    }
                }

                if (arr != null)
                {
                    for (int i = 0; i < SuperEventListener.MAX_PRIORITY; i++)
                    {
                        LinkedList<KeyValuePair<SuperFunctionCallBackV<T>, int>> list = arr[i];

                        if (list != null)
                        {
                            LinkedList<KeyValuePair<SuperFunctionCallBackV<T>, int>>.Enumerator enumerator2 = list.GetEnumerator();

                            while (enumerator2.MoveNext())
                            {
                                KeyValuePair<SuperFunctionCallBackV<T>, int> pair = enumerator2.Current;

                                pair.Key(pair.Value, ref _value, _objs);
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
            Log.Write("SuperEventListenerV:" + dicWithID.Count);
        }
    }
}



