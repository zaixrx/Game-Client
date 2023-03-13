using System;
using UnityEngine;

public abstract class SnapshotInterpolation<T>
{
    const float TIME_DIALTION = 0.02f;

    struct Entry
    {
        public T Value;
        public float Time;
    }

    float _time;
    float _timeScale;
    float _timeScaleTimer;

    bool _insert;
    float _offset;
    int _rate;

    int _head;
    int _tail;
    int _count;

    Entry[] _array;
    Entry _current;

    Entry Oldest => this[0];
    Entry Latest => this[_count - 1];

    Entry this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
            {
                throw new IndexOutOfRangeException();
            }

            return _array[(_tail + index) % _array.Length];
        }
    }

    public T Current => _current.Value;

    public void Init(int rate, int buffering)
    {
        _rate = rate;
        _array = new Entry[buffering * 3];
        _offset = (1f / rate) * buffering;

        Reset();
    }

    public void Update(float dt)
    {
        if (_count == 0)
        {
            return;
        }

        // move interpolation time forward
        _time += dt * _timeScale;

        // check if we should stop time scale override
        if (_timeScaleTimer > 0 && (_timeScaleTimer -= dt) <= 0)
        {
            _timeScale = 1;
            _timeScaleTimer = 0;
        }

        // find interpolation point
        for (var i = 0; i < (_count - 1); ++i)
        {
            var f = this[i];
            var t = this[i + 1];

            if (f.Time <= _time && t.Time >= _time)
            {
                var range = t.Time - f.Time;
                var value = _time - f.Time;

                _current.Value = Interpolate(f.Value, t.Value, Mathf.Clamp01(value / range));
                _current.Time = _time;

                // done
                return;
            }
        }

        // fallback to oldest (if before) 
        if (_time <= Oldest.Time)
        {
            _current = Oldest;
        }

        // this means time is above latest snapshot we got, handle this
        else if (_time >= Latest.Time)
        {
            var latest = Latest;
            Reset();
            _current = latest;
            _insert = true;
        }
        else
        {
            throw new Exception("couldn't find interpolation point");
        }
    }

    public void Teleport(float time, T value)
    {
        Reset();
        Add(time, value);
    }

    public void Add(float time, T value)
    {
        if (_count == _array.Length)
        {
            _array[_tail] = default;

            _tail = (_tail + 1) % _array.Length;
            _count -= 1;
        }

        // time went backwards? reset
        if (_count > 0 && Latest.Time > time)
        {
            Reset();
        }

        // init time from first sample
        if (_count == 0)
        {
            if (_insert)
            {
                _insert = false;
                Add(time - (1f / _rate), _current.Value);
            }

            _time = time - _offset;
        }
        else
        {
            _insert = false;

            var min = time - (_offset * 1.5f);
            var max = time;

            if (_time < min)
            {
                _timeScale = 1f + TIME_DIALTION;
                _timeScaleTimer = (1f / _rate) * 2;
            }
            else if (_time > max)
            {
                _time = time - _offset;
                _timeScale = 1f;
                _timeScaleTimer = 0;
            }
            else
            {
                _timeScale = 1f;
                _timeScaleTimer = 0;
            }
        }

        // store at head
        _array[_head].Time = time;
        _array[_head].Value = value;

        // move head pointer forward
        _head = (_head + 1) % _array.Length;

        // add count
        _count += 1;
    }

    protected abstract T Interpolate(T from, T to, float alpha);

    void Reset()
    {
        _head = 0;
        _tail = 0;
        _count = 0;
        _time = 0;

        _timeScale = 1;
        _timeScaleTimer = 0;

        _current = default;
        _insert = false;

        Array.Clear(_array, 0, _array.Length);
    }
}

public class SnapshotInterpolationTransform : SnapshotInterpolation<SnapshotInterpolationTransform.TransformData>
{
    public struct TransformData
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }

    protected override TransformData Interpolate(TransformData from, TransformData to, float alpha)
    {
        TransformData result;
        result.Position = Vector3.Lerp(from.Position, to.Position, alpha);
        result.Rotation = Vector3.Lerp(from.Rotation, to.Rotation, alpha);
        return result;
    }
}