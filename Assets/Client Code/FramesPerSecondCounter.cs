using TMPro;
using UnityEngine;

public class FramesPerSecondCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI fpsText;

    [SerializeField]
    private float repeatRate = .2f;

    private int fps;

    void Start()
    {
        InvokeRepeating("Count", 0, repeatRate);
    }
    void Update()
    {
        fps = (int)(1 / Time.deltaTime);
    }

    void Count() { 
        fpsText.text = fps.ToString();
    }
}
