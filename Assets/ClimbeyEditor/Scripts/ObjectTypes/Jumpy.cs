using UnityEngine;

public class Jumpy : LevelObject
{
    private void LateUpdate()
    {
        //Legs only scale on Y
        foreach (var leg in GetComponentsInChildren<Transform>())
        {
            if (leg.name == "JumpLeg")
            {
                leg.localScale = new Vector3(0.1f / transform.localScale.x, leg.localScale.y, 0.1f / transform.localScale.z);
            }
        }

        //Keep trampoline plane at certain height
        var topY = transform.localScale.y / 2;

        //Reset rotation
        var prevRot = transform.rotation;
        transform.rotation = Quaternion.identity;
        transform.FindChild("Tramp").transform.position = new Vector3(transform.position.x, (transform.position.y + topY) - 0.005f, transform.position.z);
        transform.rotation = prevRot;
    }
}
