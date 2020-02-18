from simple_socket import simple_socket
from signals import *
import threading
import time
import crcmod

class Node:
    def __init__(self, address, port = None):
        self.socket = simple_socket(address, port)
        self.thread = None
        self.stopQueue = []
        self.stopLock = threading.Lock()
        self.sendQueue = []
        self.sendLock = threading.Lock()
        self.sendQueue_async = []
        self.dataArea = DataArea()
        self.calcCRC = crcmod.mkCrcFun(0x18005, rev=False, initCrc=0x0000, xorOut=0xFFFF)

    def async_action(self):
        if not self.socket.is_connected():
            self.socket.try_connect()
        
        if self.socket.is_connected():
            # Message queue
            self.sendLock.acquire()
            self.sendQueue_async.extend(self.sendQueue)
            self.sendQueue = []
            self.sendLock.release()
            for msg in self.sendQueue_async:
                self.socket.send(msg)
            self.sendQueue_async = []

            # Update signals
            signs = self.dataArea.getSignalsForSending()
            self.socket.send(self._packdata_(signs[0], config = True))
            self.socket.send(self._packdata_(signs[1]))

    def async_close(self):
        self.socket.disconnect()

    def _packdata_(self, data, config = False) -> bytearray:
        if len(data) == 0:
            return None
        frame = bytearray()
        for s in data:
            p = s.pack(config)
            if p is not None:
                frame.extend(p)

        # TBD: Add headers 
        L = len(frame)+4
        if L > 0xFFFF:
            raise Exception("Framebuffer overflow")
        B = bytes([L//0x100, L%0x100])
        frame.insert(0, B[1])
        frame.insert(0, B[0])
        crc = self.calcCRC(frame)
        frame.extend(bytes([(crc//0x100) & 0xFF, crc & 0xFF]))
        return frame

    def _unpackdata_(self, data):
        return None

    def start(self):
        if self.thread is not None and self.thread.isAlive():
            print("Function already running - cannot start")
            return
        self.thread = threading.Thread(target=self._async_thread_)
        self.thread.setDaemon(True)
        self.thread.start()

    def stop(self, timeout = None):
        # There is a thread to wait for
        if self.thread is not None:
            # Command it to stop
            self.stopLock.acquire()
            self.stopQueue.append(1)
            self.stopLock.release()
            # Wait for thread to end
            self.thread.join(timeout)
            self.thread = None
        return

    def running(self):
        return self.thread is not None and self.thread.isAlive()

    def list_me(self):
        print(threading.enumerate())

    def send(self, data):
        self.sendLock.acquire()
        self.sendQueue.append(data)
        self.sendLock.release()
        return

    def _async_thread_(self):
        print("Thread started")
        while True:
            self.stopLock.acquire()
            if len(self.stopQueue) > 0:
                self.stopQueue.pop()
                break
            self.stopLock.release()
            self.async_action()
            time.sleep(0.02)
        self.stopLock.release()
        self.async_close()
        print("Thread stopped")

    def AddSignal(self, signal):
        self.dataArea.addSignal(signal)

    def RemoveSignal(self, signal):
        raise NotImplementedError()

if __name__ == "__main__":
    # node = Node('127.0.0.1', 13010)
    node = Node('192.168.10.183', 80)
    # node = Node('www.python.org', 80)
    # Setup things
    sign1 = Signal(0, 1, Signal.SC_OUT, Signal.ST_BOOL, "Tap1")
    sign2 = Signal(1, 1, Signal.SC_OUT, Signal.ST_BOOL, "Tap2", value=True)
    node.AddSignal(sign1)
    node.AddSignal(sign2)

    def status():
        print('Running' if node.running() else 'Stopped')

    def hello_world():
        node.send(b'Hello World')

    def flip():
        sign1.write(not sign1.read())
        sign2.write(not sign2.read())

    commands = {}
    commands['start'] = node.start
    commands['stop']  = node.stop
    commands['status'] = status
    commands['list'] = node.list_me
    commands['hello world'] = hello_world
    commands['flip'] = flip


    cmd = input(">> " )

    while(cmd != 'exit' and cmd != 'quit'):
        # Do stuff
        if cmd in commands:
            commands[cmd]()
        else:
            print('Undefined command!')

        # Get new input
        cmd = input(">> ")

    # Kill node to release resources
    node.stop()