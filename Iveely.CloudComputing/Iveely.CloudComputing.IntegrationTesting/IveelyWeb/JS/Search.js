 function ShowResult(txt)
        {
            document.getElementById('content').innerHTML = " <ul class='results'><li class='record'>"+txt+"</li><ul>";
			GetQuery('query');
        };

function GetQuery(query)
        {
            var name, value, i;
            var str = location.href;
            var num = str.indexOf("?")
            str = str.substr(num + 1);
            var arrtmp = str.split("&");

            for (i = 0; i < arrtmp.length; i++) {
                num = arrtmp[i].indexOf("=");
                if (num > 0) {
                    name = arrtmp[i].substring(0, num);
                    value = arrtmp[i].substr(num + 1);
                    if (name == query) 
					{
						document.getElementById('q1').value = value;
                    }
                }
            }
        };		