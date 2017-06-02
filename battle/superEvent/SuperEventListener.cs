using System.Collections.Generic;
using System;

namespace superEvent
{
    internal class SuperEventListener
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

        internal delegate void SuperFunctionCallBack(int _index);
        internal delegate void SuperFunctionCallBack1<T1>(int _index, T1 t1);
        internal delegate void SuperFunctionCallBack2<T1, T2>(int _index, T1 t1, T2 t2);
        internal delegate void SuperFunctionCallBack3<T1, T2, T3>(int _index, T1 t1, T2 t2, T3 t3);
        internal delegate void SuperFunctionCallBack4<T1, T2, T3, T4>(int _index, T1 t1, T2 t2, T3 t3, T4 t4);

        internal delegate void SuperFunctionCallBackV<T>(int _index, ref T t) where T : struct;
        internal delegate void SuperFunctionCallBackV1<T, T1>(int _index, ref T t, T1 t1) where T : struct;
        internal delegate void SuperFunctionCallBackV2<T, T1, T2>(int _index, ref T t, T1 t1, T2 t2) where T : struct;
        internal delegate void SuperFunctionCallBackV3<T, T1, T2, T3>(int _index, ref T t, T1 t1, T2 t2, T3 t3) where T : struct;
        internal delegate void SuperFunctionCallBackV4<T, T1, T2, T3, T4>(int _index, ref T t, T1 t1, T2 t2, T3 t3, T4 t4) where T : struct;

        internal const int MAX_PRIORITY = 16;

        private Dictionary<int, SuperEventListenerUnit> dicWithID = new Dictionary<int, SuperEventListenerUnit>();
        private Dictionary<string, Dictionary<Delegate, SuperEventListenerUnit>> dicWithEvent = new Dictionary<string, Dictionary<Delegate, SuperEventListenerUnit>>();

        private int nowIndex;

