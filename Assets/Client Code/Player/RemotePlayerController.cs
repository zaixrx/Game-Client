using UnityEngine;
using TransformData = SnapshotInterpolationTransform.TransformData;

public class RemotePlayerController : MonoBehaviour
{
    const int SNAPSHOT_INTERVAL = 30;
    const int SNAPSHOT_BUFFERING = 2;

    SnapshotInterpolationTransform _interpolation;

    void Awake()
    {
        _interpolation = new SnapshotInterpolationTransform();
        _interpolation.Init(SNAPSHOT_INTERVAL, SNAPSHOT_BUFFERING);
    }

    void Update()
    {
        _interpolation.Update(Time.deltaTime);

        transform.position = _interpolation.Current.Position;
        transform.eulerAngles = _interpolation.Current.Rotation;
    }

    public void AddSnapshot(StatePayload payload) => _interpolation.Add(payload.Time, TransformData.GetValue(payload) );
}