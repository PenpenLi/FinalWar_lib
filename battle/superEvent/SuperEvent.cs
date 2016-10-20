namespace superEvent
{
    internal struct SuperEvent
    {
        internal int index;
        internal object[] datas;

        internal SuperEvent(int _index, object[] _datas)
        {
            index = _index;
            datas = _datas;
        }
    }
}
