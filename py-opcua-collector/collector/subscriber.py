# for typing
from typing import TYPE_CHECKING, Any
if TYPE_CHECKING:
    from connection import *

from asyncua import ua
import common.logger as logging
from .exception import *


class UaSubscriber:
    """docstring for Subscriber."""

    def __init__(self, conn, create_sub_params: ua.CreateSubscriptionParameters):
        self.logger = logging.getLogger(__name__)
        self.create_sub_params = create_sub_params
        self._conn: UaConnection = conn  # type: ignore

        self._ondata_handlers = []
        self._items = []  # watching node items
        self._subscription = None
        self._handle = None

        self.metadata: Any = None

    def sid(self):
        if self._subscription is None:
            raise UnInitializedSubscription("subscription hasn't created")
        
        return self._subscription.subscription_id

    async def _create_subscription(self):
        if self._subscription is not None:
            self.logger.warn(
                "already created subscription, if intent to re-create, must call _destroy_subscription first")
            return

        self._subscription = await self._conn.c.create_subscription(self.create_sub_params, self)
        self._conn._add_sub(self)

    async def _destroy_subscription(self):
        if self._subscription is None:
            return

        await self._unsub()
        await self._subscription.delete()
        self._subscription = None
        self._conn._del_sub(self)

    async def _sub(self):
        if self._subscription is None:
            raise UnInitializedSubscription("subscription hasn't created")

        if self._handle is not None:
            self.logger.warn(
                "already subscribed, if intent to re-subscribe, must call _unsub first")
            return

        if len(self._items) == 0:
            return

        self._handle = await self._subscription.subscribe_data_change(self._items)

    async def _unsub(self):
        if self._handle is None:
            return

        await self._subscription.unsubscribe(self._handle)  # type: ignore
        self._handle = None

    async def add_items(self, items):
        if isinstance(items, list):
            self._items.extend(items)
        else:
            self._items.append(items)

        # reload if needed
        if self._handle is not None:
            await self._unsub()
            await self._sub()

    async def start(self):
        await self._sub()

    async def stop(self):
        await self._unsub()

    def on_data(self, handler):
        if isinstance(handler, list):
            self._ondata_handlers.extend(handler)
        else:
            self._ondata_handlers.append(handler)

    def datachange_notification(self, node, val, data):
        """asyncua SubHandler implementation"""
        self.logger.debug("on data change. node=%s val=%s ts=%s", node, val, data.monitored_item.Value.SourceTimestamp)
        for handler in self._ondata_handlers:
            handler(self, node, val, data)

    def event_notification(self, event):
        """asyncua SubHandler implementation"""
        pass

    def status_change_notification(self, status: ua.StatusChangeNotification):
        """asyncua SubHandler implementation"""
        pass
