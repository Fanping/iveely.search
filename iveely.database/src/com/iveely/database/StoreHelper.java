/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.database;

import com.eclipsesource.json.JsonArray;
import com.eclipsesource.json.JsonObject;
import com.eclipsesource.json.JsonValue;
import com.iveely.database.api.template.Commander;
import com.iveely.database.api.template.ExchangeCode;
import com.iveely.database.common.Message;
import com.iveely.database.storage.Types;
import com.iveely.framework.net.Packet;
import com.iveely.framework.net.SyncServer;
import com.iveely.framework.text.JsonUtil;

import static com.iveely.database.storage.Types.BOOLEAN;
import static com.iveely.database.storage.Types.CHAR;
import static com.iveely.database.storage.Types.DOUBLE;
import static com.iveely.database.storage.Types.FLOAT;
import static com.iveely.database.storage.Types.IMAGE;
import static com.iveely.database.storage.Types.INTEGER;
import static com.iveely.database.storage.Types.LONG;
import static com.iveely.database.storage.Types.SHORTSTRING;
import static com.iveely.database.storage.Types.STRING;
import static com.iveely.database.storage.Types.UNKOWN;
import com.iveely.database.storage.Warehouse;
import java.util.ArrayList;
import java.util.List;

/**
 *
 * @author X1 Carbon
 */
public class StoreHelper implements SyncServer.ICallback {

	public Packet invoke(Packet packet) {
		String jsonValue = Message.getString(packet.getData());
		if (packet.getExecutType() == Commander.CREATETABLE.ordinal()) {
			return createTable(jsonValue);
		} else if (packet.getExecutType() == Commander.INSERTONE.ordinal()) {
			return insertOne(jsonValue);
		} else if (packet.getExecutType() == Commander.INSERTMANY.ordinal()) {
			return insertMany(jsonValue);
		} else if (packet.getExecutType() == Commander.SELECTONE.ordinal()) {
			return selectOne(jsonValue);
		} else if (packet.getExecutType() == Commander.SELECTMANY.ordinal()) {
			return selectMany(jsonValue);
		} else if (packet.getExecutType() == Commander.CLOSE.ordinal()) {
			return close(jsonValue);
		} else if (packet.getExecutType() == Commander.DROPTABLE.ordinal()) {
			return dropTable(jsonValue);
		} else if (packet.getExecutType() == Commander.COUNT.ordinal()) {
			return countTable(jsonValue);
		}
		return Packet.getUnknowPacket();
	}

	private Packet dropTable(String jsonValue) {
		String dbName = JsonObject.readFrom(jsonValue).getString("dbName", "");
		String tableName = JsonObject.readFrom(jsonValue).getString("tableName", "");
		Warehouse warehouse = LocalStore.getWarehouse(dbName);
		int type = ExchangeCode.DROP_TABLE_FALURE.ordinal();
		String responseInfor = "Falure";
		if (warehouse.dropTable(tableName)) {
			type = ExchangeCode.DROP_TABLE_SUCCESS.ordinal();
			responseInfor = "Success";
		}
		Packet resPacket = new Packet();
		resPacket.setMimeType(0);
		resPacket.setExecutType(type);
		resPacket.setData(Message.getBytes(responseInfor));
		return resPacket;
	}

	private Packet countTable(String jsonValue) {
		String dbName = JsonObject.readFrom(jsonValue).getString("dbName", "");
		String tableName = JsonObject.readFrom(jsonValue).getString("tableName", "");
		Warehouse warehouse = LocalStore.getWarehouse(dbName);
		int type = ExchangeCode.COUNT_TABLE_FALURE.ordinal();
		String responseInfor = "Falure";
		int count = warehouse.count(tableName);
		if (count > -1) {
			type = ExchangeCode.COUNT_TABLE_SUCCESS.ordinal();
			responseInfor = count + "";
		}
		Packet resPacket = new Packet();
		resPacket.setMimeType(0);
		resPacket.setExecutType(type);
		resPacket.setData(Message.getBytes(responseInfor));
		return resPacket;
	}

	private Packet close(String dbName) {
		Warehouse warehouse = LocalStore.getWarehouse(dbName);
		warehouse.close();
		return Packet.getUnknowPacket();
	}

	/**
	 * Command of create table.
	 *
	 * @param jsonValue
	 * @return
	 */
	private Packet createTable(String jsonValue) {

		// 1. Parse information.
		String dbName = JsonObject.readFrom(jsonValue).getString("dbName", "");
		String tableName = JsonObject.readFrom(jsonValue).getString("tableName", "");
		JsonArray columnArray = JsonObject.readFrom(jsonValue).get("columns").asArray();

		String[] columns = new String[columnArray.size()];
		for (int i = 0; i < columnArray.size(); i++) {
			columns[i] = columnArray.get(i).asString();
		}

		JsonArray typeArray = JsonObject.readFrom(jsonValue).get("types").asArray();
		Integer[] types = new Integer[typeArray.size()];
		for (int i = 0; i < typeArray.size(); i++) {
			types[i] = Integer.parseInt(typeArray.get(i).asString());
		}

		JsonArray uniqueArray = JsonObject.readFrom(jsonValue).get("uniques").asArray();
		boolean[] uniques = new boolean[uniqueArray.size()];
		for (int i = 0; i < uniqueArray.size(); i++) {
			String u = uniqueArray.get(i).asString();
			uniques[i] = u.equals("true");
		}

		// 2. Check to record.
		boolean isSuccess = true;
		Warehouse warehouse = LocalStore.getWarehouse(dbName);
		isSuccess = isSuccess && warehouse.createTable(tableName);
		for (int i = 0; i < uniques.length; i++) {
			isSuccess = isSuccess && warehouse.createColumn(tableName, columns[i], getById(types[i]), uniques[i]);
		}

		// 3. Reponse to client.
		String responseInfor = "Create table[" + tableName + "] success.";
		int type = ExchangeCode.TABLE_CREATE_SUCCESS.ordinal();
		if (!isSuccess) {
			responseInfor = "Create failed, may be table is exist or table name is illegal.";
			type = ExchangeCode.TABLE_CREATE_FALURE.ordinal();
		}
		Packet resPacket = new Packet();
		resPacket.setMimeType(0);
		resPacket.setExecutType(type);
		resPacket.setData(Message.getBytes(responseInfor));
		return resPacket;
	}

