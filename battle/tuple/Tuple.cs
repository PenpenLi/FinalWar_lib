namespace tuple
{
    internal struct Tuple<T1>
    {
        public T1 first;

        public Tuple(T1 _first)
        {
            first = _first;
        }
    }

    internal struct Tuple<T1, T2>
    {
        public T1 first;

        public T2 second;

        public Tuple(T1 _first, T2 _second)
        {
            first = _first;

            second = _second;
        }
    }

    internal struct Tuple<T1, T2, T3>
    {
        public T1 first;

        public T2 second;

        public T3 third;

        public Tuple(T1 _first, T2 _second, T3 _third)
        {
            first = _first;

            second = _second;

            third = _third;
        }
    }

    internal struct Tuple<T1, T2, T3, T4>
    {
        public T1 first;

        public T2 second;

        public T3 third;

        public T4 fourth;

        public Tuple(T1 _first, T2 _second, T3 _third, T4 _fourth)
        {
            first = _first;

            second = _second;

            third = _third;

            fourth = _fourth;
        }
    }
}
