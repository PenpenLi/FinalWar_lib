using System.Collections.Generic;
using System;

namespace superEvent
{
    internal class SuperEventListener
    {
        private struct SuperEventListenerUnit
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

        internal delegate void SuperFunctionCallBackV<T>(int _index, ref T t);
        internal delegate void SuperFunctionCallBackV1<T, T1>(int _index, ref T t, T1 t1);
        internal delegate void SuperFunctionCallBackV2<T, T1, T2>(int _index, ref T t, T1 t1, T2 t2);
        internal delegate void SuperFunctionCallBackV3<T, T1, T2, T3>(int _index, ref T t, T1 t1, T2 t2, T3 t3);
        internal delegate void SuperFunctionCallBackV4<T, T1, T2, T3, T4>(int _index, ref T t, T1 t1, T2 t2, T3 t3, T4 t4);

        private Dictionary<int, SuperEventListenerUnit> dicWithID = new Dictionary<int, SuperEventListenerUnit>();
        private Dictionary<string, Dictionary<Delegate, SuperEventListenerUnit>> dicWithEvent = new Dictionary<string, Dictionary<Delegate, SuperEventListenerUnit>>();

        private Queue<LinkedList<SuperEventListenerUnit>> pool = new Queue<LinkedList<SuperEventListenerUnit>>();
        private Queue<LinkedListNode<SuperEventListenerUnit>> pool1 = new Queue<LinkedListNode<SuperEventListenerUnit>>();
        private Queue<Dictionary<Delegate, SuperEventListenerUnit>> pool2 = new Queue<Dictionary<Delegate, SuperEventListenerUnit>>();

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

