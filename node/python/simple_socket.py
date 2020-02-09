import socket

class simple_socket():
    def __init__(self, address, port):
        self.address = address
        self.port = port
        self.sock = None
        self.connected = False

    def try_connect(self):
        try:
            self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.sock.settimeout(0.2)
            self.sock.connect((self.address, self.port))
        except:
            return
        print('Connection established')
        self.connected = True
        return

    def is_connected(self):
        return self.connected

    def disconnect(self):
        if self.is_connected():
            try:
                self.sock.shutdown(socket.SHUT_RDWR)
            except:
                pass
            self.sock = None
            self.connected = False

    def send(self, data):
        if data is None or not self.is_connected():
            return
        try:
            self.sock.sendall(data)
        except:
            self.try_connect()
            self.sock.sendall(data)
        print("Sent: '" + str(data) + "'")