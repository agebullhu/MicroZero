



function formatDate(date, fmt) {
    if (!date || isNaN(date.getMonth()))
        return '-';
    if (!fmt)
        fmt = 'yyyy-MM-dd hh:mm:ss';
    if (/(y+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, (date.getFullYear() + '').substr(4 - RegExp.$1.length));
    }
    var o = {
        'M+': date.getMonth() + 1,
        'd+': date.getDate(),
        'h+': date.getHours(),
        'm+': date.getMinutes(),
        's+': date.getSeconds()
    };
    for (var k in o) {
        if (o.hasOwnProperty(k)) {
            if (new RegExp(`(${k})`).test(fmt)) {
                var str = o[k] + '';
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length === 1) ? str : padLeftZero(str));
            }
        }
    }
    return fmt;
}

function padLeftZero(str) {
    return ('00' + str).substr(str.length);
}



/**
 * /千分符小数表示法
 * @param {any} num 数字
 * @return {string} 格式化好的文本
 */
function toThousands(num) {
    if (!num)
        return "0.00";
    num = num.toString();
    if (!num)
        return "0.00";

    var result;

    var p = num.indexOf(".");
    if (p >= 0) {
        result = num.substr(p, num.length - p);
        num = num.substring(0, p);
    } else {
        result = ".00";
    }
    while (num.length > 3) {
        result = "," + num.slice(-3) + result;
        num = num.slice(0, num.length - 3);
    }
    if (num) {
        result = num + result;
    }
    return result;
}

/**
 * /千分符整数表示法
 * @param {any} num 数字
 * @return {string} 格式化好的文本
 */

function toThousandsInt(num) {
    if (!num)
        return "0";
    num = num.toString();
    if (!num)
        return "0";

    var result;

    var p = num.indexOf(".");
    if (p >= 0) {
        result = num.substr(p, num.length - p);
        num = num.substring(0, p);
    } else {
        result = "";
    }
    while (num.length > 3) {
        result = "," + num.slice(-3) + result;
        num = num.slice(0, num.length - 3);
    }
    if (num) {
        result = num + result;
    }
    return result;
}