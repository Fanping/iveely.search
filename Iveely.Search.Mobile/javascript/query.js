//--------------------变量窗体消息----------------------------//
var searchAddress = "ws://www.iveely.com:9101/";

var searchSocket;

function setCookie(name, value, expires) {
    var x = name + "=" + escape(value);
    if (expires) {
        var d = new Date();
        d.setTime(d.getTime() + expires * 24 * 3600 * 1000);
        x += "; Expires=" + d.toGMTString();
    }
    document.cookie = x;
}

function getCookie(name) {
    var a = document.cookie.split("; ");
    name += "=";
    for (var i = 0; i < a.length; i++) if (a[i].indexOf(name) == 0) return unescape(a[i].substr(name.length));
    return "";
}

function getDiffTime(date) {
    var crawlDate = new Date(date);
    var currentDate = new Date();
    var diff = (currentDate.getTime() - crawlDate.getTime()) / 1000 * 60;
    if (diff < 60) {
        return diff + "min前";
    }
    return "";
}

function processPageSearch(records) {
    var result = "";
    var isFisrtResult = true;
    for (var i = 0; i < records.length; i++) {
        var pageSet = records[i] + "";
        var pageId = pageSet.substr(pageSet.indexOf("[PAGEID]:") + 9, pageSet.indexOf("[TITLE]:") - 12);
        if (pageId == "") {
            continue;
        }
        var pageQuery = pageSet.substr(pageSet.indexOf("[QUERY]:") + 8, pageSet.indexOf("[FROMSERVER]:") - pageSet.indexOf("[QUERY]:") - 8).replace("\n", "");
        // This is wiki.
        if (isFisrtResult) {
            var pageAbs = pageSet.substr(pageSet.indexOf("[ABSTRACT]:") + 11, pageSet.indexOf("[DATE]:") - pageSet.indexOf("[ABSTRACT]:") - 11);
            if (pageId == '-2') {
                var answerResult = "<div style='border: 1px solid #B210B6;'><div class=\"AnswerResult\">";
                answerResult += pageQuery + ": <strong><font style=\"color:#B210B6\">" + pageAbs + "</font></strong></div></div>";
                var answerContent = document.getElementById('answer');
                answerContent.innerHTML = answerResult;
                isFisrtResult = false;
                continue;
            }
        }

        // Clean answer.
        if (isFisrtResult && pageId != '-2') {
            var answerContent = document.getElementById('answer');
            answerContent.innerHTML = "";
        }

        var pageTitle = pageSet.substr(pageSet.indexOf("[TITLE]:") + 8, pageSet.indexOf("[URL]:") - pageSet.indexOf("[TITLE]:") - 8) + "";
        var pageUrl = pageSet.substr(pageSet.indexOf("[URL]:") + 6, pageSet.indexOf("[IMAGEURL]:") - pageSet.indexOf("[URL]:") - 6);
        var pageImage = pageSet.substr(pageSet.indexOf("[IMAGEURL]:") + 11, pageSet.indexOf("[ABSTRACT]:") - pageSet.indexOf("[IMAGEURL]:") - 11);
        var pageAbs = pageSet.substr(pageSet.indexOf("[ABSTRACT]:") + 11, pageSet.indexOf("[DATE]:") - pageSet.indexOf("[ABSTRACT]:") - 11);
        var pageDate = pageSet.substr(pageSet.indexOf("[DATE]:") + 7, pageSet.indexOf("[CRAWLDATE]:") - pageSet.indexOf("[DATE]:") - 7);
        var pageCrawlDate = pageSet.substr(pageSet.indexOf("[CRAWLDATE]:") + 12, pageSet.indexOf("[QUERY]:") - pageSet.indexOf("[CRAWLDATE]:") - 13);

        var pageServer = pageSet.substr(pageSet.indexOf("[FROMSERVER]:") + 13, pageSet.indexOf("[WEIGHT]:") - pageSet.indexOf("[FROMSERVER]:") - 13);
        var pageRank = pageSet.substr(pageSet.lastIndexOf("[WEIGHT]:") + 9, pageSet.length - pageSet.lastIndexOf("[WEIGHT]:") - 9);
        var record = "";
        record += "  <li class=\"record\" id='" + pageId + "'> ";
        record += "      <h3 class=\"title\">";
        if (pageImage.length > 10) {
            record += "<img src='" + pageImage + "' onload='javascript:drawImage(this,14,14)' />";
        }
        record += "          <a href='PageJump.html?from=" + pageServer + "&id=" + pageId + "&url=" + pageUrl + "&q=" + pageQuery + "' target=\"_blank\">" + pageTitle + "</a>";
        record += "       </h3>";
        record += "       <div class=\"desc\">";
        if (pageDate != "") {
            record += "    <span class=\"date\">" + pageDate + "</span>";
        }
        record += "      " + pageAbs + "</div>";
        var tempUrl = (pageUrl.length > 35 ? pageUrl.substr(0, 35) + '...' : pageUrl);
        tempUrl = tempUrl.replace(pageQuery, "<strong>" + pageQuery + "</strong>");
        record += "       <div><span class=\"url\">" + tempUrl + "</span>";
        //record += "<span class=\"date\">&nbsp;" + pageServer + "</span>";
        //record += "<span class=\"date\">&nbsp;权值:" + pageRank + "</span>";
        //record += "<a href='snapshot.html?from=" + pageServer.replace(":", "_") + "&id=" + pageId.replace("[", "") + "&q=" + pageQuery + "' class=\"snapshot\"target=\"_blank\">正文快照</a></div></li>";
        result += record.replace("\n", "") + "";
        isFisrtResult = false;
    }
    var content = document.getElementById('search_result');
    if (result != "") {
        content.innerHTML = result;
        if (window.localStorage) {
            localStorage.setItem("text_" + pageQuery, result);
            setCookie("text_" + pageQuery, "0123456", 2);
        }
    } else {
        content.innerHTML = "";
    }
}

