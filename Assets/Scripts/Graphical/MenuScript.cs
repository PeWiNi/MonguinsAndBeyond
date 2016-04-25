using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    [SerializeField] NetworkManager manager;
    private Hashtable attributes;
    [SerializeField] GameObject HUD;
    [SerializeField] GameObject Attribute;
    public int team = 0;
    [SerializeField] bool hotKeys = true;

    public InputField mainInputField;
    public Button joinButton;
    public Button bananaButton;
    public Button fishButton;

    // Use this for initialization
    void Start () {
        swapMenus(false);
        mainInputField.text = manager.networkAddress;
        mainInputField.onValueChanged.AddListener(delegate { UpdateAddress(); });
        joinButton.onClick.AddListener(delegate { JoinClient(); });
        bananaButton.onClick.AddListener(delegate { ButtonUpdate(1); });
        fishButton.onClick.AddListener(delegate { ButtonUpdate(2); });
        joinButton.interactable = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!hotKeys)
            return;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }

        if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null) {
            if (Input.GetKeyDown(KeyCode.S)) {
                MakeServer();
            }
            if (Input.GetKeyDown(KeyCode.H)) {
                Host();
            }
            if (Input.GetKeyDown(KeyCode.C)) {
                JoinClient();
            }
        }
        if (NetworkServer.active && NetworkClient.active) {
            if (Input.GetKeyDown(KeyCode.X)) {
                QuitSession();
            }
        }
    }

    public void UpdateAddress() {
        manager.networkAddress = mainInputField.text;
    }

    public void ButtonUpdate(int teamSelect) {
        team = teamSelect;
        joinButton.interactable = team > 0;
        Color highlight = new Color(51f / 255f, 68f / 255f, 34f / 255f);
        Color normal = new Color(137f / 255f, 143f / 255f, 43f / 255f);
        print(team);
        bananaButton.image.color = team == 1 ? highlight : normal;
        fishButton.image.color = team == 2 ? highlight : normal;
    }

    public void Host() { //LAN Host
        fetchAttributes();
        swapMenus(true);
        manager.StartHost();
    }

    public void JoinClient() { //LAN Client
        fetchAttributes();
        swapMenus(true);
        manager.StartClient();
    }

    public void MakeServer() { //LAN Server Only
        manager.StartServer();
    }

    public void QuitSession() { //Stop
        manager.StopHost();
        swapMenus(false);
    }

    void swapMenus(bool hud) {
        try {
            HUD.SetActive(hud);
            Attribute.SetActive(!hud);
        }
        catch {
            GameObject.Find("HUD").SetActive(hud);
            GameObject.Find("attributeSliders").SetActive(!hud);
        }
    }

    void fetchAttributes() {
        try { attributes = Attribute.GetComponent<AttributeSliders>().getAttributes(); } catch { attributes = GameObject.Find("attributeSliders").GetComponent<AttributeSliders>().getAttributes(); }
    }
    public Hashtable getAttributes() {
        return attributes;
    }
}
