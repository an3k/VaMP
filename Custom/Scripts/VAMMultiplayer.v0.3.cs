// VAM Multiplayer v0.3
// vamrobotics (8-25-2019)
// an3k (11-21-2019)
// Do whatever you want with this, attribution would be nice ;)

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace vamrobotics
{
	class VAMMultiplayer : MVRScript
	{
		private Socket client;

		protected JSONStorableStringChooser playerChooser;
		protected JSONStorableStringChooser serverChooser;
		protected JSONStorableStringChooser portChooser;
		protected JSONStorableStringChooser protocolChooser;
		protected JSONStorableString diagnostics;
		protected UIDynamicTextField diagnosticsTextField;
		protected UIDynamicButton connectToServer;
		protected UIDynamicButton disconnectFromServer;
		private List<string> playerList;
		private List<Player> players;
		public float updateTimer;
		
		public override void Init()
		{
			try
			{
				pluginLabelJSON.val = "VAM Multiplayer v0.3";

				// Find all 'Person' Atoms currently in the scene
				Atom tempAtom;
				playerList = new List<string>();
				players = new List<Player>();
				foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
				{
					tempAtom = SuperController.singleton.GetAtomByUid(atomUID);
					if (tempAtom.type == "Person")
					{
						// Add new Player/'Person' Atom to playerList
						playerList.Add(atomUID);

						// Create new Player and add Player's Atom's targets to Player's object
						FreeControllerV3[] targets = tempAtom.freeControllers;
						Player tempPlayer = new Player(atomUID);
						foreach (FreeControllerV3 target in targets)
						{
							tempPlayer.addTarget(target.name, target.transform.position, target.transform.position, target.transform.rotation, target.transform.rotation);
							// SuperController.LogMessage(atomUID);
							// SuperController.LogMessage(target.name);
							// SuperController.LogMessage(target.transform.position.x.ToString() + "," + target.transform.position.y.ToString()+ "," + target.transform.position.z.ToString());
							// SuperController.LogMessage(target.transform.rotation.w.ToString() + "," + target.transform.rotation.x.ToString() + "," + target.transform.rotation.y.ToString() + "," + target.transform.rotation.z.ToString());
						}
						players.Add(tempPlayer);
					}
				}

				// Setup player selector
				playerChooser = new JSONStorableStringChooser("Player Chooser", playerList, null, "Select Player", PlayerChooserCallback);
				RegisterStringChooser(playerChooser);
				CreatePopup(playerChooser);

				// Setup server selector
				List<string> servers = new List<string>();
				// Add new 'servers.Add("NEW SERVER IP");' to add new servers to the list
				servers.Add("167.99.95.234");
				servers.Add("127.0.0.1");
				servers.Add("192.168.1.1");
				serverChooser = new JSONStorableStringChooser("Server Chooser", servers, servers[0], "Select Server", ServerChooserCallback);
				RegisterStringChooser(serverChooser);
				CreatePopup(serverChooser, true);

				// Setup server selector
				List<string> ports = new List<string>();
				// Add new 'ports.Add("NEW PORT");' to add new ports to the list
				ports.Add("8888");
				ports.Add("80");
				ports.Add("443");
				portChooser = new JSONStorableStringChooser("Port Chooser", ports, ports[0], "Select Port", PortChooserCallback);
				RegisterStringChooser(portChooser);
				CreatePopup(portChooser, true);

				// Setup network protocol selector
				List<string> protocols = new List<string>();
				protocols.Add("UDP");
				protocols.Add("TCP");
				protocolChooser = new JSONStorableStringChooser("Protocol Chooser", protocols, protocols[0], "Select Net Protocol", ProtocolChooserCallback);
				RegisterStringChooser(protocolChooser);
				CreatePopup(protocolChooser, true);

				// Setup connect to server button
				connectToServer = CreateButton("Connect to server", true);
				connectToServer.button.onClick.AddListener(ConnectToServerCallback);

				// Setup disconnect from server button
				disconnectFromServer = CreateButton("Disconnect from server", true);
				disconnectFromServer.button.onClick.AddListener(DisconnectFromServerCallback);

				// Setup a text field for diagnostics
				diagnostics = new JSONStorableString("Diagnostics", "Diagnostics:\n");
				diagnosticsTextField = CreateTextField(diagnostics);
			}
			catch (Exception e)
			{
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
			try
			{
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Update is called with each rendered frame by Unity
		void Update()
		{
			try
			{
			}
			catch (Exception e)
			{
				SuperController.LogError("Exception caught: " + e);
			}
		}


		void LateUpdate()
		{
			try
			{
			}
			catch (Exception e)
			{
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// FixedUpdate is called with each physics simulation frame by Unity
		protected void FixedUpdate()
		{
			if (updateTimer > 1.0f)
			{
				try
				{
					// Updates the position and rotation of every target for every 'other' player and sends the current position and rotation data to the server for the main player
					foreach (string playerName in playerList)
					{
						if (playerName != playerChooser.val)
						{
							// Find correct player in the List
							int playerIndex = players.FindIndex(p => p.playerName == playerName);
							Player player = players[playerIndex];

							// Update only target positions and rotations for the 'other' players
							foreach (Player.TargetData target in player.playerTargets)
							{
								if ((target.targetName == "control") || (target.targetName == "headControl") || (target.targetName == "chestControl") || (target.targetName == "hipControl") || (target.targetName == "lFootControl") || (target.targetName == "rFootControl") || (target.targetName == "lHandControl") || (target.targetName == "rHandControl"))
								{
									// If server connection is live
									if (client != null)
									{
										//string response = SendToServer(player + "," + target.targetName + "," + target.position.x.ToString() + "," + target.position.y.ToString() + "," + target.position.z.ToString() + "|");
										string response = SendToServer(player.playerName + "," + target.targetName + "|");

										if (response != "none|")
										{
											// Parse the response from the server and assign the position and rotation data to the target
											string[] targetData = response.Split(',');

											Atom playerAtom = SuperController.singleton.GetAtomByUid(targetData[0]);
											FreeControllerV3 targetObject = playerAtom.GetStorableByID(target.targetName) as FreeControllerV3;

											Vector3 tempPosition = targetObject.transform.position;
											tempPosition.x = float.Parse(targetData[2]);
											tempPosition.y = float.Parse(targetData[3]);
											tempPosition.z = float.Parse(targetData[4]);

											Quaternion tempRotation = targetObject.transform.rotation;
											tempRotation.w = float.Parse(targetData[5]);
											tempRotation.x = float.Parse(targetData[6]);
											tempRotation.y = float.Parse(targetData[7]);
											tempRotation.z = float.Parse(targetData[8]);

											targetObject.transform.position = tempPosition;
											targetObject.transform.rotation = tempRotation;
										}

										// SuperController.LogMessage(response);
									}
								}
							}
						}
						else
						{
							Atom playerAtom = SuperController.singleton.GetAtomByUid(playerName);

							// Find correct player in the List
							int playerIndex = players.FindIndex(p => p.playerName == playerName);
							Player player = players[playerIndex];

							// Update only changed target positions and rotations for the main player
							foreach (Player.TargetData target in player.playerTargets)
							{
								if ((target.targetName == "control") || (target.targetName == "headControl") || (target.targetName == "chestControl") || (target.targetName == "hipControl") || (target.targetName == "lFootControl") || (target.targetName == "rFootControl") || (target.targetName == "lHandControl") || (target.targetName == "rHandControl"))
								{
									FreeControllerV3 targetObject = playerAtom.GetStorableByID(target.targetName) as FreeControllerV3;
									//SuperController.LogError((playerName + "," + target.targetName + "," + target.position.x.ToString() + "," + target.position.y.ToString() + "," + target.position.z.ToString() + "," + target.rotation.w.ToString() + "," + target.rotation.x.ToString() + "," + target.rotation.y.ToString() + "," + target.rotation.z.ToString() + "|"));
									//SuperController.LogError((playerName + "," + target.targetName + "," + target.positionOld.x.ToString() + "," + target.positionOld.y.ToString() + "," + target.positionOld.z.ToString() + "," + target.rotationOld.w.ToString() + "," + target.rotationOld.x.ToString() + "," + target.rotationOld.y.ToString() + "," + target.rotationOld.z.ToString() + "|"));

									if (Vector3.Distance(targetObject.transform.position,target.positionOld) > 0.05f || Mathf.Abs(Quaternion.Angle(targetObject.transform.rotation,target.rotationOld)) > 0.05f)
									{
										// if (target.updateTimer > 0.5f)
										// {
											// If server connection is live
											if (client != null)
											{
												// Send main player's target position and rotation data to the server to be recorded
												string response = SendToServer(playerName + "," + target.targetName + "," + targetObject.transform.position.x.ToString() + "," + targetObject.transform.position.y.ToString() + "," + targetObject.transform.position.z.ToString() + "," + targetObject.transform.rotation.w.ToString() + "," + targetObject.transform.rotation.x.ToString() + "," + targetObject.transform.rotation.y.ToString() + "," + targetObject.transform.rotation.z.ToString() + "|");
											}
											// target.updateTimer = 0.0f;
										// }
									}

									// Update the 'Old' position and rotation data
									// target.updateTimer += Time.fixedDeltaTime;
									target.positionOld = targetObject.transform.position;
									target.rotationOld = targetObject.transform.rotation;
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					SuperController.LogError("Exception caught: " + e);
				}
				updateTimer = 0.0f;
			}
			updateTimer += Time.fixedDeltaTime;
		}
		
		protected void PlayerChooserCallback(string player)
		{
			SuperController.LogMessage(player + " selected.");
		}

		protected void ServerChooserCallback(string server)
		{
			SuperController.LogMessage(server + " selected.");
		}

		protected void PortChooserCallback(string port)
		{
			SuperController.LogMessage(port + " selected.");
		}

		protected void ProtocolChooserCallback(string protocol)
		{
			SuperController.LogMessage(protocol + " selected.");
		}

		protected void ConnectToServerCallback()
		{
			//  Close any established socket server connection
			if (client != null)
			{
				DisconnectFromServerCallback();
			}

			try
			{
				// Connects to the UI selected server:port with the corresponding selected protocol
				IPHostEntry ipHostEntry = Dns.GetHostEntry(serverChooser.val);
				IPAddress ipAddress = Array.Find(ipHostEntry.AddressList, ip => ip.AddressFamily == AddressFamily.InterNetwork);
				// SuperController.LogMessage(ipHostEntry.AddressList[0].ToString());
				IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, int.Parse(portChooser.val));

				if (protocolChooser.val == "TCP")
				{
					client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

					client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
				}
				else if (protocolChooser.val == "UDP")
				{
					client = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
				}

				client.Connect(ipEndPoint);

				SuperController.LogMessage("Connected to server: " + serverChooser.val + ":" + portChooser.val + ".");

				// Send initial player data to the server
				foreach (string playerName in playerList)
				{
					string response = SendToServer(playerName + "|");

					SuperController.LogMessage(response);

					Atom playerAtom = SuperController.singleton.GetAtomByUid(playerName);

					// Find correct player in the List
					int playerIndex = players.FindIndex(p => p.playerName == playerName);
					Player player = players[playerIndex];

					// Update only changed target positions and rotations for the main player
					foreach (Player.TargetData target in player.playerTargets)
					{
						if ((target.targetName == "control") || (target.targetName == "headControl") || (target.targetName == "chestControl") || (target.targetName == "hipControl") || (target.targetName == "lFootControl") || (target.targetName == "rFootControl") || (target.targetName == "lHandControl") || (target.targetName == "rHandControl"))
						{
							FreeControllerV3 targetObject = playerAtom.GetStorableByID(target.targetName) as FreeControllerV3;

							// Send main player's target position and rotation data to the server to be recorded
							response = SendToServer(playerName + "," + target.targetName + "," + target.position.x.ToString() + "," + target.position.y.ToString() + "," + target.position.z.ToString() + "," + target.rotation.w.ToString() + "," + target.rotation.x.ToString() + "," + target.rotation.y.ToString() + "," + target.rotation.z.ToString() + "|");

							SuperController.LogMessage(response);
						}
					}
				}
			}
			catch (Exception e)
			{
				SuperController.LogError("Exception caught: " + e);
			}
		}

		protected string SendToServer(string message)
		{
			// Sends data to server over existing socket connection
			if (client != null)
			{
				byte[] messageBytes = Encoding.ASCII.GetBytes(message);

				int bytesSent = client.Send(messageBytes);

				byte[] responseBytes = new byte[65535];

				int bytesReceived = client.Receive(responseBytes, 0, responseBytes.Length, 0);

				return Encoding.UTF8.GetString(responseBytes, 0, bytesReceived);
			}
			else
			{
				SuperController.LogError("Tried to send but not connected to any server.");

				return "Not Connected.";
			}
		}

		protected void DisconnectFromServerCallback()
		{
			// Closes the current socket connection to the server
			if (client != null)
			{
				client.Shutdown(SocketShutdown.Both);
				client.Close();
				client = null;
				SuperController.LogMessage("Disconnected from current server.");
			}
		}

		// OnDestroy is where you should put any cleanup
		// if you registered objects to supercontroller or atom, you should unregister them here
		protected void OnDestroy()
		{
		}
	}

	// Class for the players
	public class Player
	{
		public string playerName;
		public List<TargetData> playerTargets;

		public Player(string name)
		{
			playerName = name;

			playerTargets = new List<TargetData>();
		}

		public void addTarget(string name, Vector3 pos, Vector3 posOld, Quaternion rot, Quaternion rotOld)
		{
			playerTargets.Add(new TargetData { targetName = name, position = pos, positionOld = posOld, rotation = rot, rotationOld = rot });
		}

		// Using a class to hold the various players target data since Tuples are not as supported in Unity
		public class TargetData
		{
			public string targetName;
			public Vector3 position;
			public Vector3 positionOld;
			public Quaternion rotation;
			public Quaternion rotationOld;
			public float updateTimer;

			public string TargetName { get; set; }
			public string Position { get; set; }
			public string PositionOld { get; set; }
			public string Rotation { get; set; }
			public string RotationOld { get; set; }
			public string UpdateTimer { get; set; }
		}
	}
}
