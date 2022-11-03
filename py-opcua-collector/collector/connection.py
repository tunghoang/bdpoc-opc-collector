# for typing
from typing import Optional, Union
from .subscriber import UaSubscriber

from asyncua import Client, ua
import common.logger as logging


class UaConnection:
    """docstring for UaConnection."""

    def __init__(self, c: Client):
        self.logger = logging.getLogger(__name__)
        self.c = c
        self._subs = []

    async def __aenter__(self):
        # await self.connect()
        return self

    async def __aexit__(self, exc_type, exc_value, traceback):
        pass
        # await self.disconnect()

    # async def connect(self, url: str):
    #     pass

    async def subscribe(self, params: Union[ua.CreateSubscriptionParameters, int, None]=None):
        if params is None or not isinstance(params, ua.CreateSubscriptionParameters):
            period = params or 1000
            params = ua.CreateSubscriptionParameters()
            params.RequestedPublishingInterval = period # type: ignore
            params.RequestedLifetimeCount = 10000 # type: ignore
            params.RequestedMaxKeepAliveCount = self.c.get_keepalive_count(1000)  # type: ignore
            params.MaxNotificationsPerPublish = 10000 # type: ignore
            params.PublishingEnabled = True # type: ignore
            params.Priority = 0 # type: ignore

        subr = UaSubscriber(self, params)
        await subr._create_subscription()

        return subr
    
    async def unsubscribe(self, subr: UaSubscriber):
        if subr._conn is not self:
            self.logger.warning("cannot unsubscribe subscriber that does not owned")

        await subr._destroy_subscription()

    async def unsubscribe_all(self):
        for subr in self._subs:
            await self.unsubscribe(subr)

    def _add_sub(self, subr: UaSubscriber):
        self._subs.append(subr)

    def _del_sub(self, subr: UaSubscriber):
        self._subs.remove(subr)

