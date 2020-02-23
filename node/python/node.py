# -*- coding: utf-8 -*-

"""Module defining the entry point to creating a node instance."""

from simple_socket import simple_socket
from signals import DataArea, Signal
from crcmod import mkCrcFun
import threading
import time


class Node (object):
    """Class allowing for syncronizing of data over network."""

    def __init__(self, address, port=None):
        """Default constructor."""
        self.socket = simple_socket(address, port)
        self.thread = None
        self.stopQueue = []
        self.stopLock = threading.Lock()
        self.sendQueue = []
        self.sendLock = threading.Lock()
        self.sendQueue_async = []
        self.dataArea = DataArea()
        poly = 0x18005
        crct = 0
        out = 0xFFFF
        self.calcCRC = mkCrcFun(poly, rev=False, initCrc=crct, xorOut=out)

    def async_action(self):
        """Function to be run asyncronisly."""
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
            self.socket.send(self._packdata(signs[0], config=True))
            self.socket.send(self._packdata(signs[1]))

    def async_close(self):
        """Disconnect Node, function run by async thread."""
        self.socket.disconnect()

    def _packdata(self, data, config=False) -> bytearray:
        if len(data) == 0:
            return None
        frame = bytearray()
        for signal in data:
            packedsignal = signal.pack(config)
            if packedsignal is not None:
                frame.extend(packedsignal)

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

    def _unpackdata(self, data):
        raise NotImplementedError()

    def start(self):
        if self.thread is not None and self.thread.isAlive():
            print("Function already running - cannot start")
            return
        self.thread = threading.Thread(target=self._async_thread)
        self.thread.setDaemon(True)
        self.thread.start()

    def stop(self, timeout=None):
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
        """Returns all running threads."""
        return [str(thread) for thread in threading.enumerate()]

    def send(self, data):
        self.sendLock.acquire()
        self.sendQueue.append(data)
        self.sendLock.release()

    def _async_thread(self):
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

    def addsignal(self, signal):
        """Add signal to node."""
        self.dataArea.addSignal(signal)

    def removesignal(self, signal):
        """Remove signal from node."""
        raise NotImplementedError()


if __name__ == '__main__':
    port = 13010  # 13010 or 80
    node = Node('192.168.10.183', port)

    sign1 = Signal(0, 1, Signal.SC_OUT, Signal.ST_BOOL, 'Tap1')
    sign2 = Signal(1, 1, Signal.SC_OUT, Signal.ST_BOOL, 'Tap2', value=True)
    node.addsignal(sign1)
    node.addsignal(sign2)

    def status():
        """Request status."""
        print('Running' if node.running() else 'Stopped')

    def flip():
        """Flip the two bytes listed above."""
        sign1.write(not sign1.read())
        sign2.write(not sign2.read())

    commands = {}
    commands['start'] = node.start
    commands['stop']  = node.stop
    commands['status'] = status
    commands['list'] = node.list_me
    commands['flip'] = flip

    cmd = input('>> ')

    while cmd != 'exit' and cmd != 'quit':
        # Do stuff
        if cmd in commands:
            commands[cmd]()
        else:
            print('Undefined command!')

        # Get new input
        cmd = input('>> ')

    # Kill node to release resources
    node.stop()