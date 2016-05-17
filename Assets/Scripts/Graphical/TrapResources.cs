using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TrapResources : MonoBehaviour {
    [SerializeField]
    Image trap1Resources;
    [SerializeField]
    Image[] trap2Resources;
    [SerializeField]
    Image trap3Resources;

    /// <summary>
    /// Toggle color of trap resources
    /// 0 is Banana, 1 is Leaf, 2 is Stick, 3 is Sap
    /// </summary>
    /// <param name="on">Toggle white(true) or black(false)</param>
    /// <param name="number">0 is Banana, 1 is Leaf, 2 is Stick, 3 is Sap</param>
    public void Toggle(bool on, int number) {
        switch(number) {
            case (0):
                trap1Resources.color = on ? Color.white : Color.black;
                break;
            case (1):
                trap2Resources[0].color = on ? Color.white : Color.black;
                break;
            case (2):
                trap2Resources[1].color = on ? Color.white : Color.black;
                break;
            case (3):
                trap3Resources.color = on ? Color.white : Color.black;
                break;
        }
    }
}