        internal int AddListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T, T1>(string _eventName, SuperFunctionCallBackV1<T, T1> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T, T1>(string _eventName, SuperFunctionCallBackV1<T, T1> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T, T1, T2>(string _eventName, SuperFunctionCallBackV2<T, T1, T2> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T, T1, T2>(string _eventName, SuperFunctionCallBackV2<T, T1, T2> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T, T1, T2, T3>(string _eventName, SuperFunctionCallBackV3<T, T1, T2, T3> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T, T1, T2, T3>(string _eventName, SuperFunctionCallBackV3<T, T1, T2, T3> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListener<T, T1, T2, T3, T4>(string _eventName, SuperFunctionCallBackV4<T, T1, T2, T3, T4> _callBack)
        {
            return AddListenerReal(_eventName, _callBack, 0);
        }

        internal int AddListener<T, T1, T2, T3, T4>(string _eventName, SuperFunctionCallBackV4<T, T1, T2, T3, T4> _callBack, int _priority)
        {
            return AddListenerReal(_eventName, _callBack, _priority);
        }

        internal int AddListenerReal(string _eventName, Delegate _callBack, int _priority)
        {
            SuperEventListenerUnit unit = new SuperEventListenerUnit(nowIndex, _eventName, _callBack, _priority);

            nowIndex++;

            dicWithID.Add(unit.index, unit);

            Dictionary<Delegate, SuperEventListenerUnit> dic;

            if (!dicWithEvent.TryGetValue(_eventName, out dic))
            {
                dic = GetDic();

                dicWithEvent.Add(_eventName, dic);
            }

            dic.Add(_callBack, unit);

            return unit.index;
        }

        internal void RemoveListener(int _index)
        {
            SuperEventListenerUnit unit;

            if (dicWithID.TryGetValue(_index, out unit))
            {
                dicWithID.Remove(_index);

                Dictionary<Delegate, SuperEventListenerUnit> dic = dicWithEvent[unit.eventName];

                dic.Remove(unit.callBack);

                if (dic.Count == 0)
                {
                    ReleaseDic(dic);

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

        internal void RemoveListener<T>(string _eventName, SuperFunctionCallBackV<T> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T, T1>(string _eventName, SuperFunctionCallBackV1<T, T1> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T, T1, T2>(string _eventName, SuperFunctionCallBackV2<T, T1, T2> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T, T1, T2, T3>(string _eventName, SuperFunctionCallBackV3<T, T1, T2, T3> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        internal void RemoveListener<T, T1, T2, T3, T4>(string _eventName, SuperFunctionCallBackV4<T, T1, T2, T3, T4> _callBack)
        {
            RemoveListenerReal(_eventName, _callBack);
        }

        private void RemoveListenerReal(string _eventName, Delegate _callBack)
        {
            Dictionary<Delegate, SuperEventListenerUnit> dic;

            if (dicWithEvent.TryGetValue(_eventName, out dic))
            {
                SuperEventListenerUnit unit;

                if (dic.TryGetValue(_callBack, out unit))
                {
                    dicWithID.Remove(unit.index);

                    dic.Remove(_callBack);

                    if (dic.Count == 0)
                    {
                        ReleaseDic(dic);

                        dicWithEvent.Remove(_eventName);
                    }
                }
            }
        }

        internal bool DispatchEvent(string _eventName)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBack>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBack cb = unit.callBack as SuperFunctionCallBack;

                    cb(unit.index);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T1>(string _eventName, T1 t1)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBack1<T1>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBack1<T1> cb = unit.callBack as SuperFunctionCallBack1<T1>;

                    cb(unit.index, t1);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T1, T2>(string _eventName, T1 t1, T2 t2)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBack2<T1, T2>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBack2<T1, T2> cb = unit.callBack as SuperFunctionCallBack2<T1, T2>;

                    cb(unit.index, t1, t2);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T1, T2, T3>(string _eventName, T1 t1, T2 t2, T3 t3)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBack3<T1, T2, T3>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBack3<T1, T2, T3> cb = unit.callBack as SuperFunctionCallBack3<T1, T2, T3>;

                    cb(unit.index, t1, t2, t3);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T1, T2, T3, T4>(string _eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBack4<T1, T2, T3, T4>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBack4<T1, T2, T3, T4> cb = unit.callBack as SuperFunctionCallBack4<T1, T2, T3, T4>;

                    cb(unit.index, t1, t2, t3, t4);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T>(string _eventName, ref T t)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBackV<T>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBackV<T> cb = unit.callBack as SuperFunctionCallBackV<T>;

                    cb(unit.index, ref t);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T, T1>(string _eventName, ref T t, T1 t1)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBackV1<T, T1>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBackV1<T, T1> cb = unit.callBack as SuperFunctionCallBackV1<T, T1>;

                    cb(unit.index, ref t, t1);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T, T1, T2>(string _eventName, ref T t, T1 t1, T2 t2)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBackV2<T, T1, T2>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBackV2<T, T1, T2> cb = unit.callBack as SuperFunctionCallBackV2<T, T1, T2>;

                    cb(unit.index, ref t, t1, t2);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T, T1, T2, T3>(string _eventName, ref T t, T1 t1, T2 t2, T3 t3)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBackV3<T, T1, T2, T3>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBackV3<T, T1, T2, T3> cb = unit.callBack as SuperFunctionCallBackV3<T, T1, T2, T3>;

                    cb(unit.index, ref t, t1, t2, t3);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool DispatchEvent<T, T1, T2, T3, T4>(string _eventName, ref T t, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            LinkedList<SuperEventListenerUnit> linkedList = DispatchEventReal<SuperFunctionCallBackV4<T, T1, T2, T3, T4>>(_eventName);

            if (linkedList != null)
            {
                while (linkedList.First != null)
                {
                    LinkedListNode<SuperEventListenerUnit> node = linkedList.First;

                    SuperEventListenerUnit unit = node.Value;

                    SuperFunctionCallBackV4<T, T1, T2, T3, T4> cb = unit.callBack as SuperFunctionCallBackV4<T, T1, T2, T3, T4>;

                    cb(unit.index, ref t, t1, t2, t3, t4);

                    ReleaseLinkedListNode(node);

                    linkedList.RemoveFirst();
                }

                ReleaseLinkedList(linkedList);

                return true;
            }
            else
            {
                return false;
            }
        }

        private LinkedList<SuperEventListenerUnit> DispatchEventReal<T>(string _eventName)
        {
            LinkedList<SuperEventListenerUnit> linkedList = null;

            Dictionary<Delegate, SuperEventListenerUnit> dic;

            if (dicWithEvent.TryGetValue(_eventName, out dic))
            {
                IEnumerator<KeyValuePair<Delegate, SuperEventListenerUnit>> enumerator = dic.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key is T)
                    {
                        if (linkedList == null)
                        {
                            linkedList = GetLinkedList();
                        }

                        KeyValuePair<Delegate, SuperEventListenerUnit> pair = enumerator.Current;

                        int priority = pair.Value.priority;

                        LinkedListNode<SuperEventListenerUnit> addNode = GetLinkedListNode(pair.Value);

                        LinkedListNode<SuperEventListenerUnit> lastNode = linkedList.First;

                        if (lastNode == null)
                        {
                            linkedList.AddFirst(addNode);
                        }
                        else
                        {
                            while (true)
                            {
                                SuperEventListenerUnit unit = lastNode.Value;

                                if (priority > unit.priority)
                                {
                                    lastNode = lastNode.Next;

                                    if (lastNode == null)
                                    {
                                        linkedList.AddLast(addNode);

                                        break;
                                    }
                                }
                                else
                                {
                                    linkedList.AddBefore(lastNode, addNode);

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return linkedList;
        }

        private LinkedList<SuperEventListenerUnit> GetLinkedList()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                LinkedList<SuperEventListenerUnit> linkedList = new LinkedList<SuperEventListenerUnit>();

                return linkedList;
            }
        }

        private void ReleaseLinkedList(LinkedList<SuperEventListenerUnit> _linkedList)
        {
            pool.Enqueue(_linkedList);
        }

        private LinkedListNode<SuperEventListenerUnit> GetLinkedListNode(SuperEventListenerUnit _unit)
        {
            LinkedListNode<SuperEventListenerUnit> linkedListNode;

            if (pool1.Count > 0)
            {
                linkedListNode = pool1.Dequeue();

                linkedListNode.Value = _unit;
            }
            else
            {
                linkedListNode = new LinkedListNode<SuperEventListenerUnit>(_unit);
            }

            return linkedListNode;
        }

        private void ReleaseLinkedListNode(LinkedListNode<SuperEventListenerUnit> _linkedListNode)
        {
            pool1.Enqueue(_linkedListNode);
        }

        private Dictionary<Delegate, SuperEventListenerUnit> GetDic()
        {
            if (pool2.Count > 0)
            {
                return pool2.Dequeue();
            }
            else
            {
                Dictionary<Delegate, SuperEventListenerUnit> dic = new Dictionary<Delegate, SuperEventListenerUnit>();

                return dic;
            }
        }

        private void ReleaseDic(Dictionary<Delegate, SuperEventListenerUnit> _dic)
        {
            pool2.Enqueue(_dic);
        }

        internal void Clear()
        {
            dicWithID.Clear();

            IEnumerator<Dictionary<Delegate, SuperEventListenerUnit>> enumerator = dicWithEvent.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.Clear();

                ReleaseDic(enumerator.Current);
            }

            dicWithEvent.Clear();
        }

        internal void LogNum()
        {
        }
    }
}