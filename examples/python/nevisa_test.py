from nevisa_file_api import *
import socketio

#sio = socketio.Client(logger=True, engineio_logger=True) # For Debugging
sio = socketio.Client()

@sio.event()
def connect():
    print('connect : connection established')

@sio.on('start-file-process')
def start_file_process(data):
    print('start_file_process : message received with ', data)

@sio.on('queue-report')
def queue_report(data):
    print('queue_report : message received with ', data)

@sio.on('message')
def message(data):
    print('message : message received with ', data)

@sio.event
def disconnect():
    print('disconnect : disconnected from server')


if __name__ == '__main__':

    # The path of your file (Common audio or video formats are accepted.)
    FILE_PATH = 'test.wav'

    # Login --------------------------------------
    token, code = login("Your username", "Your password")

    # Add File for Recognition --------------------------------------
    files, code = add_file(token, FILE_PATH)

    sio.connect(SERVER_ADDRESS, headers={'token': token, 'platform': 'browser'})

    sio.emit('start-file-process', {'files':files})

    sio.wait()
