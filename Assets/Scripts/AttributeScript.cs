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

    // Use this for initialization
    void Start() {
        attributeTextPrefab = Resources.Load("Prefabs/attributeText", typeof(Text)) as Text;
        attFields = new Text[attributes.Split(';').Length];
        attPoints = setPoints();
    }

    // Update is called once per frame
    void Update() {
        if (dragUI.getPosition() != currentPosition) {
            currentPosition = dragUI.getPosition();
            //Update attribute values
            int[] attributes = getDistribution();
            int i = 0;
            foreach (int att in attributes) {
                print(attFields[i++].text + " is (" + att + " of 100)");
            }
        }

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
        for (int i = 0; i < distribution.Length; i++) {
            dist[i] = System.Math.Sqrt(System.Math.Pow((attPoints[i].x - position.x), 2) + System.Math.Pow((attPoints[i].y - position.y), 2));
            total += dist[i];
        }
        int missing = 0;
        int[] min = new int[attLength];
        int minNum = 100;
        int z = 0;
        print("Total is " + total);
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
            print(attFields[i].text + " is " + dist[i] + " or (" + distribution[i] + " of 100)");
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
