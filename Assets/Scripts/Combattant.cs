using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Combattant : Troup
{

    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {

        base.Update();

        
    }

    private void OnMouseOver()
    {
        Follow();
    }

    private void OnMouseExit()
    {
        // Standby();
    }
}
