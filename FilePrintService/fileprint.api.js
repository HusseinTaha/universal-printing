/*FilePrint api
 * Created By CoMrEd
 */
var FilePrint = function (printerName, data) {

    this.printerName = printerName;
    this.data = data;
    this.port = 9878;
};

var objToSend = {
    action: '',
    htmlData: '',
    printer: ''
};

var log = function (msg) {
    console.log("FilePrint: " + msg);
};

FilePrint.prototype.connect = function () {
    objToSend.action = 'HandShake';
    objToSend.htmlData = '';
    objToSend.printer = this.printerName;
    this.connected = false;
    this.socket = new WebSocket("ws://localhost:" + this.port + "/service");
    var self = this;
    self.socket.onopen = function () {
        log("Connection established");
        self.socket.send(JSON.stringify(objToSend));
    };
    self.socket.onmessage = function (evt) {
        var received_msg = evt.data;
        var result = JSON.parse(received_msg);
        if (result.isError) {
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

                objToSend.action = 'Print';
                objToSend.htmlData = self.data;
                objToSend.printer = self.printerName;
                self.socket.send(JSON.stringify(objToSend));
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
}