        internal int AddListener(string _eventName, SuperFunctionCallBack _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener(string _eventName, SuperFunctionCallBack _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T1>(string _eventName, SuperFunctionCallBack1<T1> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T1>(string _eventName, SuperFunctionCallBack1<T1> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T1, T2>(string _eventName, SuperFunctionCallBack2<T1, T2> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T1, T2>(string _eventName, SuperFunctionCallBack2<T1, T2> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T1, T2, T3>(string _eventName, SuperFunctionCallBack3<T1, T2, T3> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T1, T2, T3>(string _eventName, SuperFunctionCallBack3<T1, T2, T3> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T1, T2, T3, T4>(string _eventName, SuperFunctionCallBack4<T1, T2, T3, T4> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T1, T2, T3, T4>(string _eventName, SuperFunctionCallBack4<T1, T2, T3, T4> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack, int _priority) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T, T1>(string _eventName, SuperFunctionCallBackV1<T, T1> _callBack) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T, T1>(string _eventName, SuperFunctionCallBackV1<T, T1> _callBack, int _priority) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T, T1, T2>(string _eventName, SuperFunctionCallBackV2<T, T1, T2> _callBack) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T, T1, T2>(string _eventName, SuperFunctionCallBackV2<T, T1, T2> _callBack, int _priority) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T, T1, T2, T3>(string _eventName, SuperFunctionCallBackV3<T, T1, T2, T3> _callBack) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T, T1, T2, T3>(string _eventName, SuperFunctionCallBackV3<T, T1, T2, T3> _callBack, int _priority) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T, T1, T2, T3, T4>(string _eventName, SuperFunctionCallBackV4<T, T1, T2, T3, T4> _callBack) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T, T1, T2, T3, T4>(string _eventName, SuperFunctionCallBackV4<T, T1, T2, T3, T4> _callBack, int _priority) where T : struct
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListenerReal(string _eventName, Delegate _callBack, int _priority)
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

        internal void RemoveListener(string _eventName, SuperFunctionCallBack _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T1>(string _eventName, SuperFunctionCallBack1<T1> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T1, T2>(string _eventName, SuperFunctionCallBack2<T1, T2> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T1, T2, T3>(string _eventName, SuperFunctionCallBack3<T1, T2, T3> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T1, T2, T3, T4>(string _eventName, SuperFunctionCallBack4<T1, T2, T3, T4> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack) where T : struct
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T, T1>(string _eventName, SuperFunctionCallBackV1<T, T1> _callBack) where T : struct
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T, T1, T2>(string _eventName, SuperFunctionCallBackV2<T, T1, T2> _callBack) where T : struct
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T, T1, T2, T3>(string _eventName, SuperFunctionCallBackV3<T, T1, T2, T3> _callBack) where T : struct
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T, T1, T2, T3, T4>(string _eventName, SuperFunctionCallBackV4<T, T1, T2, T3, T4> _callBack) where T : struct
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        private void RemoveListenerReal(string _eventName, Delegate _callBack)
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

        internal void DispatchEvent(string _eventName)
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBack>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBack cb = unit.callBack as SuperFunctionCallBack;

                            cb(unit.index);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T1>(string _eventName, T1 t1)
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBack1<T1>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBack1<T1> cb = unit.callBack as SuperFunctionCallBack1<T1>;

                            cb(unit.index, t1);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T1, T2>(string _eventName, T1 t1, T2 t2)
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBack2<T1, T2>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBack2<T1, T2> cb = unit.callBack as SuperFunctionCallBack2<T1, T2>;

                            cb(unit.index, t1, t2);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T1, T2, T3>(string _eventName, T1 t1, T2 t2, T3 t3)
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBack3<T1, T2, T3>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBack3<T1, T2, T3> cb = unit.callBack as SuperFunctionCallBack3<T1, T2, T3>;

                            cb(unit.index, t1, t2, t3);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T1, T2, T3, T4>(string _eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBack4<T1, T2, T3, T4>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBack4<T1, T2, T3, T4> cb = unit.callBack as SuperFunctionCallBack4<T1, T2, T3, T4>;

                            cb(unit.index, t1, t2, t3, t4);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T>(string _eventName, ref T t) where T : struct
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBackV<T>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBackV<T> cb = unit.callBack as SuperFunctionCallBackV<T>;

                            cb(unit.index, ref t);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T, T1>(string _eventName, ref T t, T1 t1) where T : struct
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBackV1<T, T1>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBackV1<T, T1> cb = unit.callBack as SuperFunctionCallBackV1<T, T1>;

                            cb(unit.index, ref t, t1);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T, T1, T2>(string _eventName, ref T t, T1 t1, T2 t2) where T : struct
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBackV2<T, T1, T2>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBackV2<T, T1, T2> cb = unit.callBack as SuperFunctionCallBackV2<T, T1, T2>;

                            cb(unit.index, ref t, t1, t2);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T, T1, T2, T3>(string _eventName, ref T t, T1 t1, T2 t2, T3 t3) where T : struct
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBackV3<T, T1, T2, T3>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBackV3<T, T1, T2, T3> cb = unit.callBack as SuperFunctionCallBackV3<T, T1, T2, T3>;

                            cb(unit.index, ref t, t1, t2, t3);
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T, T1, T2, T3, T4>(string _eventName, ref T t, T1 t1, T2 t2, T3 t3, T4 t4) where T : struct
        {
            List<SuperEventListenerUnit>[] arr = DispatchEventReal<SuperFunctionCallBackV4<T, T1, T2, T3, T4>>(_eventName);

            if (arr != null)
            {
                for (int i = 0; i < MAX_PRIORITY; i++)
                {
                    List<SuperEventListenerUnit> list = arr[i];

                    if (list != null)
                    {
                        for (int m = 0; m < list.Count; m++)
                        {
                            SuperEventListenerUnit unit = list[m];

                            SuperFunctionCallBackV4<T, T1, T2, T3, T4> cb = unit.callBack as SuperFunctionCallBackV4<T, T1, T2, T3, T4>;

                            cb(unit.index, ref t, t1, t2, t3, t4);
                        }
                    }
                }
            }
        }

        private List<SuperEventListenerUnit>[] DispatchEventReal<T>(string _eventName)
        {
            List<SuperEventListenerUnit>[] arr = null;

            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is T)
                    {
                        if (arr == null)
                        {
                            arr = new List<SuperEventListenerUnit>[MAX_PRIORITY];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        List<SuperEventListenerUnit> list;

                        if (arr[priority] == null)
                        {
                            list = new List<SuperEventListenerUnit>();

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list.Add(enumerator.Current.Value);
                    }
                }
            }

            return arr;
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



