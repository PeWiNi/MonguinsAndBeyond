using UnityEngine;
using System.Collections;

public class MakeScreenshot : MonoBehaviour {
    private bool takeScreenshot = false;
    public string shotName = "MonguinsThesis";

    public void Setup() {
        InvokeRepeating("TakeScreenshot", 1, 30);
    }

    public void TakeScreenshot() {
        shotName = "MonguinsThesis";
        takeScreenshot = true;
    }

    public void TakeScreenshot(string name) {
        shotName = name;
        takeScreenshot = true;
    }

    public static string ScreenShotName(string name) {
        string filePath = string.Format("{0}/images", System.IO.Directory.GetParent(Application.dataPath));
        System.IO.Directory.CreateDirectory(filePath);
        return string.Format("{0}/{1}_screen({2}).png", filePath,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), name);
    }

    void LateUpdate() {
        if (takeScreenshot) {
            StartCoroutine(UploadPNG());
        }
    }

    IEnumerator UploadPNG() {
        // We should only read the screen after all rendering is complete
        yield return new WaitForEndOfFrame();
        
        // Create a texture the size of the screen, RGB24 format
        var width = Screen.width;
        var height = Screen.height;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        string filename = ScreenShotName(shotName);
        System.IO.File.WriteAllBytes(filename, bytes);
        Destroy(tex);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
        takeScreenshot = false;
    }
}