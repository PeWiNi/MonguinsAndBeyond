using UnityEngine;
using UnityEngine.Networking;
public class MyNetworkManagerHUD : MonoBehaviour {
	private NetworkManager manager;
    private System.Collections.Hashtable attributes;
    //private System.Collections.Hashtable attributes;
    [SerializeField] public bool showGUI = true;
	[SerializeField] public int offsetX;
	[SerializeField] public int offsetY;
    [SerializeField]
    public GameObject HUD;
    [SerializeField]
    public GameObject Attribute;

    public int team = 0;

    // Runtime variable
    bool showServer = false;

	void Awake() {
		manager = GetComponent<NetworkManager>();
        swapMenus(false);
    }

    void Update() {
		if (!showGUI)
			return;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }

        if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null) {
			if (Input.GetKeyDown(KeyCode.S)) {
				S();
			}
			if (Input.GetKeyDown(KeyCode.H)) {
				H();
			}
			if (Input.GetKeyDown(KeyCode.C)) {
				C();
            }
        }
		if (NetworkServer.active && NetworkClient.active) {
			if (Input.GetKeyDown(KeyCode.X)) {
				manager.StopHost();
            }
        }
	}

	void OnGUI() {
		if (!showGUI)
			return;

		int xpos = 10 + offsetX;
		int ypos = 100 + offsetY;
		int spacing = 24;

		if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null) {
            // Team Selection
            if (GUI.Button(new Rect(Screen.width - xpos - 100, ypos, 100, 20), "Banana")) {
                team = 1;
            } if (GUI.Button(new Rect(Screen.width - xpos - 100, ypos + spacing, 100, 20), "Fish")) {
                team = 2;
            }

            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Host(H)")) {
				H();
			}
			ypos += spacing;

			if (team > 0 && GUI.Button(new Rect(xpos, ypos, 105, 20), "LAN Client(C)")) {
				C();
			}
			manager.networkAddress = GUI.TextField(new Rect(xpos + 105, ypos, 95, 20), manager.networkAddress);
			ypos += spacing;

			if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Server Only(S)")) {
				S();
			}
			ypos += spacing;

            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Exit (Esc)")) {
                Application.Quit();
            }
            ypos += spacing;
        }
		else {
			if (NetworkServer.active) {
                GUI.Label(new Rect(xpos, ypos, 300, 20), "Server: port=" + manager.networkPort);
				ypos += spacing;
			}
			if (NetworkClient.active) {
				GUI.Label(new Rect(xpos, ypos, 300, 20), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
				ypos += spacing;
			}
		}

		if (NetworkClient.active && !ClientScene.ready) {
			if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Client Ready")) {
				ClientScene.Ready(manager.client.connection);
				
				if (ClientScene.localPlayers.Count == 0) {
					ClientScene.AddPlayer(0);
				}
			}
			ypos += spacing;
		}

		if (NetworkServer.active || NetworkClient.active) {
			if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Stop (X)")) {
				manager.StopHost();
			}
			ypos += spacing;
		}
        #region Matchmaking stuff
        /*
        if (!NetworkServer.active && !NetworkClient.active) {
			ypos += 10;

			if (manager.matchMaker == null) {
				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Enable Match Maker (M)")) {
					manager.StartMatchMaker();
				}
				ypos += spacing;
			}
			else {
				if (manager.matchInfo == null) {
					if (manager.matches == null) {
						if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Create Internet Match")) {
							manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
						}
						ypos += spacing;

						GUI.Label(new Rect(xpos, ypos, 100, 20), "Room Name:");
						manager.matchName = GUI.TextField(new Rect(xpos+100, ypos, 100, 20), manager.matchName);
						ypos += spacing;

						ypos += 10;

						if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Find Internet Match")) {
							manager.matchMaker.ListMatches(0,20, "", manager.OnMatchList);
						}
						ypos += spacing;
					}
					else {
						foreach (var match in manager.matches) {
							if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Join Match:" + match.name)) {
								manager.matchName = match.name;
								manager.matchSize = (uint)match.currentSize;
								manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);
							}
							ypos += spacing;
						}
					}
				}

				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Change MM server")) {
					showServer = !showServer;
				}
				if (showServer) {
					ypos += spacing;
					if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Local")) {
						manager.SetMatchHost("localhost", 1337, false);
						showServer = false;
					}
					ypos += spacing;
					if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Internet")) {
						manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
						showServer = false;
					}
					ypos += spacing;
					if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Staging")) {
						manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
						showServer = false;
					}
				}

				ypos += spacing;

				GUI.Label(new Rect(xpos, ypos, 300, 20), "MM Uri: " + manager.matchMaker.baseUri);
				ypos += spacing;

				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Disable Match Maker")) {
					manager.StopMatchMaker();
				}
				ypos += spacing;
			}
		} */
        #endregion
    }

    void H() { //LAN Host
        fetchAttributes();
        swapMenus(true);
        manager.StartHost();
    }

    void C() { //LAN Client
        fetchAttributes();
        swapMenus(true);
        manager.StartClient();
    }

    void S() { //LAN Server Only
        manager.StartServer();
    }

    void swapMenus(bool hud) {
        try {
            HUD.SetActive(hud);
            Attribute.SetActive(!hud);
        } catch {
            GameObject.Find("HUD").SetActive(hud);
            GameObject.Find("attributeSliders").SetActive(!hud);
        }
    }

    void fetchAttributes() {
        //try { attributes = Attribute.GetComponent<AttributeSliders>().getAttributes(); } catch { attributes = GameObject.Find("attributeSliders").GetComponent<AttributeSliders>().getAttributes(); }
    }
    public System.Collections.Hashtable getAttributes() {
        return attributes;
    }
}
