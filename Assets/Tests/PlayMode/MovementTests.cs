using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class MovementTests
{
    [UnityTest]
    public IEnumerator Movement()
    {
        var gameObject = new GameObject();
        var player = gameObject.AddComponent<LocalPlayerController>();

        var inputPayload = new InputPayload()
        {
            Tick = 1,
            Time = 2.99f,
            Input = new Vector3(-1, 0, 1),
            Rotation = new Vector3(25, 250, 0),
        };

        player.Move(inputPayload);

        // Its throwing this exception for some reason
        // NUnit.Framework.AssertionException:   Expected: (-0.16, 0.00, -0.06)  But was:  (-0.16, 0.00, -0.06)
        Assert.AreEqual(new Vector3(-0.08f, -0.07f, -0.21f), gameObject.transform.position);

        yield return null;
    }
}
