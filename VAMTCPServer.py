# VAM Multiplayer Server v0.1
# vamrobotics (8-25-2019)
# Do whatever you want with this, attribution would be nice ;)

import os
import socket
import subprocess
import threading
import urllib2
import sys
import random

players = {}

class VAMMultiplayerServer():
	def __init__(self, host, port):
		self.host = host
		self.port = port
		self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
		self.sock.bind((self.host, self.port))

	def listen(self):
		self.sock.listen(5)
		while True:
			client, address = self.sock.accept()
			client.settimeout(90)
			threading.Thread(target = self.clientConnection, args = (client, address)).start()

	def clientConnection(self, client, address):
		while True:
			try:
				request = client.recv(65535)
				if request.endswith("|"):
					request = request[:-1]
					tmp = request.split(",")
					global players
					if len(tmp) == 1:
						playerName = tmp[0]
						if playerName not in players:
							players[playerName] = {}
							print "Adding new player: " + playerName
							client.send(playerName + " added to server.")
						else:
							client.send(playerName + " already added to server.")
					if len(tmp) >= 2:
						playerName = tmp[0]
						if len(tmp) == 2:
							targetName = tmp[1]
							if targetName in players[playerName]:
								client.send(playerName + "," + targetName + "," + players[playerName][targetName])
							else:
								client.send("none|")
						if len(tmp) == 9:
							targetName = tmp[1]
							xPos = tmp[2]
							yPos = tmp[3]
							zPos = tmp[4]
							wRot = tmp[5]
							xRot = tmp[6]
							yRot = tmp[7]
							zRot = tmp[8]
							#print "Player: " + playerName
							#print "Target Name: " + targetName
							#print "X Position: " + xPos
							#print "Y Position: " + yPos
							#print "Z Position: " + zPos
							#print "W Rotation: " + wRot
							#print "X Rotation: " + xRot
							#print "Y Rotation: " + yRot
							#print "Z Rotation: " + zRot
							players[playerName][targetName] = xPos + "," + yPos + "," + zPos + "," + wRot + "," + xRot + "," + yRot + "," + zRot
							client.send("Target data recorded|")
						
			except Exception as e:
				print e
			finally:
				pass
				#client.close()

def main():
	host = "0.0.0.0"
	port = 8888
	print "VAM Multiplayer Server started."
	VAMMultiplayerServer(host, port).listen()

if __name__ == "__main__":
	main()