	/**
	 * Insert a data to disk.
	 *
	 * @param jsonValue
	 * @return
	 */
	private Packet insertOne(String jsonValue) {
		String resp = "empty";
		int id = -1;
		try {
			// 1. Parse information.
			String dbName = JsonObject.readFrom(jsonValue).getString("dbName", "");
			String tableName = JsonObject.readFrom(jsonValue).getString("tableName", "");
			JsonArray dataArray = JsonObject.readFrom(jsonValue).get("values").asArray();
			String[] values = new String[dataArray.size()];
			for (int i = 0; i < dataArray.size(); i++) {
				values[i] = dataArray.get(i).asString();
			}

			// 2. Store data.
			Warehouse warehouse = LocalStore.getWarehouse(dbName);
			id = warehouse.insert(tableName, values);
		} catch (Exception e) {
			resp = e.getMessage();
		}

		Packet packet = new Packet();
		packet.setExecutType(id);
		packet.setData(Message.getBytes(resp));
		return packet;
	}

	private Packet insertMany(String jsonValue) {
		String resp;
		int type = ExchangeCode.INSERT_MANY_FALURE.ordinal();
		try {
			// 1. Parse information.
			String dbName = JsonObject.readFrom(jsonValue).getString("dbName", "");
			String tableName = JsonObject.readFrom(jsonValue).getString("tableName", "");
			List<JsonValue> result = JsonObject.readFrom(jsonValue).get("values").asArray().values();
			Integer[] ids = new Integer[result.size()];
			Warehouse warehouse = LocalStore.getWarehouse(dbName);
			for (int i = 0; i < result.size(); i++) {
				JsonArray dataArray = result.get(i).asArray();
				String[] values = new String[dataArray.size()];
				for (int j = 0; j < dataArray.size(); j++) {
					values[j] = dataArray.get(j).asString();
				}
				ids[i] = warehouse.insert(tableName, values);
			}
			resp = JsonUtil.objectToJson(ids);
			type = ExchangeCode.INSERT_MANY_SUCCESS.ordinal();

			// 2. Store data.
		} catch (Exception e) {
			resp = e.getMessage();
		}

		Packet packet = new Packet();
		packet.setExecutType(type);
		packet.setData(Message.getBytes(resp));
		return packet;
	}

	private Packet selectOne(String jsonValue) {
		// 1. Parse information.
		String dbName = JsonObject.readFrom(jsonValue).getString("dbName", "");
		String tableName = JsonObject.readFrom(jsonValue).getString("tableName", "");
		Integer id = Integer.parseInt(JsonObject.readFrom(jsonValue).getString("id", "-1"));

		// 2. Get data.
		Warehouse warehouse = LocalStore.getWarehouse(dbName);
		Object[] objs = warehouse.selectById(tableName, id);
		int type = ExchangeCode.SELECT_ONE_FALURE.ordinal();
		String respData = "Falure";
		if (objs != null) {
			type = ExchangeCode.SELECT_ONE_SUCCESS.ordinal();
			List<Object[]> list = new ArrayList<>();
			list.add(objs);
			respData = JsonUtil.listToJson(list);
		}

		// 3. Build packet.
		Packet packet = new Packet();
		packet.setExecutType(type);
		packet.setData(Message.getBytes(respData));
		return packet;
	}

	private Packet selectMany(String jsonValue) {
		// 1. Parse information.
		String dbName = JsonObject.readFrom(jsonValue).getString("dbName", "");
		String tableName = JsonObject.readFrom(jsonValue).getString("tableName", "");
		Integer id = Integer.parseInt(JsonObject.readFrom(jsonValue).getString("startIndex", "-1"));
		Integer count = Integer.parseInt(JsonObject.readFrom(jsonValue).getString("count", "-1"));

		// 2. Get data.
		Warehouse warehouse = LocalStore.getWarehouse(dbName);
		int type = ExchangeCode.SELECT_MANY_FALURE.ordinal();
		String respData = "Falure";
		List<Object[]> list = new ArrayList<>();
		for (int i = id; i < id + count; i++) {
			Object[] objs = warehouse.selectById(tableName, i);
			if (objs != null) {
				list.add(objs);
			} else {
				list.add(new Object[] { "null" });
			}
		}
		if (list.size() > 0) {
			type = ExchangeCode.SELECT_MANY_SUCCESS.ordinal();
			respData = JsonUtil.listToJson(list);
		}

		// 3. Build packet.
		Packet packet = new Packet();
		packet.setExecutType(type);
		packet.setData(Message.getBytes(respData));
		return packet;
	}

	private Types getById(int id) {
		switch (id) {
		case 0:
			return INTEGER;
		case 1:
			return LONG;
		case 2:
			return DOUBLE;
		case 3:
			return BOOLEAN;
		case 4:
			return STRING;
		case 5:
			return FLOAT;
		case 6:
			return CHAR;
		case 7:
			return SHORTSTRING;
		case 8:
			return IMAGE;
		default:
			return UNKOWN;
		}

	}
}
