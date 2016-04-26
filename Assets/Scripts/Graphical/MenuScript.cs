﻿using UnityEngine;
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
        #region Failsafe: Find objects if not set
        if (!manager)
            manager = GetComponent<NetworkManager>();
        if (!HUD) 
            HUD = GameObject.Find("HUD");
        if (!HUD) {
            HUD = Instantiate(Resources.Load("Prefabs/HUD"), new Vector3(), Quaternion.identity) as GameObject;
            HUD.transform.parent = transform;
            HUD.name = "HUD";
        }
        if (!Attribute) 
            Attribute = GameObject.Find("MenuAttribute");
        if (!Attribute) {
            Attribute = Instantiate(Resources.Load("Prefabs/MenuAttribute"), new Vector3(), Quaternion.identity) as GameObject;
            Attribute.transform.parent = transform;
            Attribute.name = "MenuAttribute";
        }
        if (!mainInputField)
            mainInputField = Attribute.transform.FindChild("MenuStuff").GetComponentInChildren<InputField>();
        if (!joinButton)
            joinButton = Attribute.transform.FindChild("MenuStuff").FindChild("Join").GetComponent<Button>();
        if (!bananaButton)
            bananaButton = Attribute.transform.FindChild("MenuStuff").FindChild("TeamSelection").FindChild("Banana").GetComponent<Button>();
        if (!fishButton)
            fishButton = Attribute.transform.FindChild("MenuStuff").FindChild("TeamSelection").FindChild("Fish").GetComponent<Button>();
        #endregion
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
        bananaButton.image.color = team == 1 ? highlight : normal;
        fishButton.image.color = team == 2 ? highlight : normal;
    }

    public void Host() { //LAN Host
        fetchAttributes();
        if (team == 0) team = 1;
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
        HUD.SetActive(hud);
        Attribute.SetActive(!hud);
    }

    void fetchAttributes() {
        try { attributes = Attribute.GetComponent<AttributeSliders>().getAttributes(); } catch { attributes = GameObject.Find("attributeSliders").GetComponent<AttributeSliders>().getAttributes(); }
    }
    public Hashtable getAttributes() {
        return attributes;
    }
}