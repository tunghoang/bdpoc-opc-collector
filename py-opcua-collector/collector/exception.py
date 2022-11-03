
class CollectorException(Exception):
    pass


class UaSubscriberException(CollectorException):
    pass

class UnInitializedSubscription(UaSubscriberException):
    pass
