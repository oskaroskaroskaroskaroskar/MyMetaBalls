using UnityEngine;
using System.Collections;

public class MetaBall : MonoBehaviour {
    protected float radius;
    public bool negativeBall;

    [HideInInspector]
    public float factor;
    private void Awake()
    {
        radius = transform.localScale.x;
    }
    public virtual void Start() {
        this.factor = (this.negativeBall ? -1 : 1) * this.radius * this.radius * 2;
    }
}
