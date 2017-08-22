using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTime : MonoBehaviour {
    public float speedup = .2f;
    public float slowdown = -.2f;
    public float timeScale = .2f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Time.timeScale = timeScale;
        if (Input.GetButtonDown("fast"))
        {
            ChangeTime(speedup);
        }
        if (Input.GetButtonDown("slow"))
        {
            ChangeTime(slowdown);
        }
        
	}
    public void ChangeTime(float time)
    {
        Time.timeScale += time;
    } 
}
