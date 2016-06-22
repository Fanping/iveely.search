package com.iveely.database;

import com.iveely.database.common.Configurator;
import com.iveely.database.common.Validator;
import com.iveely.database.storage.Warehouse;
import com.iveely.framework.file.Folder;

import java.io.File;
import java.io.IOException;
import java.util.Set;
import java.util.TreeMap;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Local storage.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-27 17:17:35
 */
public class LocalStore {

	/**
	 * All warehouses.
	 */
	private static TreeMap<String, Warehouse> houses;

	/**
	 * Warehouse path.
	 */
	private static final String houseFolder = "Warehouses/idb.cfg";

	/**
	 * Root folder.
	 */
	private static final String root = "Warehouses";

	/**
	 * Get root of database.
	 *
	 * @return
	 */
	public static String getRoot() {
		return root;
	}

	/**
	 * Get databases.
	 *
	 * @return
	 */
	public static String[] getDatabases() {
		// 1. Load from config file.
		if (houses == null) {
			deserialize();
			if (houses == null) {
				houses = new TreeMap<String, Warehouse>();
			}
		}

		// 2. Get names.
		Set<String> keys = houses.keySet();
		String[] names = new String[keys.size()];
		names = keys.toArray(names);
		return names;
	}

	public static boolean isWarehouseExist(String houseName) {
		// 1. Load from config file.
		if (houses == null) {
			deserialize();
			if (houses == null) {
				houses = new TreeMap<>();
			}
		}
		return houses.containsKey(houseName);
	}

	/**
	 * Get warehouse as database. If not have ,create one.
	 *
	 * @param houseName
	 * @return
	 */
	public static Warehouse getWarehouse(String houseName) {
		// 1. Load from config file.
		if (houses == null) {
			deserialize();
			if (houses == null) {
				houses = new TreeMap<>();
			}
		}

		// 2. Get warehouse.
		if (houses.containsKey(houseName)) {
			return houses.get(houseName);
		} else {
			if (Validator.isLegal(houseName)) {
				File tRoot = new File(getRoot());
				if (!tRoot.exists()) {
					tRoot.mkdir();
				}
				File child = new File(getRoot() + "/" + houseName);
				if (!child.exists()) {
					child.mkdir();
					serialize();
				}
				Warehouse house = new Warehouse(houseName);
				houses.put(houseName, house);
				serialize();
				return house;
			}
		}
		return null;
	}

	/**
	 * Drop warehouse.
	 *
	 * @param houseName
	 * @return
	 */
	public static boolean dropWarehouse(String houseName) {
		if (houses != null && houses.containsKey(houseName)) {
			// TODO:What's means?
			Warehouse house = houses.get(houseName);
			house = null;
			houses.remove(houseName);
			Folder.deleteDirectory(getRoot() + "/" + houseName);
			serialize();
			return true;
		}
		return false;
	}

	/**
	 * Back up warehouse.
	 *
	 * @param houseName
	 * @param backupName
	 * @return
	 */
	public static boolean backupWarehouse(String houseName, String backupName) {
		if (houses != null && houses.containsKey(houseName) && Validator.isLegal(backupName)) {
			try {
				Warehouse h = houses.get(houseName);
				h.close();
				Folder.copyDirectiory(getRoot() + "/" + houseName, getRoot() + "/" + backupName);
				String lastBakName = h.getLastBakUp();
				if (!lastBakName.isEmpty()) {
					Folder.deleteDirectory(getRoot() + "/" + lastBakName);
				}
				h.setLastBakUp(getRoot() + "/" + backupName);
				return true;
			} catch (IOException ex) {
				Logger.getLogger(LocalStore.class.getName()).log(Level.SEVERE, null, ex);
			}
		}
		return false;
	}

	/**
	 * Serialize to config file.
	 */
	public static void serialize() {
		if (houses != null) {
			Configurator.save(houseFolder, houses);
		}
	}

	/**
	 * Deserialize from config file.
	 */
	private static void deserialize() {
		Object obj = Configurator.load(houseFolder);
		if (obj != null) {
			houses = (TreeMap<String, Warehouse>) obj;
		}
	}

}
