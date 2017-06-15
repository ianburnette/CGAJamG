using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {
    #region Private Variables
    [SerializeField]
    Transform player;
    [SerializeField]
    Vector3 offset;
    #endregion

    #region Public Properties
		
    #endregion

    #region Unity Functions
	void Start () {
		
	}
	
	void Update () {
        transform.position = player.transform.position + offset;
	}
    #endregion

    #region Custom Functions
		
    #endregion
}
