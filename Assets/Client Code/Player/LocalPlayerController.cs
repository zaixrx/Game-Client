using System;
using UnityEngine;

public struct InputPayload
{
    public int Tick;
    public float Time;
    public Vector3 Input;
    public Vector3 Rotation;
}

[Serializable]
public struct StatePayload
{
    public int Tick;
    public float Time;
    public Vector3 Rotation;
    public Vector3 Position;
}

public class LocalPlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5f;

    #region Client Predection
    private float ticksTimeDelta;

    private Vector3 movementVector;

    public InputPayload[] inputPayloads;
    public StatePayload[] statePayloads;

    public InputPayload lastInputPayload;
    public StatePayload receivedStatePayload;

    private StatePayload lastProcessedState;
    private StatePayload defaultSP;
    #endregion

    void Start()
    {
        ticksTimeDelta = 1f / 30f;
        defaultSP = default(StatePayload);

        inputPayloads = new InputPayload[Constants.DATA_BUFFER_SIZE];
        statePayloads = new StatePayload[Constants.DATA_BUFFER_SIZE];
    }

    void FixedUpdate()
    {
        if (!Client.Instance.isConnected) return;

        HandleTick(Client.Instance.serverTick, Client.Instance.time);
    }

    float xInput, yInput;
    private void HandleTick(int tick, float time)
    {
        // Checks if there is a new state that is not processed yet
        if (!receivedStatePayload.Equals(defaultSP) && lastProcessedState.Equals(defaultSP) ||
            !receivedStatePayload.Equals(lastProcessedState))
        {
            Reconcile(tick);
        }

        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        int index = tick % Constants.DATA_BUFFER_SIZE;

        InputPayload inputPayload = new InputPayload()
        {
            Tick = tick,
            Time = time,
            Rotation = transform.eulerAngles,
            Input = new Vector3(xInput, 0, yInput)
        };
        StatePayload statePayload = Move(inputPayload);

        inputPayloads[index] = inputPayload;
        statePayloads[index] = statePayload;

        ClientSend.PlayerInput(inputPayload);
    }

    [SerializeField]
    private bool reconcile = true;

    public void Reconcile(int tick)
    {
        if (!reconcile) return;

        lastProcessedState = receivedStatePayload;
        int index = receivedStatePayload.Tick % Constants.DATA_BUFFER_SIZE;

        if (receivedStatePayload.Position != statePayloads[index].Position)
        {
            var distance = Vector3.Distance(receivedStatePayload.Position, statePayloads[index].Position);

            Debug.Log($"Reconcile {distance}");

            transform.position = receivedStatePayload.Position;
            transform.eulerAngles = receivedStatePayload.Rotation;

            statePayloads[index] = receivedStatePayload;

            int tickToProcess = receivedStatePayload.Tick + 1;

            while (tickToProcess < tick)
            {
                int bufferIndex = tickToProcess % Constants.DATA_BUFFER_SIZE;

                StatePayload statePayload = Move(inputPayloads[bufferIndex]);

                statePayloads[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }
    }

    public StatePayload Move(InputPayload payload)
    {
        ticksTimeDelta = 1f / 30f;

        transform.eulerAngles = payload.Rotation;

        movementVector = transform.right * payload.Input.x + transform.forward * payload.Input.z;
        movementVector *= playerSpeed * ticksTimeDelta;

        transform.position += movementVector;

        return new()
        {
            Time = payload.Time,
            Tick = payload.Tick,
            Rotation = transform.eulerAngles,
            Position = transform.position
        };
    }

    public void SetPlayerState(StatePayload payload)
    {
        receivedStatePayload = payload;
    }
}