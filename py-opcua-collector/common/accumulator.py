
class BufferEmpty(Exception):
    pass


class BufferFull(Exception):
    pass


class Buffer:
    """'Circular' Buffer, are not thread-safe."""

    def __init__(self, maxsize: int = 0):
        self.maxsize = maxsize
        self._buf = [None] * maxsize
        self._len = 0
        self._last = -1

    @property
    def length(self):
        """buffer current length."""
        return self._len

    def empty(self):
        return self.length == 0

    def full(self):
        return self.length == self.maxsize

    def add(self, items):
        dropped = 0

        if isinstance(items, list):
            for item in items:
                dropped += self._add_one(item)
        else:
            dropped = self._add_one(items)

        return dropped

    def _add_one(self, item):
        dropped = 0

        self._last = self.next(self._last)
        if self.full():
            dropped += 1
        else:
            self._len += 1

        self._buf[self._last] = item

        return dropped

    def next(self, index):
        index += 1

        return index % self.maxsize
    
    def get(self):
        if self.empty():
            raise BufferEmpty()

        i = (self._last - self._len + 1) % self.maxsize
        item = self._buf[i]
        self._len -= 1

        return item

    def flush(self):
        # ASK: check empty, is needed?
        items = [None]*self._len
        start = (self._last - self._len + 1) % self.maxsize

        for i in range(self._len):
            items[i] = self._buf[start]
            start = self.next(start)

        self._len = 0

        return items