//--------------------WebScoket相关----------------------------//
function onOpen(evt) {
    loggerWrite("CONNECTED");
};

function onClose(evt) {
    loggerWrite("DISCONNECTED");
};

function onSearchMessage(evt) {
    var responseData = evt.data;

    // 1. Is page search
    if (responseData[0] == '1') {
        var pageRecords = responseData.split("[PAGERECORD]");
        if (pageRecords.length > 0) {
            processPageSearch(pageRecords);
            return;
        }
        var content = document.getElementById('search_result');
        content.innerHTML = responseData.replace("1_", "");
    }

    // 2. Is image search
    if (responseData[0] == '2') {
        var imageRecords = responseData.split("[IMAGERECORD]");
        if (imageRecords.length > 1) {
            processImageSearch(imageRecords);
            return;
        }
        var imageDiv = document.getElementById('ImageResult');
        imageDiv.innerHTML = responseData.replace("2_", "");
    }
    if (responseData[0] == '3') {
        var content = document.getElementById('search_result');
        content.innerHTML = responseData;
    }
};

function onError(evt) {
    //alert(evt.data);
};

function sendSearchMessage(message) {
    loggerWrite("SENT: " + message);
    if (searchSocket != null) {
        try {
            searchSocket.send(message);
            return true;
        } catch (e) {
            loggerWrite(e);
        }
    } else {
      initWebSocket();
    }

    return false;
};

function loggerWrite(message) {
    //alert(message);
};

//--------------------窗体消息----------------------------//
window.onload = function (evt) {
    initWebSocket();
};

function initWebSocket() {
    searchSocket = new WebSocket(searchAddress);
    searchSocket.onopen = function (evt) {
        onOpen(evt)
    };
    searchSocket.onclose = function (evt) {
        onClose(evt)
    };
    searchSocket.onmessage = function (evt) {
        onSearchMessage(evt)
    };
    searchSocket.onerror = function (evt) {
        onError(evt)
    };
}

window.onunload = function (evt) {
    if (searchSocket) {
        searchSocket.close();
    }
}

//--------------------智能提示相关----------------------------//
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

function sendQuery(query) {
    scroll(0, 0);
    if (query != '') {
        // Text search local store.
        if (window.localStorage && getCookie("xtext_" + query).length > 5) {
            var textData = localStorage.getItem("text_" + query);
            if (textData != null && textData.length > 100) {
                var content = document.getElementById('search_result');
                content.innerHTML = textData;
            } else {
                sendSearchMessage('[SEARCH]:' + query);
            }
        } else {
            sendSearchMessage('[SEARCH]:' + query);
        }
      
        document.title = query + " | Iveely 搜索结果";
    }
}

function drawImage(ImgD, iwidth, iheight) {
    //参数(图片,允许的宽度,允许的高度)   
    var image = new Image();
    image.src = ImgD.src;
    if (image.width > 0 && image.height > 0) {
        if (image.width / image.height >= iwidth / iheight) {
            if (image.width > iwidth) {
                ImgD.width = iwidth;
                ImgD.height = (image.height * iwidth) / image.width;
            } else {
                ImgD.width = image.width;
                ImgD.height = image.height;
            }
        } else {
            if (image.height > iheight) {
                ImgD.height = iheight;
                ImgD.width = (image.width * iheight) / image.height;
            } else {
                ImgD.width = image.width;
                ImgD.height = image.height;
            }
        }
    }
}