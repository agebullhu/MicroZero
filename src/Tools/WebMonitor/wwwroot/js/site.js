// Write your Javascript code.
/**
 * 更新到vue数组
 * @param {any} array 数组
 * @param {any} item 节点
 * @param {any} key 主键
 */
function vue_update_array(array, item, key) {
    for (var idx = 0; idx < array.length; idx++) {
        var old = array[idx];
        if (old[key] === item[key]) {
            Vue.set(array, idx, item);
            return;
        }
    }
    array.push(item);
}
/**
 * ajax get 操作
 * @param {any} url 地址
 * @param {any} vue_obj vue快捷对象
 */
function do_sync_get(url, vue_obj, job, callback) {
    $.get(url, function (data) {
        console.log(data);
        if (data.success) {
            vue_obj.$notify({
                title: job,
                message: '操作成功',
                type: 'success',
                position: 'bottom-left',
                duration: 2000
            });
            if (callback)
                callback(data);
        }
        else {
            vue_obj.$notify({
                title: job,
                message: data.status.msg,
                type: 'warning',
                position: 'bottom-left',
                duration: 2000
            });
        }
    }).error(function (e) {
        vue_obj.$notify.error({
            title: job,
            message: '操作异常',
            position: 'bottom-left',
            duration: 2000
        });
    });
}

function formatDate(date, fmt) {
    if (isNaN(date.getMonth()))
        return date;
    if (!fmt)
        fmt= 'yyyy-MM-dd hh:mm:ss'
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
 * /千分符数字表示法
 */
function toThousands(num) {
    if (!num || num == "")
        return "0.00";
    num = num.toString();
    if (num == "")
        return "0.00";

    var result;

    var ls;
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
 * /千分符数字表示法
 */
function toThousandsInt(num) {
    if (!num || num == "")
        return "0";
    num = num.toString();
    if (num == "")
        return "0";

    var result;

    var ls;
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