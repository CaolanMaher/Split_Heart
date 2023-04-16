using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomType : MonoBehaviour
{
    //0 -> LR, 1 -> LRB, 2 -> LRT, 3 -> LRBT
    public int type;

    public void RoomDestruction()
    {
        Destroy(gameObject.transform.parent.gameObject);
    }
}
