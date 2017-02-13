using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : LevelObject
{
    private void LateUpdate()
    {
        transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
    }
}
