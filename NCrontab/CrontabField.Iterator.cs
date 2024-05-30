using NCrontab.Utils;

namespace NCrontab
{
    public sealed partial class CrontabField
    {
        internal sealed class Iterator
        {
            readonly CrontabField _crontabField;
            readonly bool _forwardMoving;

            public Iterator(CrontabField crontabField, bool forwardMoving)
            {
                _crontabField = crontabField;
                _forwardMoving = forwardMoving;
            }

            /// <summary>
            /// Returns the first valid value (according to filter) beginning at <paramref name="start"/>.
            /// </summary>
            /// <param name="start"></param>
            /// <returns></returns>
            public int Next(int start)
            {
                if (BeyondLowerBoundValueSet(start))
                    return LowerBoundValueSet;

                var startIndex = _crontabField.ValueToIndex(start);
                var lastIndex = _crontabField.ValueToIndex(UpperBoundValueSet);

                var incrementor = new Incrementor(_forwardMoving);
                for (var i = startIndex; incrementor.BeforeOrEqual(i, lastIndex); i = incrementor.Increment(i))
                {
                    if (_crontabField._bits[i])
                        return _crontabField.IndexToValue(i);
                }

                return nil;
            }

            public int LowerBound => _forwardMoving ? _crontabField.GetFirst() : _crontabField.GetLast();

            public int LowerBoundValueSet => _forwardMoving ? _crontabField._minValueSet : _crontabField._maxValueSet;
            public int UpperBoundValueSet => _forwardMoving ? _crontabField._maxValueSet : _crontabField._minValueSet;

            public bool BeyondLowerBoundValueSet(int start) => _forwardMoving ?
                                                                start < _crontabField._minValueSet :
                                                                start > _crontabField._maxValueSet;

        }
    }
}
