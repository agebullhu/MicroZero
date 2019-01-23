function ws_state(active) {

}

var ws = function(option) {
    var that = this;
    that.sub = option.sub;
    that.user_close = false;
    that.state = "+open";
    that.addr = option.address;
    that.process = option.onmessage;
    that.open = function () {
        that.state = "+open";
        that.user_close = false;
        console.log("try open " + that.addr);
        that.socket = new WebSocket(that.addr);
        that.socket.onopen = that.onopen;
        that.socket.onclose = that.onclose;
        that.socket.onmessage = that.onmessage;
        that.socket.onerror = that.onerror;
    };
    that.close = function () {
        that.state = "-closing";
        that.user_close = true;
        that.socket.close();
        that.socket = null;
        console.log("close " + that.addr);
    };
    that.change_sub = function(title) {
        try {
            if (that.state === "+ok" && that.sub) {
                that.socket.send("-" + that.sub);
                console.log("-sub " + that.sub);
            }
            that.sub = title;
            if (!that.socket)
                that.open();
            else if (that.state === "+ok" && that.sub) {
                that.socket.send("+" + that.sub);
                console.log("+sub " + that.sub);
            }
        } catch (err) {
            console.log(err);
        }
    };

    that.onopen = function (e) {
        that.state = "+ok";
        console.log("opened " + that.addr);
        ws_state(true);
        if (!that.sub)
            return;
        console.log("sub " + that.sub);
        try {
            that.socket.send("+" + that.sub);
        } catch (err) {
            console.log(err);
            return;
        }
    };
    that.onclose = function(e) {
        ws_state(false);
        console.log(e);
        if (!that.user_close)
            window.setTimeout(that.open, 500);
        else
            that.state = "-close";
    };
    that.onmessage = function(e) {
        var data = null;
        try {
            if (typeof(e.data) != "string")
                return;
            data = eval('(' + e.data + ')');
            if (data)
                that.process(data);
        } catch (err) {
            console.log(err);
            return;
        }
    };
    that.onerror = function(err) {
        ws_state(false);
        console.log(err);
        that.state = "-error";
    };
    return that;
};

