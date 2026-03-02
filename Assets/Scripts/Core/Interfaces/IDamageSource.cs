using UnityEngine;

public interface IDamageSource
{
    public Transform transform { get; }
    public Vector3 damageSourcePos { get; set; }
}
