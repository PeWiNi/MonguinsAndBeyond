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

    public bool useBanana(int count = 1, bool use = true) {
        if (bananas == 0)
            return false;
        if (use)
            bananas -= count;
        transform.FindChild("Banana").GetComponentInChildren<Text>().text = "" + GetComponent<Inventory>().bananaCount;
        return true;
    }

    public bool useSticks(int count = 1, bool use = true) {
        if (sticks == 0)
            return false;
        if (use)
            sticks -= count;
        transform.FindChild("Stick").GetComponentInChildren<Text>().text = "" + GetComponent<Inventory>().stickCount;
        return true;
    }

    public bool useForSpikes(int count = 1, bool use = true) {
        if (sticks == 0 || leaves == 0)
            return false;
        if (use) {
            sticks -= count;
            leaves -= count;
        }
        transform.FindChild("Stick").GetComponentInChildren<Text>().text = "" + GetComponent<Inventory>().stickCount;
        transform.FindChild("Leaf").GetComponentInChildren<Text>().text = "" + GetComponent<Inventory>().leafCount;
        return true;
    }

    public bool useSap(int count = 1, bool use = true) {
        if (sap == 0)
            return false;
        if (use)
            sap -= count;
        transform.FindChild("Sap").GetComponentInChildren<Text>().text = "" + GetComponent<Inventory>().sapCount;
        return true;
    }

    public bool useLeaf(int count = 1, bool use = true) {
        if (leaves == 0)
            return false;
        if (use)
            leaves -= count;
        transform.FindChild("Leaf").GetComponentInChildren<Text>().text = "" + GetComponent<Inventory>().leafCount;
        return true;
    }

    public bool useBerry(string type) {
        int berry = (type == "BerryG" ? berryG : type == "BerryB" ? berryB : berryR);
        if (berry == 0)
            return false;
        berry = (type == "BerryG" ? berryG-- : type == "BerryB" ? berryB-- : berryR--);
        transform.FindChild(type).GetComponentInChildren<Text>().text =
                type == "BerryG" ? "" + GetComponent<Inventory>().berryGCount :
                type == "BerryB" ? "" + GetComponent<Inventory>().berryBCount :
                                   "" + GetComponent<Inventory>().berryRCount ;
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


    /// <summary>
    /// Logic for picking up and identifying berries
    /// </summary>
    /// <param name="value">Berry value (determined by random number between 0 and 100)</param>
    /// <param name="RNG">The Wisdom stat of the player</param>
    /// <returns>0 if unknown, 1 if good, 2 if bad</returns>
    public int pickupBerry(int value, float RNG) {
        GetComponentInParent<HUDScript>().ResetCastBar();
        if (RNG == 0) {
            return pickupBerry(Herb.Condition.Random);
        } else { 
            int rand = Random.Range(0, 100);
            // Wisdom formula (0..10 = 0-25%, 11..35 = 25-50%, 36..100 = 50-100%)
            if (rand < (RNG <= 10 ? RNG * 2.5f : RNG <= 35 ? ((RNG - 10) * 1f) + 25f : ((RNG - 36) * 0.78125f) + 50f))
                return getBerryType(value);
            return pickupBerry(Herb.Condition.Random);
        }
    }

    int getBerryType(int value) {
        if (value > 50) {
            return pickupBerry(Herb.Condition.Regeneration);
        } else {
            return pickupBerry(Herb.Condition.Degenration);
        }
    }

    public int pickupBerry(Herb.Condition cond) {
        int type = 0;
        switch(cond) {
            case (Herb.Condition.Regeneration):
                berryG++;
                transform.FindChild("BerryG").GetComponentInChildren<Text>().text = "" + berryGCount;
                type = 1;
                break;
            case (Herb.Condition.Degenration):
                berryB++;
                transform.FindChild("BerryB").GetComponentInChildren<Text>().text = "" + berryBCount;
                type = 2;
                break;
            default:
                berryR++;
                transform.FindChild("BerryR").GetComponentInChildren<Text>().text = "" + berryRCount;
                break;
        }
        return type;
    }
}
