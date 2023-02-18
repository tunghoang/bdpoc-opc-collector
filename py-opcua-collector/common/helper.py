from datetime import datetime, timedelta

def str_to_datetime(s, default=None):
    if not s:
        if default is not None:
            return default
        return datetime.utcnow()
    # FIXME: try different datetime formats
    for fmt in ["%Y-%m-%d", "%Y-%m-%d %H:%M", "%Y-%m-%d %H:%M:%S"]:
        try:
            return datetime.strptime(s, fmt)
        except ValueError:
            pass
