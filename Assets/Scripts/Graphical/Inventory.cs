using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {
    [SerializeField]
    int bananas = 10;
    [SerializeField]
    int sticks = 2;
    [SerializeField]
    int sap = 2;
    [SerializeField]
    int leaves = 2;
    [SerializeField]
    int berryR = 2;
    [SerializeField]
    int berryG = 2;
    [SerializeField]
    int berryB = 2;
    public int bananaCount { get { return bananas; } }
    public int stickCount  { get { return sticks; } }
    public int sapCount    { get { return sap; } }
    public int leafCount   { get { return leaves; } }
    public int berryRCount { get { return berryR; } }
    public int berryGCount { get { return berryG; } }
    public int berryBCount { get { return berryB; } }
    //THOUGHT: Cap/Duration/whatever?

    public bool useBanana(int count = 1) {
        if (bananas == 0)
            return false;
        bananas -= count;
        return true;
    }

    public bool useSticks(int count = 1) {
        if (sticks == 0)
            return false;
        sticks -= count;
        return true;
    }

    public bool useForSpikes(int count = 1) {
        if (sticks == 0 || leaves == 0)
            return false;
        sticks -= count;
        leaves -= count;
        return true;
    }

    public bool useSap(int count = 1) {
        if (sap == 0)
            return false;
        sap -= count;
        return true;
    }

    public bool useLeaf(int count = 1) {
        if (leaves == 0)
            return false;
        leaves -= count;
        return true;
    }

    public bool useBerry(string type) {
        int berry = (type == "BerryG" ? berryG : type == "BerryB" ? berryB : berryR);
        if (berry == 0)
            return false;
        berry = (type == "BerryG" ? berryG-- : type == "BerryB" ? berryB-- : berryR--);
        return true;
    }

    public void pickupBanana(int count = 1) {
        bananas += count;
        transform.FindChild("Banana").GetComponentInChildren<Text>().text = "" + bananaCount;
    }

    public void pickupSticks(int count = 1) {
        sticks += count;
        transform.FindChild("Stick").GetComponentInChildren<Text>().text = "" + stickCount;
    }

    public void pickupSap(int count = 1) {
        sap += count;
        transform.FindChild("Sap").GetComponentInChildren<Text>().text = "" + sapCount;
    }

    public void pickupLeaf(int count = 1) {
        leaves += count;
        transform.FindChild("Leaf").GetComponentInChildren<Text>().text = "" + leafCount;
    }

    public void pickupBerry(int value, float RNG) {
        if (value > 50) // Also do useful stuff with RNG
            berryG++;
        else berryB++;
        transform.FindChild("BerryR").GetComponentInChildren<Text>().text = "" + berryRCount;
        transform.FindChild("BerryG").GetComponentInChildren<Text>().text = "" + berryGCount;
        transform.FindChild("BerryB").GetComponentInChildren<Text>().text = "" + berryBCount;
        Debug.Log("Picked up Berry! " + (value > 50 ? "A GOOD ONE!" : "A bad one ._."));
    }
}
