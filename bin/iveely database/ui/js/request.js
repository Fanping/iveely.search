var uiAddress = "ws://127.0.0.1:4322/";
var uiSocket;

window.onload = function (evt) {
    initWebSocket();
};

window.onunload = function (evt) {
    if (uiSocket) {
        uiSocket.close();
    }
}

function getQueryString(name) {
    if (location.href.indexOf("?") == -1 || location.href.indexOf(name + '=') == -1) {
        return '';
    }

    var queryString = location.href.substring(location.href.indexOf("?") + 1);
    var parameters = queryString.split("&");
    var pos, paraName, paraValue;
    for (var i = 0; i < parameters.length; i++) {
        pos = parameters[i].indexOf('=');
        if (pos == -1) {
            continue;
        }

        paraName = parameters[i].substring(0, pos);
        paraValue = parameters[i].substring(pos + 1);
        if (paraName == name) {
            return unescape(decodeURI(paraValue).replace(/\+/g, " "));
        }
    }
    return '';
};

function initWebSocket() {
    uiSocket = new WebSocket(uiAddress);
    uiSocket.onopen = function (evt) {
        onOpen(evt)
    };
    uiSocket.onclose = function (evt) {
        onClose(evt)
    };
    uiSocket.onmessage = function (evt) {
        onMessage(evt)
    };
    uiSocket.onerror = function (evt) {
        onError(evt)
    };
}

function buildCommand(command, database) {
    var mixQuery = "{ \"command\":\"" + command + "\",";
    mixQuery += "\"database\":\"" + database + "\"}";
    return mixQuery;
}

function sendCommand(command, database) {
    if (uiSocket != null) {
        try {
            var data = buildCommand(command, database);
            uiSocket.send(data);
            return true;
        } catch (e) {
            alert(e);
        }
    }
    return false;
};

function showDatabase() {
    sendCommand('show dbs', 'system');
}

function showTables(dbName) {
    sendCommand('show tbs', dbName);
}

function dropTable(tableName) {
    var dbName = getQueryString("name");
    sendCommand('drop', dbName+":"+tableName);
}

function flush() {
    var dbName = getQueryString("name");
    sendCommand('flush', dbName);
}

function onOpen(evt) {
    var dbName = getQueryString("name");
    if (dbName.length == 0) {
        showDatabase();
    } else {
        showTables(dbName);
    }
};

function onClose(evt) {

};

function onMessage(evt) {
    var responseData = evt.data;
    var jsonObject = new Function("return " + responseData)();
    var type = jsonObject.command;
    if (type == "show dbs") {
        var size = jsonObject.dbs.length;
        var totalData = "";
        for (var i = 0; i < size; i++) {
            var data = "<tr>";
            data += "<td><a href='database.html?name=" + jsonObject.dbs[i] + "' >" + jsonObject.dbs[i] + "</a></td>";
            data += "</tr>";
            totalData += data;
        }
        var content = document.getElementById('topology-summary-tbody');
        content.innerHTML = totalData;
    }
    else if (type == "show tbs") {
        var size = jsonObject.names.length;
        var totalData = "";
        for (var i = 0; i < size; i++) {
            var data = "<tr>";
            data += "<td>" + jsonObject.names[i] + "</td>";
            data += "<td>" + jsonObject.counter[i] + "</td>";
            data += "<td><a href='javascript:void(0)' OnClick=\"javascript:dropTable('" + jsonObject.names[i] + "')\">Delete</a></td>";
            data += "</tr>";
            totalData += data;
        }
        var content = document.getElementById('task-summary-tbody');
        content.innerHTML = totalData;
    }
    else if(type == "drop") {
        window.location = window.location;
    }
};

function onError(evt) {

};