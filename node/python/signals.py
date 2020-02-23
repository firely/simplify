# -*- coding: utf-8 -*-

import threading
import codecs

class DataArea:

    def __init__(self):
        self.data_in = []
        self.data_out = []
        self.data_cyclic = []
        self.updateLock = threading.Lock()
        self.data_pendingUpdate = []
        self.data_configToSend = []

    def addSignal(self, signal : object):
        # TBD: Check for duplicats
        if signal.signalConfig == Signal.SC_CYCLIC:
            self.data_cyclic.append(signal)
        if signal.signalConfig == Signal.SC_IN:
            self.data_in.append(signal)
        if signal.signalConfig == Signal.SC_OUT:
            self.data_out.append(signal)
        signal.setDataArea(self)
        signal.queue_send_config_to_server() # Have signal information sent across network


    def triggerUpdate(self, signal):
        # Mark a signal ready for sending
        self.updateLock.acquire()
        self.data_pendingUpdate.append(signal)
        self.updateLock.release()

    def triggerSendConfig(self, signal):
        # Mark a signal ready for sending
        self.updateLock.acquire()
        self.data_configToSend.append(signal)
        self.updateLock.release()

    def getSignalsForSending(self):
        self.updateLock.acquire()
        ret = self.data_pendingUpdate
        self.data_pendingUpdate = []
        config = self.data_configToSend
        self.data_configToSend = []
        self.updateLock.release()
        return config, ret



class Signal(object):
    """Wraps a variable to be sent across the network."""

    SC_CYCLIC   = 1
    SC_IN       = 2
    SC_OUT      = 3

    ST_BOOL     =  1  # Data types that can fit in a single byte has number 1 - 20
    ST_INT16    = 21  # Data types that can fir in two bytes has number 21-40
    ST_REAL32   = 41  # etc..

    def __init__(self, sign_id: int, sub_id: int, signalConfig: int, signalType: int, name: str, value=None, params=None, cb=None, other: dict = None):
        self.sign_id = sign_id
        self.sub_id = sub_id
        self.signalConfig = signalConfig
        self.signalType = signalType
        self.name = name
        self.params = params
        self.value = value
        self.cb = cb
        self.config_bytes = Signal.generate_config_data(self)

    def __str__(self):
        return self.name + " (" + str(self.value) + ")"

    def write(self, value):
        if self.signalConfig != Signal.SC_IN:
            self.value = value
            if self.signalConfig != Signal.SC_CYCLIC:
                self.dataArea.triggerUpdate(self)
        else:
            raise Exception("Not possible to write to an input signal")
    
    def read(self):
        return self.value

    def setDataArea(self, dataArea):
        self.dataArea = dataArea

    def queue_send_config_to_server(self):
        self.dataArea.triggerSendConfig(self)

    def read_cb(self):
        if self.cb != None:
            self.cb(self)

    def pack(self, config = False) -> bytearray:
        if config:
            return self.config_bytes
        frame = bytearray()
        frame.extend(bytes([self.sign_id//0x100, self.sign_id%0x100, self.sub_id%0x100]))
        frame.extend(bytes([self.signalType%0x100])) # Type
        if self.signalType == Signal.ST_BOOL:
            frame.extend(bytes([self.value%2]))
        else:
            raise NotImplementedError()
        return frame

    @ staticmethod
    def generate_config_data(signal):
        """
        Create a byte array that fully describes the signal and its settings
        """
        frame = bytearray()
        frame.extend(bytes([0x00, 0x00])) # A config frame starts with double zero
        id = bytes([signal.sign_id//0x100, signal.sign_id%0x100, signal.sub_id%0x100]) # ID
        frame.extend(id) # ID
        config = bytes([signal.signalConfig%0x100]) # Config
        frame.extend(config) # Config
        type_ = bytes([signal.signalType%0x100])
        frame.extend(type_) # Type
        name = codecs.utf_16_encode(signal.name)[0] # Name
        frame.extend([len(name)]) # Name length
        frame.extend(name) # Name
        return frame
