using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : LevelObject
{
    private void LateUpdate()
    {
        transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
    }
}
