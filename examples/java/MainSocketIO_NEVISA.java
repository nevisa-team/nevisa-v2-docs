package com.nevisa.socketIO;

import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import io.socket.client.IO;
import io.socket.client.Socket;
import io.socket.emitter.Emitter;

public class MainSocketIO_NEVISA {

	public static void main(String[] args) throws URISyntaxException, InterruptedException, JSONException {

		IO.Options options = new IO.Options();
		Map<String, List<String>> headers = new HashMap<>();

		List<String> strList = new ArrayList<>();
		strList.add(
				"eyJhbGciOiJXPN0eFfHQ");
		headers.put("token", strList);
		List<String> strList2 = new ArrayList<>();
		strList2.add("browser");
		headers.put("platform", strList2);

		options.extraHeaders = headers;
		final Socket socket = IO.socket("https://ent.persianspeech.com/", options);

		socket.on(Socket.EVENT_CONNECT, new Emitter.Listener() {

			@Override
			public void call(Object... args) {
				System.out.println("Connected");

				JSONObject msg = new JSONObject();
				try {
					msg.put("id", "6550be8e9fcb40d05");
				} catch (JSONException e) {
					e.printStackTrace();
				}

				List<JSONObject> jsonList = new ArrayList<>();
				jsonList.add(msg);

				JSONObject obj = new JSONObject();
				try {
					obj.put("files", new JSONArray(jsonList));
				} catch (JSONException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

				socket.emit("start-file-process", obj);
			}

		});
		
		socket.on("start-file-process", new Emitter.Listener() {
			@Override
			public void call(Object... args) {
				System.out.println("start-file-process : " + args[0]);
				JSONObject jsonObj;
				boolean lockChecked = false;
				try {
					jsonObj = new JSONObject(args[0].toString());
					lockChecked = jsonObj.getBoolean("lockChecked");
					if (!lockChecked) {
						// خطای ترد پارتی

						return;
					}
				} catch (JSONException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}

		});
		
		socket.on("message", new Emitter.Listener() {
			@Override
			public void call(Object... args) {

				try {
					JSONObject jsonObj = new JSONObject(args[0].toString());
					if (!jsonObj.isNull("code")) {

						String code = jsonObj.getString("code");

						// خطای ترد پارتی

						return;
					}
				} catch (JSONException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}

		});
		
		Emitter tmp = socket.on("queue-report", new Emitter.Listener() {
			private String recText = "";
			
			@Override
			public void call(Object... args) {

				try {

					JSONObject jsonObj = new JSONObject(args[0].toString());
					if (jsonObj.isNull("status")) {

						String code = jsonObj.getString("code");
						System.out.println("queue-report : " + args[0]);

//خطایی رخ داده است
						// خطای تردپارتی

						return;
					}

					String status = jsonObj.getString("status");

					if (status.equalsIgnoreCase("succeeded")) {
						
						recText = jsonObj.getString("resultText");
						
						socket.disconnect();
					}
				} catch (JSONException e) {

					e.printStackTrace();
				}

			}
		});
		
		socket.on(Socket.EVENT_DISCONNECT, new Emitter.Listener() {

			@Override
			public void call(Object... args) {
				System.out.println("Socket disconnected");
			}

		});
		
		socket.on(Socket.EVENT_CONNECT_ERROR, new Emitter.Listener() {
			@Override
			public void call(Object... args) {
				System.out.println("Error In Socket Connection " + args[0]);
			}
		});

		socket.connect();
		
	}

}
