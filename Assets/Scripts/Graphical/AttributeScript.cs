using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class AttributeScript : MonoBehaviour {

    //DragUI
    public DragUI dragUI;

    public string attributes = "";
    private Text attributeTextPrefab;
    private Vector3 currentPosition;
    private Text[] attFields;
    private Vector3[] attPoints;

    private int lowerTreshold = 5;

    public GameObject capsule;

    // Use this for initialization
    void Start() {
        attributeTextPrefab = Resources.Load("Prefabs/attributeText", typeof(Text)) as Text;
        attFields = new Text[attributes.Split(';').Length];
        attPoints = setPoints();
        capsule = Resources.Load("Prefabs/Capsule", typeof(GameObject)) as GameObject;
        /*
        for (int i = 1; i <= 35; i++) {
            print(i + " = " + compositeHealthFormula(i));
            GameObject Capsule3 = (GameObject)Instantiate(capsule, new Vector3(i, 4, 0), Quaternion.identity);
            float size = compositeHealthFormula(i) / 1000;
            Capsule3.transform.localScale = new Vector3(size, size, size);
            /*
            GameObject Capsule = (GameObject)Instantiate(capsule, new Vector3(i, 0, 0), Quaternion.identity);
            size = healthFormula3(i) / 1000;
            Capsule.transform.localScale = new Vector3(size, size, size);
            GameObject Capsule2 = (GameObject)Instantiate(capsule, new Vector3(i, 2, 0), Quaternion.identity);
            size = healthFormula2(i) / 1000;
            Capsule2.transform.localScale = new Vector3(size, size, size);
            GameObject Capsule4 = (GameObject)Instantiate(capsule, new Vector3(i, -2, 0), Quaternion.identity);
            size = healthFormula(i) / 100;
            Capsule4.transform.localScale = new Vector3(size, size, size);
            //GameObject Capsule5 = (GameObject)Instantiate(capsule, new Vector3(i, -4, 0), Quaternion.identity);
            //size = compositeHealthFormula2(i) / 1000;
            //Capsule5.transform.localScale = new Vector3(size, size, size);
            GameObject Capsule6 = (GameObject)Instantiate(capsule, new Vector3(i, 6, 0), Quaternion.identity);
            size = compositeHealthFormula3(i) / 1000;
            Capsule6.transform.localScale = new Vector3(size, size, size);
            GameObject Capsule7 = (GameObject)Instantiate(capsule, new Vector3(i, 8, 0), Quaternion.identity);
            size = healthFormula4(i) / 1000;
            Capsule7.transform.localScale = new Vector3(size, size, size);
        }
        */
    }

    // Update is called once per frame
    void Update() {
        if (dragUI.getPosition() != currentPosition) {
            currentPosition = dragUI.getPosition();
            //Update attribute values
            int[] attributes = getDistribution();
        }

    }

    public float healthFormula(int numberOfPlayers) {
        float health = 100;
        for (int i = 1; i < numberOfPlayers; i++) {
            health = (health - ((100.0f / (Mathf.Pow(1.71f, i))) / 100.0f) * health);
        }
        return health;
    }

    public float healthFormula2(int numberOfPlayers) {
        float health = 1000 + (numberOfPlayers - 1) * 100;
        health /= numberOfPlayers;
        return health;
    }

    public float healthFormula3(int numberOfPlayers) {
        float health = 1000;
        for (int i = 1; i < numberOfPlayers; i++) {
            health *=  0.875f;
        }
        return health;
    }

    public float healthFormula4(int numberOfPlayers) {
        float health = 1000;
        for (int i = 1; i < numberOfPlayers; i++) {
            health *= 0.90f;
        }
        health += 1000.0f / 3.0f;
        return health;// - 1000 * (1 / 3);
    }

    public float compositeHealthFormula(int numberOfPlayers) {
        float health = 1000;
        for (int i = 1; i < numberOfPlayers; i++) {
            health *= 0.875f;
        }
        if(numberOfPlayers > 9) {
            health += (numberOfPlayers - 7) * 100;
            health /= numberOfPlayers - 8;
        }

        return health;
    }

    public float compositeHealthFormula2(int numberOfPlayers) { //BEST IN TEST
        float health = 1000;
        for (int i = 1; i < numberOfPlayers; i++) {
            health *= 0.875f;
        }
        if (numberOfPlayers > 6) {
            health += (numberOfPlayers - 4) * 100;
            health /= numberOfPlayers - 5;
        }

        return health;
    }

    public float compositeHealthFormula3(int numberOfPlayers) {
        float health = 1000;
        for (int i = 1; i < numberOfPlayers; i++) {
            health *= 0.875f;
        }
        if (numberOfPlayers > 9) {
            health *= 0.9375f;
        }
        if (numberOfPlayers > 17) {
            health = health + ((numberOfPlayers - 15) * 100) / numberOfPlayers - 16;
        }

        return health;
    }

    public Hashtable getAttributes() {
        Hashtable ht = new Hashtable();
        int[] attributes = getDistribution();
        int i = 0;
        foreach (int att in attributes)
            ht.Add(attFields[i++].text, att);
        return ht;
    }

    private Vector3[] setPoints() {
        string[] att = attributes.Split(';');
        int numAtt = att.Length;
        Vector3[] points = new Vector3[numAtt];
        //Circumference
        float d = dragUI.getAreaSize().max.y;
        float C = d * Mathf.PI; //pi = C / d
        float arcLength = C / numAtt;
        for (int i = 0; i < numAtt; i++) {
            //Calculate angle (+ rotate circle to start from top)
            float angle = ((arcLength * i) / (d / 2)) + (Mathf.PI / 2); //angle = arcLength / radius
            //Point on Circumference (origin (j, k))
            float x = d * (float)System.Math.Cos(angle) + dragUI.getAreaSize().center.x; //x(t) = r cos(t) + j
            float y = d * (float)System.Math.Sin(angle) + dragUI.getAreaSize().center.y; //y(t) = r sin(t) + k
            points[i] = new Vector3(x, y, dragUI.getPosition().z);
            if (attFields[i] == null) { //First time setup, instantiate and setParent to this
                attFields[i] = (Text)Instantiate(attributeTextPrefab, points[i], transform.rotation);
                attFields[i].transform.SetParent(transform);
            }
            attFields[i].transform.position = dragUI.dragArea.position + points[i];
            attFields[i].text = att[i];
        }

        return points;
    }

    public int[] getDistribution() {
        Vector3 position = currentPosition;
        int attLength = attributes.Split(';').Length;
        int[] distribution = new int[attLength];
        double[] dist = new double[attLength];
        double total = 0;
        for (int i = 0; i < distribution.Length; i++) { //Following line needs reverse-engineering for the sake of mapping values to the wheel
            dist[i] = System.Math.Sqrt(System.Math.Pow((attPoints[i].x - position.x), 2) + System.Math.Pow((attPoints[i].y - position.y), 2));
            total += dist[i];
        }
        int missing = 0;
        int[] min = new int[attLength];
        int minNum = 100;
        int z = 0;
        //print("Total is " + total);
        //TODO: make fix for more than 4 ._. it goes NEGATIVE (at certain values)
        for (int i = 0; i < attLength; i++) {
            double value = (dist[i] / total * 100) * (attLength - 1) - 100; // try 100 - (dist[i] / total * 100) * (attLength - 1) and make fix for Abs ^^
            if (value < 0)
                distribution[i] = (int)(Mathf.Abs((float)value));
            else
                distribution[i] = 0;
            if (distribution[i] < lowerTreshold)
                distribution[i] = 0;
            missing += distribution[i];
            if (distribution[i] < minNum && distribution[i] > 0) { 
                min[z++] = i;
                minNum = distribution[i];
            }
            //print(attFields[i].text + " is " + dist[i] + " or (" + distribution[i] + " of 100)");
        }
        int newMissing = 0;
        missing = 100 - missing;
        if (z != 1) {
            missing = (int)(missing / (z + 1));
            for (int i = 0; i < z; i++) {
                distribution[min[i]] += missing;
                if (distribution[min[i]] < 0) 
                    distribution[min[i]] = 0;
            }
            foreach(int distri in distribution)
                newMissing += distri;
            newMissing = 100 - newMissing;
            if (newMissing != 0) { /*
                if(distribution[min[0]] <= 0) {
                    int q = 1;
                    newMissing = (int)(newMissing / (z));
                    while(q<z)
                        distribution[min[q++]] += newMissing;
                }
                else */
                    distribution[min[0]] += newMissing;
            }
        } else
            distribution[min[0]] = minNum + missing;

        return distribution;
    }
}
