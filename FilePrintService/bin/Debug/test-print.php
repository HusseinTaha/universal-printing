<!DOCTYPE html>
<html>
<head>
	<title>page to print</title>
</head>
<body>


page to print web tst send to printer.

<br/>
<a href="fileprintmagnet:url_=http://localhost:7665/test-print.php=+=printerName_=dell">Print to Dell printer</a>
<br/>
<a href="fileprintmagnet:url_=http://localhost:7665/test-print.php=+=printerName_=BILL">Print to BILl printer</a>

<script type="text/javascript" src="fileprint.api.min.js?v=2"></script>

<script type="text/javascript">
	
	var fileprint = new FilePrint('Dell 1130 Laser Printer', '<strong>htmelfd f sf sf s fs f sf sf sfsfsf</strong>');
	fileprint.connect();
	
</script>

</body>
</html>