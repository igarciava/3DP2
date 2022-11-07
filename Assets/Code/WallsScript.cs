using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallsScript : MonoBehaviour
{
    public Animation Animation;

    public void PlayAnimation(string AnimationName)
    {
        Animation.Play(AnimationName);
    }
}
