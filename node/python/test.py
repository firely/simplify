from node import Node
from signals import Signal
import threading
import time

if __name__ == "__main__":
    # node = node.Node('192.168.10.183', 80)
    node = Node('127.0.0.1', 10030)

    sign = Signal(1, 1, Signal.SC_OUT, Signal.ST_BOOL, 'Flip', value=False)
    node.addsignal(sign)

    lock = threading.Lock()
    msg = []

    def async_update_loop():
        """Function periodically updates variables."""
        time.sleep(0.1)
        node.start()
        while True:  # Loop and update values each cycle
            lock.acquire()
            if len(msg) > 0 and msg.pop() == 'stop':
                break
            lock.release()
            time.sleep(0.5)
            sign.write(not sign.read())
        lock.release()
        node.stop()

    thread = threading.Thread(target=async_update_loop)
    thread.start()

    print("[Press Enter to exit]")
    input()

    lock.acquire()
    msg.append('stop')
    lock.release()

    thread.join(5)
