import node
from signals import Signal
import threading, time

if __name__ == "__main__":
    node = node.Node('192.168.10.183', 80)

    sign = Signal(0, 1, Signal.SC_OUT, Signal.ST_BOOL, "Flip", value = False)
    node.AddSignal(sign)

    lock = threading.Lock()
    msg = []
    def _async_update_loop_():
        node.start()
        while True: # Loop and update values each cycle
            lock.acquire()
            if len(msg) > 0 and msg.pop() == 'stop':
                break
            lock.release()
            time.sleep(0.5)
            sign.write(not sign.read())
        lock.release()
        node.stop()
        return


    thread = threading.Thread(target= _async_update_loop_)
    thread.start()

    input("[Press Enter to exit]")

    lock.acquire()
    msg.append('stop')
    lock.release()

    thread.join(5)