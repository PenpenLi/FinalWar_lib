using System.Collections.Generic;
using System;

namespace superEvent
{
    internal class SuperEventListenerV
    {
        internal delegate void EventCallBack<T>(SuperEvent e, ref T _value) where T : struct;

        private class SuperEventListenerUnitBase
        {
            internal int index;
            internal string eventName;
            internal Delegate callBack;
        }

        private class SuperEventListenerUnit<T> : SuperEventListenerUnitBase where T : struct
        {
            internal SuperEventListenerUnit(int _index, string _eventName, EventCallBack<T> _callBack)
            {
                index = _index;
                eventName = _eventName;
                callBack = _callBack;
            }
        }

        private Dictionary<int, SuperEventListenerUnitBase> dicWithID = new Dictionary<int, SuperEventListenerUnitBase>();
        private Dictionary<string, Dictionary<Delegate, SuperEventListenerUnitBase>> dicWithEvent = new Dictionary<string, Dictionary<Delegate, SuperEventListenerUnitBase>>();

        private int nowIndex;

        internal int AddListener<T>(string _eventName, EventCallBack<T> _callBack) where T : struct
        {
            SuperEventListenerUnit<T> unit = new SuperEventListenerUnit<T>(nowIndex, _eventName, _callBack);

            nowIndex++;

            dicWithID.Add(unit.index, unit);

            Dictionary<Delegate, SuperEventListenerUnitBase> dic;

            if (dicWithEvent.ContainsKey(_eventName))
            {
                dic = dicWithEvent[_eventName];
            }
            else
            {
                dic = new Dictionary<Delegate, SuperEventListenerUnitBase>();

                dicWithEvent.Add(_eventName, dic);
            }

            dic.Add(_callBack, unit);

            return unit.index;
        }

        internal void RemoveListener(int _index)
        {
            if (dicWithID.ContainsKey(_index))
            {
                SuperEventListenerUnitBase unit = dicWithID[_index];

                dicWithID.Remove(_index);

                Dictionary<Delegate, SuperEventListenerUnitBase> dic = dicWithEvent[unit.eventName];

                dic.Remove(unit.callBack);

                if (dic.Count == 0)
                {
                    dicWithEvent.Remove(unit.eventName);
                }
            }
        }

        internal void RemoveListener<T>(string _eventName, EventCallBack<T> _callBack) where T : struct
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnitBase> dic = dicWithEvent[_eventName];

                if (dic.ContainsKey(_callBack))
                {
                    SuperEventListenerUnitBase unit = dic[_callBack];

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
                Dictionary<Delegate, SuperEventListenerUnitBase> dic = dicWithEvent[_eventName];

                KeyValuePair<Delegate, SuperEvent>[] arr = new KeyValuePair<Delegate, SuperEvent>[dic.Count];

                Dictionary<Delegate, SuperEventListenerUnitBase>.Enumerator enumerator = dic.GetEnumerator();

                int i = 0;

                while (enumerator.MoveNext())
                {
                    KeyValuePair<Delegate, SuperEventListenerUnitBase> pair = enumerator.Current;

                    SuperEvent ev = new SuperEvent(pair.Value.index, _objs);

                    arr[i] = new KeyValuePair<Delegate, SuperEvent>(pair.Key, ev);

                    i++;
                }

                for (i = 0; i < arr.Length; i++)
                {
                    KeyValuePair<Delegate, SuperEvent> pair = arr[i];

                    if(pair.Key is EventCallBack<T>)
                    {
                        EventCallBack<T> callBack = pair.Key as EventCallBack<T>;

                        callBack(pair.Value, ref _value);
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



