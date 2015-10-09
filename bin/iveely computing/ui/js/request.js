var uiAddress = "ws://127.0.0.1:9000/";
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

function buildCommand(command, topology, password) {
    var mixQuery = "{ \"command\":\"" + command + "\",";
    mixQuery += "\"password\":\"" + password + "\",";
    mixQuery += "\"topology\":\"" + topology + "\"}";
    return mixQuery;
}



function sendCommand(command, topology, password) {
    if (uiSocket != null) {
        try {
            var data = buildCommand(command, topology, password);
            uiSocket.send(data);
            return true;
        } catch (e) {
            alert(e);
        }
    }
    return false;
};

function showClusterSummary() {
    sendCommand('cluster summary', 'empty', 'empty');
};

function showTopologySummary() {
    sendCommand('topology summary', 'empty', 'empty');
};

function showSlaveSummary() {
    sendCommand('slave summary', 'empty', 'empty');
};

function showSystemSummary() {
    sendCommand('system summary', 'empty', 'empty');
};

function queryTolopology() {
    sendCommand('query topology', getQueryString('name'), 'empty');
};

function taskSummary() {
    sendCommand('task summary', getQueryString('name'), 'empty');
};

function statisticSummary() {
    sendCommand('statistic summary', getQueryString('name'), 'empty');
};

function killTopology() {
    var pwd = prompt("You want kill the toplogy? Enter your password:", "");
    if (pwd != null) {
        sendCommand('kill', getQueryString('name'),pwd);
    }
}

function rebalanceTopology() {
    var pwd = prompt("You want rebalance the toplogy? Enter your password:", "");
    if (pwd != null) {
        sendCommand('rebalance', getQueryString('name'), pwd);
    }
}

function onOpen(evt) {


    showClusterSummary();
    showTopologySummary();
    showSlaveSummary();
    showSystemSummary();

    queryTolopology();
    taskSummary();
    statisticSummary();

};

function onClose(evt) {

};

function onMessage(evt) {
    var responseData = evt.data;
    var jsonObject = new Function("return " + responseData)();
    var type = jsonObject.resType;
    if (type == "cluster summary") {
        var data = ""; //  "<tbody><tr>";
        data += "<td>" + jsonObject.version + "</td>";
        data += "<td>" + jsonObject.setupTime + "</td>";
        data += "<td>" + jsonObject.slaveCount + "</td>";
        data += "<td>" + jsonObject.usedSlotCount + "</td>";
        data += "<td>" + jsonObject.freeSlotCount + "</td>";
        data += "<td>" + jsonObject.totalSlotCount + "</td>";
        data += "<td>empty</td>";
        data += "<td>empty</td>";
        //data += "</tr></tbody>";
        var content = document.getElementById('cluster-summary-tbody');
        content.innerHTML = data;
    }
    else if (type == "topology summary" || type == "query topology") {
        var size = jsonObject.ZBuffer.length;
        var totalData = "";
        for (var i = 0; i < size; i++) {
            var data = "<tr>";
            data += "<td><a href='topology.html?name=" + jsonObject.ZBuffer[i].name + "' >" + jsonObject.ZBuffer[i].name + "</a></td>";
            data += "<td>" + jsonObject.ZBuffer[i].id + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].status + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].setupTime + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].inSlaveCount + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].threadCount + "</td>";
            data += "<td>empty</td>";
            data += "</tr>";
            totalData += data;
        }
        var content = document.getElementById('topology-summary-tbody');
        content.innerHTML = totalData;
    }
    else if (type == "slave summary") {
        var size = jsonObject.ZBuffer.length;
        var totalData = "";
        for (var i = 0; i < size; i++) {
            var data = "<tr>";
            data += "<td>" + jsonObject.ZBuffer[i].id + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].host + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].setupTime + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].slotsCount + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].runningApp + "</td>";
            data += "</tr>";
            totalData += data;
        }
        var content = document.getElementById('slave-summary-tbody');
        content.innerHTML = totalData;
    }
    else if (type == "system summary") {
        var data = "";
        data += "<tr><td>Zookeeper Server</td><td>" + jsonObject.zkServer + "</td></tr>";
        data += "<tr><td>Zookeeper Port</td><td>" + jsonObject.zkPort + "</td></tr>";
        data += "<tr><td>Master Server</td><td>" + jsonObject.masterServer + "</td></tr>";
        data += "<tr><td>Master Port</td><td>" + jsonObject.masterPort + "</td></tr>";
        data += "<tr><td>Slot Count / slave</td><td>" + jsonObject.slotCount + "</td></tr>";
        data += "<tr><td>Slave Root Path</td><td>" + jsonObject.slaveRoot + "</td></tr>";
        data += "<tr><td>Master Root</td><td>" + jsonObject.masterRoot + "</td></tr>";
        data += "<tr><td>Max Worker Count</td><td>" + jsonObject.maxWorkerCount + "</td></tr>";
        var content = document.getElementById('system-summary-tbody');
        content.innerHTML = data;
    }
    else if (type == "task summary") {
        var size = jsonObject.ZBuffer.length;
        var totalData = "";
        for (var i = 0; i < size; i++) {
            var data = "<tr>";
            data += "<td>" + jsonObject.ZBuffer[i].guid + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].type + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].connectPath + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].exception + "</td>";
            data += "</tr>";
            totalData += data;
        }
        var content = document.getElementById('task-summary-tbody');
        content.innerHTML = totalData;
    }
    else if (type == "statistic summary") {
        var size = jsonObject.ZBuffer.length;
        var totalData = "";
        for (var i = 0; i < size; i++) {
            var data = "<tr>";
            data += "<td>" + jsonObject.ZBuffer[i].guid + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].emitCount + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].emitAvg + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].executeCount + "</td>";
            data += "<td>" + jsonObject.ZBuffer[i].executeAvg + "</td>";
            data += "</tr>";
            totalData += data;
        }
        var content = document.getElementById('statistic-summary-tbody');
        content.innerHTML = totalData;
    }
    else if (type == "kill topology" || type == "rebalance topology") {
        alert(jsonObject.respData);
    }
};

function onError(evt) {

};