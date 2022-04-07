using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MeepPositionSelect : MonoBehaviour
{
    private GameScript gs;
    private MeepPositions p;
    private Direction d;
    [SerializeField] private Text t;
    private bool none = false;
    public void Set_Params(GameScript _gs, (MeepPositions p, Direction d) v)
    {
        gs = _gs;
        p = v.p;
        d = v.d;
        t.text = d + " " + p;
    }

    public void Click()
    {
        if (none)
        {
            gs.SkipMeepPlacement();
        }
        else
        {
            gs.PlaceMeep(p, d);
        }
    }
    public void Set_As_None(GameScript _gs)
    {
        gs = _gs;
        none = true;
         t.text = "Continue";
    }
}
