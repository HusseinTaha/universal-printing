/*FilePrint api
 * Created By CoMrEd
 */

var createLink = function (magnet, port, timeout) {
    var finger = magnet;
    finger += "p=" + port;
    finger += "&timeout=" + timeout;
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
    fileUrl: '',
    printer: '',
    extension: '',
    dataSTR: null,
    info: {},
    type: ''
};

var log = function (msg) {
    console.log("FilePrint: ", msg);
};


var FilePrint = function (timeout, readyCallback, onErrorResult) {
    this.magnetKey = 'ufileprintmagnet:';
    this.port = this.getRandomPortNumber(8000, 9500);
    this.onErrorResult = onErrorResult;
    this.readyCallback = readyCallback;
    this.retry = 3;
    this.timeout = timeout;
    return this;
};

FilePrintTypes = {};
FilePrintTypes.PDF = "PDF";
FilePrintTypes.HTML = "HTML";
FilePrintTypes.WORD = "WORD";
FilePrintTypes.IMAGE = "IMAGE";

FilePrint.prototype.initSendObj = function () {
    objToSend.action = '';
    objToSend.fileUrl = '';
    objToSend.printer = '';
    objToSend.extension = '';
    objToSend.type = '';
    objToSend.info = {};
    objToSend.dataSTR = null;
};

FilePrint.prototype.getRandomPortNumber = function (min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
};

FilePrint.prototype.setup = function () {
    var self = this;
    var link = createLink(this.magnetKey, this.port, this.timeout);
    link.click();
    link.parentNode.removeChild(link);
    //link.remove();
    setTimeout(function () { self.connect() }, 200);
    return this;
};

FilePrint.prototype.printPDF = function (printerName, fileUrl, useRaw) {
    var self = this;
    return self.print(printerName, fileUrl, "pdf", FilePrintTypes.PDF, { useRaw: useRaw });
};

FilePrint.prototype.printHTML = function (printerName, fileUrl, useRaw, useBrowserPrint, margins) {
    var self = this;
    return self.print(printerName, fileUrl, "html", FilePrintTypes.HTML,
        { useRaw: useRaw, useBrowserPrint: useBrowserPrint, margins: margins });
};

FilePrint.prototype.printIMAGE = function (printerName, fileUrl, extension, useRaw, width, height) {
    var self = this;
    return self.print(printerName, fileUrl, extension, FilePrintTypes.IMAGE,
        { useRaw: useRaw, width: width, height: height });
};

FilePrint.prototype.printWORD = function (printerName, fileUrl, extension, useRaw) {
    var self = this;
    return self.print(printerName, fileUrl, extension, FilePrintTypes.WORD,
        { useRaw: useRaw });
};

FilePrint.prototype.print = function (printerName, fileUrl, extension, type, info) {
    var self = this;
    self.initSendObj();
    objToSend.action = 'Print';
    objToSend.fileUrl = fileUrl;
    objToSend.printer = printerName;
    objToSend.extension = extension;
    objToSend.type = type;
    objToSend.info = info;

    var xhr = new XMLHttpRequest();
    xhr.open('GET', fileUrl, true);
    xhr.responseType = 'blob';

    xhr.onload = function (e) {
        if (this.status == 200) {
            // get binary data as a response
            blobUtil.blobToBase64String(this.response).then(function (base64str) {
                // success
                objToSend.dataSTR = base64str;
                self.socket.send(JSON.stringify(objToSend));
            }).catch(function (err) {
                // error
            });
        }
    };

    xhr.send();

    //self.socket.send(JSON.stringify(objToSend));
    return self;
};

FilePrint.prototype.getPrinters = function (listPrintersCallback) {
    var self = this;
    self.initSendObj();
    self.listPrintersCallback = listPrintersCallback;
    objToSend.action = 'ListPrinters';
    self.socket.send(JSON.stringify(objToSend));
    return self;
};

FilePrint.prototype.close = function () {
    var self = this;
    if (self.connected && self.socket != null) {
        self.initSendObj();
        objToSend.action = 'Stop';
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
            self.onErrorResult && self.onErrorResult(result);
            log(result);
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