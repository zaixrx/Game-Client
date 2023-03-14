using System;
using UnityEngine;

public struct InputPayload
{
    public int Tick;
    public float Time;
    public Vector2 Input;
    public Vector2 Rotation;
}

[Serializable]
public struct StatePayload 
{
    public int Tick;
    public float Time;
    public Vector3 Position;
    public Vector2 Rotation;
}

public class LocalPlayerController : MonoBehaviour
{
    [SerializeField]
    private float playerSpeed = 5f;

    [SerializeField]
    private Transform playerCamera;

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

    private int tick;
    private float time;

    void Start()
    {
        ticksTimeDelta = 1f / 30f;
        defaultSP = default(StatePayload);

        inputPayloads = new InputPayload[Constants.DATA_BUFFER_SIZE];
        statePayloads = new StatePayload[Constants.DATA_BUFFER_SIZE];
    }

    void Update()
    {
        time += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!Client.Instance.isConnected) return;

        HandleTick(time);

        tick++;
    }

    int previousTick = -1;
    float xInput, yInput;
    private void HandleTick(float time)
    {
        // This litertally took me like 2 days to figure out
        if (previousTick == tick) return;

        previousTick = tick;

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
            Input = new Vector3(xInput, yInput)
        };
        StatePayload statePayload = Move(inputPayload);

        inputPayloads[index] = inputPayload;
        statePayloads[index] = statePayload;

        ClientSend.PlayerInput(inputPayload);
    }

    [SerializeField, Tooltip("Should be turned off only when debugging")]
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
        transform.eulerAngles = payload.Rotation;

        movementVector = transform.right * payload.Input.x + transform.forward * payload.Input.y;
        movementVector *= playerSpeed * ticksTimeDelta;

        transform.position += movementVector;

        return new()
        {
            Time = payload.Time,
            Tick = payload.Tick,
            Rotation = new Vector2(playerCamera.eulerAngles.x, transform.eulerAngles.y),
            Position = transform.position
        };
    }

    public void SetPlayerState(StatePayload payload) => receivedStatePayload = payload;
}