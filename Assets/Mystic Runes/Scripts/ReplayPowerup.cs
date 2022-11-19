using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayPowerup : Powerup
{
    protected override void PerformPowerupEffect()
    {
        GameManager.Instance.PlaySequencePreview();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
