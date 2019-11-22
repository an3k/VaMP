# VAM Multiplayer UDP Server v0.1
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
		self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
		#self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
		self.sock.bind((self.host, self.port))

	def listen(self):
		#self.sock.listen(5)
		while True:
			request, address = self.sock.recvfrom(65535)
			#client.settimeout(90)
			threading.Thread(target = self.clientConnection, args = (request, address)).start()

	def clientConnection(self, request, address):
		try:
			#request = self.sock.recvfrom(65535)
			#print request
			if request.endswith("|"):
				request = request[:-1]
				tmp = request.split(",")
				global players
				if len(tmp) == 1:
					playerName = tmp[0]
					if playerName not in players:
						players[playerName] = {}
						print "Adding new player: " + playerName
						self.sock.sendto(playerName + " added to server.", address)
					else:
						self.sock.sendto(playerName + " already added to server.", address)
				if len(tmp) >= 2:
					playerName = tmp[0]
					if len(tmp) == 2:
						targetName = tmp[1]
						if targetName in players[playerName]:
							self.sock.sendto(playerName + "," + targetName + "," + players[playerName][targetName], address)
						else:
							self.sock.sendto("none|", address)
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
						self.sock.sendto(playerName + " " + targetName + " stored.", address)
					
		except Exception as e:
			print e
		finally:
			pass

def main():
	host = "0.0.0.0"
	port = 8888
	print "VAM Multiplayer Server started."
	VAMMultiplayerServer(host, port).listen()

if __name__ == "__main__":
	main()
