using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketGraphsDisplayer : MonoBehaviour
{
    [SerializeField]
    private MultiGraphRenderer
        multiGraphRendererPosition, multiGraphRendererRotation,
        multiGraphRendererLinear, multiGraphRendererAngular;

    private void FixedUpdate()
    {
        multiGraphRendererPosition.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketTransform.position.x));
        multiGraphRendererPosition.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketTransform.position.y));
        multiGraphRendererPosition.AddPoint(2, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketTransform.position.z));

        multiGraphRendererRotation.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, RocketData.EulerRotation.x));
        multiGraphRendererRotation.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, RocketData.EulerRotation.y));
        multiGraphRendererRotation.AddPoint(2, new Vector2(Time.timeSinceLevelLoad, RocketData.EulerRotation.z));

        multiGraphRendererLinear.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketRigidbody.velocity.x));
        multiGraphRendererLinear.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketRigidbody.velocity.y));
        multiGraphRendererLinear.AddPoint(2, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketRigidbody.velocity.z));

        //float rps2rpm = 60f / (Mathf.PI * 2f);
        float rps2dps = 180f / Mathf.PI;
        multiGraphRendererAngular.AddPoint(0, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketRigidbody.angularVelocity.x * rps2dps));
        multiGraphRendererAngular.AddPoint(1, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketRigidbody.angularVelocity.y * rps2dps));
        multiGraphRendererAngular.AddPoint(2, new Vector2(Time.timeSinceLevelLoad, RocketData.RocketRigidbody.angularVelocity.z * rps2dps));
    }
}
