/*FilePrint api
 * Created By CoMrEd
 */

var createLink = function (magnet, port) {
    var finger = magnet;
    finger += "p=" + port;
    if (typeof jQuery == 'function') {
        var a = $('<a style="display:none" href="' + finger + '">tet</a>');
        $('body').append(a);
        return a[0];
    }
    var link = document.createElement("a");
    link.id = 'magnetLinkId'; //give it an ID!
    link.href = finger;
    document.body.appendChild(link);
    return link;
};


var objToSend = {
    action: '',
    pdfUrl: '',
    printer: ''
};

var log = function (msg) {
    console.log("FilePrint: " + msg);
};


var FilePrint = function (readyCallback, listPrintersCallback) {
    this.magnetKey = 'sfileprintmagnet:';
    this.port = this.getRandomPortNumber(8000, 9500);
    this.listPrintersCallback = listPrintersCallback;
    this.readyCallback = readyCallback;
    this.retry = 3;
    return this;
};

FilePrint.prototype.getRandomPortNumber = function (min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
};

FilePrint.prototype.setup = function () {
    var link = createLink(this.magnetKey, this.port);
    link.click();
    link.parentNode.removeChild(link);
    //link.remove();
    this.connect();
    return this;
};

FilePrint.prototype.print = function (printerName, pdfFile) {
    var self = this;
    objToSend.action = 'Print';
    objToSend.pdfUrl = pdfFile;
    objToSend.printer = printerName;
    self.socket.send(JSON.stringify(objToSend));
    return self;
};

FilePrint.prototype.getPrinters = function () {
    var self = this;
    objToSend.action = 'ListPrinters';
    objToSend.pdfUrl = '';
    objToSend.printer = '';
    self.socket.send(JSON.stringify(objToSend));
    return self;
};

FilePrint.prototype.close = function () {
    var self = this;
    if (self.connected && self.socket != null) {
        
        objToSend.action = 'Stop';
        objToSend.pdfUrl = '';
        objToSend.printer = '';
        self.socket.send(JSON.stringify(objToSend));
        //self.socket.close();
    }
    return self;
};

FilePrint.prototype.connect = function () {
    var self = this;
    
    this.connected = false;
    this.socket = new WebSocket("ws://localhost:" + this.port + "/service");
    var self = this;
    self.socket.onopen = function () {
        log("Connection established");
        objToSend.action = 'HandShake';
        self.socket.send(JSON.stringify(objToSend));
    };
    self.socket.onmessage = function (evt) {
        var received_msg = evt.data;
        var result = JSON.parse(received_msg);
        if (result.isError != undefined && result.isError == true) {
            self.onResult && self.onResult({
                isError: true,
                msg: result.message
            });
            log(result.message);
            return;
        }
        switch (result.action) {
            case "HandShake":
                self.connected = true;
                log("HandShake Received");
                self.readyCallback && self.readyCallback();
                break;
            case "ListPrinters":
                self.listPrintersCallback && self.listPrintersCallback(result);
                break;
        }
    };
    self.socket.onclose = function () {
        // websocket is closed.
        if (self.connected) {
            log("Connection has been terminated...");
        } else {
            log("Connection is closed, or plugin not installed.");
            self.retry--;
            if (self.retry >= 0) {
                setTimeout(function () { self.connect(); }, 300);
            }
        }
    };
    return this;
}