import logging
logging.basicConfig(
    format='%(asctime)s %(levelname)s %(message)s',
    level=logging.ERROR,
    datefmt='%Y-%m-%d %H:%M:%S')

def getLogger(name):
    log = logging.getLogger(name)
    log.setLevel(logging.INFO)
    
    return log
