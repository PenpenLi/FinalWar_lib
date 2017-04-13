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
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBack, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBack)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBack, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBack, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBack, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBack, int>(pair.Key as SuperFunctionCallBack, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBack, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBack, int> pair = list[m];

                                SuperFunctionCallBack cb = pair.Key;

                                cb(pair.Value);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T1>(string _eventName, T1 t1)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBack1<T1>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBack1<T1>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBack1<T1>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBack1<T1>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBack1<T1>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBack1<T1>, int>(pair.Key as SuperFunctionCallBack1<T1>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBack1<T1>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBack1<T1>, int> pair = list[m];

                                SuperFunctionCallBack1<T1> cb = pair.Key;

                                cb(pair.Value, t1);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T1, T2>(string _eventName, T1 t1, T2 t2)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBack2<T1, T2>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBack2<T1, T2>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBack2<T1, T2>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBack2<T1, T2>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBack2<T1, T2>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBack2<T1, T2>, int>(pair.Key as SuperFunctionCallBack2<T1, T2>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBack2<T1, T2>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBack2<T1, T2>, int> pair = list[m];

                                SuperFunctionCallBack2<T1, T2> cb = pair.Key;

                                cb(pair.Value, t1, t2);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T1, T2, T3>(string _eventName, T1 t1, T2 t2, T3 t3)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBack3<T1, T2, T3>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBack3<T1, T2, T3>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBack3<T1, T2, T3>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBack3<T1, T2, T3>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBack3<T1, T2, T3>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBack3<T1, T2, T3>, int>(pair.Key as SuperFunctionCallBack3<T1, T2, T3>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBack3<T1, T2, T3>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBack3<T1, T2, T3>, int> pair = list[m];

                                SuperFunctionCallBack3<T1, T2, T3> cb = pair.Key;

                                cb(pair.Value, t1, t2, t3);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T1, T2, T3, T4>(string _eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBack4<T1, T2, T3, T4>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBack4<T1, T2, T3, T4>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBack4<T1, T2, T3, T4>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBack4<T1, T2, T3, T4>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBack4<T1, T2, T3, T4>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBack4<T1, T2, T3, T4>, int>(pair.Key as SuperFunctionCallBack4<T1, T2, T3, T4>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBack4<T1, T2, T3, T4>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBack4<T1, T2, T3, T4>, int> pair = list[m];

                                SuperFunctionCallBack4<T1, T2, T3, T4> cb = pair.Key;

                                cb(pair.Value, t1, t2, t3, t4);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T>(string _eventName, ref T t) where T : struct
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBackV<T>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBackV<T>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBackV<T>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBackV<T>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBackV<T>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBackV<T>, int>(pair.Key as SuperFunctionCallBackV<T>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBackV<T>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBackV<T>, int> pair = list[m];

                                SuperFunctionCallBackV<T> cb = pair.Key;

                                cb(pair.Value, ref t);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T, T1>(string _eventName, ref T t, T1 t1) where T : struct
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBackV1<T, T1>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBackV1<T, T1>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBackV1<T, T1>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBackV1<T, T1>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBackV1<T, T1>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBackV1<T, T1>, int>(pair.Key as SuperFunctionCallBackV1<T, T1>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBackV1<T, T1>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBackV1<T, T1>, int> pair = list[m];

                                SuperFunctionCallBackV1<T, T1> cb = pair.Key;

                                cb(pair.Value, ref t, t1);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T, T1, T2>(string _eventName, ref T t, T1 t1, T2 t2) where T : struct
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBackV2<T, T1, T2>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBackV2<T, T1, T2>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBackV2<T, T1, T2>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBackV2<T, T1, T2>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBackV2<T, T1, T2>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBackV2<T, T1, T2>, int>(pair.Key as SuperFunctionCallBackV2<T, T1, T2>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBackV2<T, T1, T2>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBackV2<T, T1, T2>, int> pair = list[m];

                                SuperFunctionCallBackV2<T, T1, T2> cb = pair.Key;

                                cb(pair.Value, ref t, t1, t2);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T, T1, T2, T3>(string _eventName, ref T t, T1 t1, T2 t2, T3 t3) where T : struct
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBackV3<T, T1, T2, T3>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBackV3<T, T1, T2, T3>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBackV3<T, T1, T2, T3>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBackV3<T, T1, T2, T3>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBackV3<T, T1, T2, T3>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBackV3<T, T1, T2, T3>, int>(pair.Key as SuperFunctionCallBackV3<T, T1, T2, T3>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBackV3<T, T1, T2, T3>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBackV3<T, T1, T2, T3>, int> pair = list[m];

                                SuperFunctionCallBackV3<T, T1, T2, T3> cb = pair.Key;

                                cb(pair.Value, ref t, t1, t2, t3);
                            }
                        }
                    }
                }
            }
        }

        internal void DispatchEvent<T, T1, T2, T3, T4>(string _eventName, ref T t, T1 t1, T2 t2, T3 t3, T4 t4) where T : struct
        {
            if (dicWithEvent.ContainsKey(_eventName))
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[_eventName];

                KeyValuePair<SuperFunctionCallBackV4<T, T1, T2, T3, T4>, int>[][] arr = null;

                Dictionary<Delegate, SuperEventListenerUnit>.Enumerator enumerator = dic.GetEnumerator();

                int arrIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is SuperFunctionCallBackV4<T, T1, T2, T3, T4>)
                    {
                        if (arr == null)
                        {
                            arr = new KeyValuePair<SuperFunctionCallBackV4<T, T1, T2, T3, T4>, int>[MAX_PRIORITY][];
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        KeyValuePair<SuperFunctionCallBackV4<T, T1, T2, T3, T4>, int>[] list;

                        if (arr[priority] == null)
                        {
                            list = new KeyValuePair<SuperFunctionCallBackV4<T, T1, T2, T3, T4>, int>[dic.Count];

                            arr[priority] = list;
                        }
                        else
                        {
                            list = arr[priority];
                        }

                        list[arrIndex] = new KeyValuePair<SuperFunctionCallBackV4<T, T1, T2, T3, T4>, int>(pair.Key as SuperFunctionCallBackV4<T, T1, T2, T3, T4>, pair.Value.index);
                    }

                    arrIndex++;
                }

                if (arr != null)
                {
                    for (int i = 0; i < MAX_PRIORITY; i++)
                    {
                        KeyValuePair<SuperFunctionCallBackV4<T, T1, T2, T3, T4>, int>[] list = arr[i];

                        if (list != null)
                        {
                            int length = list.Length;

                            for (int m = 0; m < length; m++)
                            {
                                KeyValuePair<SuperFunctionCallBackV4<T, T1, T2, T3, T4>, int> pair = list[m];

                                SuperFunctionCallBackV4<T, T1, T2, T3, T4> cb = pair.Key;

                                cb(pair.Value, ref t, t1, t2, t3, t4);
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



