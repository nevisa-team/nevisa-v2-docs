import requests

SERVER_ADDRESS = "https://ent.persianspeech.com"
LOGIN_API = "/api/auth/login"
ADD_FILE_API = '/api/files/add'
# API List: https://ent.persianspeech.com/api/docs


# Login --------------------------------------
def login(username, password):
    json_data = {
        'username': username,
        'password': password
    }
    response = requests.post(SERVER_ADDRESS + LOGIN_API,
                             json=json_data, verify=False)

    if response.status_code != 200:
        raise Exception("Login Failed!")

    code = response.json()['code']
    token = response.json()['token']
    return token, code

# Add File for Recognition --------------------------------------


def add_file(token, file_path, usePunctuations=False, useTextToNumber=False, userPlatform='browser'):
    json_data = {
        'usePunctuations': 'true' if usePunctuations else 'false',
        'useTextToNumber': 'true' if useTextToNumber else 'false',
        'userPlatform': userPlatform
    }
    headers = {
        'authorization': token
    }
    with open(file_path, 'rb') as file:
        files = [
            ('files', (file_path, file, ))
        ]
        response = requests.post(SERVER_ADDRESS + ADD_FILE_API,
                                 data=json_data, files=files, headers=headers, verify=False)

    result = 'Success' if response.status_code == 200 else 'Failure'
    print(f"add_file result: {result}", response.json(), sep='\n')

    if result == 'Failure':
        raise Exception("add_file Failed!")
    code = response.json()['code']
    files = response.json()['files']
    return files, code
