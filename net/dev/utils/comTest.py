import zmq
import sys

contextOut = zmq.Context()
DSSout = contextOut.socket(zmq.REQ)

print 'Opening Connection to tcp://%s:%s' % (sys.argv[1],sys.argv[2])
DSSout.connect('tcp://%s:%s' % (sys.argv[1],sys.argv[2]))


def do_com(request):
    req_bytes=request.encode('utf-8')
    DSSout.send(req_bytes)
    status=DSSout.recv()
    data = status.decode('utf-8')
    logging.info('reply recieved %s: ' % data)
    return data

print('trying')
do_com('update sent from net')
