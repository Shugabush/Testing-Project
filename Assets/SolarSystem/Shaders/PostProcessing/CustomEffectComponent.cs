using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable, VolumeComponentMenu("CustomEffectComponent")]
public class CustomEffectComponent : VolumeComponent, IPostProcessComponent
{
    // Intensity parameter that goes from 0 to 1
    public ClampedFloatParameter intensity = new ClampedFloatParameter(value: 0, min: 0, max: 1, overrideState: true);

    // A color that is constant even when the weight changes
    public NoInterpColorParameter overlayColor = new NoInterpColorParameter(Color.cyan);

    public bool IsActive()
    {
        return intensity.value > 0;
    }
}
