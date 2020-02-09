import threading

class DataArea:

    def __init__(self):
        self.data_in = []
        self.data_out = []
        self.data_cyclic = []
        self.updateLock = threading.Lock()
        self.data_pendingUpdate = []

    def addSignal(self, signal : object):
        # TBD: Check for duplicats
        if signal.signalConfig == Signal.SC_CYCLIC:
            self.data_cyclic.append(signal)
        if signal.signalConfig == Signal.SC_IN:
            self.data_in.append(signal)
        if signal.signalConfig == Signal.SC_OUT:
            self.data_out.append(signal)
        signal.setDataArea(self)

    def triggerUpdate(self, signal):
        # Mark a signal ready for sending
        self.updateLock.acquire()
        self.data_pendingUpdate.append(signal)
        self.updateLock.release()

    def getSignalsForSending(self):
        self.updateLock.acquire()
        ret = self.data_pendingUpdate
        self.data_pendingUpdate = []
        self.updateLock.release()
        return ret



class Signal:
    SC_CYCLIC   = 1
    SC_IN       = 2
    SC_OUT      = 3

    ST_BOOL     = 1
    ST_REAL32   = 2
    ST_INT16    = 3
    

    def __init__(self, sign_id : int, sub_id : int, signalConfig : int, signalType : int, name : str, value = None, params = None, cb = None, other : dict = None):
        self.sign_id = sign_id
        self.sub_id = sub_id
        self.signalConfig = signalConfig
        self.signalType = signalType
        self.name = name
        self.params = params
        self.value = value
        self.cb = cb

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

    def read_cb(self):
        if self.cb != None:
            self.cb(self)

    def pack(self) -> bytearray:
        frame = bytearray()
        frame.extend(bytes([self.sign_id//0x100, self.sign_id%0x100, self.sub_id%0x100]))
        if self.signalType == Signal.ST_BOOL:
            frame.extend(bytes([self.value%2]))
        else:
            raise NotImplementedError()
        return frame