using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGeneration : MonoBehaviour {
    #region Private Variables
    [SerializeField] GameObject chunk;
    [SerializeField] Vector2 fieldSize, chunkSize;
    [SerializeField] Transform chunkParent;
    #endregion

    #region Public Properties
		
    #endregion

    #region Unity Functions
	void Start () {
		
	}

    void OnEnable()
    {
        for (int i = 0; i < fieldSize.x; i++)
        {
            for (int j = 0; j < fieldSize.y; j++)
            {
                GameObject newChunk = GameObject.Instantiate(chunk);
                newChunk.transform.parent = chunkParent;
                newChunk.transform.position = new Vector2(i * chunkSize.x, j * -chunkSize.y);
            }
        }
    }
	
	void Update () {
		
	}
    #endregion

    #region Custom Functions
		
    #endregion
}
