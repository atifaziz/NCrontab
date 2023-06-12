using System;

namespace NCrontab.Utils
{
    internal class Incrementor
    {
        readonly bool _forwardMoving;

        public Incrementor(bool forwardMoving)
        {
            _forwardMoving = forwardMoving;
        }

        public bool BeforeOrEqual(int value, int limit) => _forwardMoving ? value <= limit : value >= limit;
        public bool After(int value, int limit) => _forwardMoving ? value > limit : value < limit;
        public bool AfterOrEqual(int value, int limit) => _forwardMoving ? value >= limit : value <= limit;
        public bool AfterOrEqual(DateTime value, DateTime limit) => _forwardMoving ? value >= limit : value <= limit;
        public int Increment(int value) => _forwardMoving ? value + 1 : value - 1;
    }
}
