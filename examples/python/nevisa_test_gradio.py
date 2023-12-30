import gradio as gr
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
    global response
    print('queue_report : message received with ', data)
    if(data['status']=='succeeded'):
        response = data
        sio.disconnect()

@sio.on('message')
def message(data):
    print('message : message received with ', data)

@sio.event
def disconnect():
    print('disconnect : disconnected from server')

# Login --------------------------------------
token, code = login("Your username", "Your password")

def transcribe(audio):
    # Add File for Recognition --------------------------------------
    files, code = add_file(token, audio)
    
    sio.connect(SERVER_ADDRESS, headers={'token': token, 'platform': 'browser'})

    sio.emit('start-file-process', {'files':files})

    sio.wait()

    # Final Result --------------------------------------

    if response['status'] == "succeeded":
        print(f"Final Result: {response['resultText']}")
        return response['resultText']

demo = gr.Interface(
    transcribe,
    gr.Audio(type="filepath"),
    "text",
)

demo.launch()
