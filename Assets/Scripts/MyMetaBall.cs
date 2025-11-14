using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMetaBall : MetaBall
{
    // Start is called before the first frame update
    public override void Start()
    {
        factor = radius * radius * .3f;
    }

}
