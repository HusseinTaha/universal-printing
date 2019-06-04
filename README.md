# Universal Printing with cross-browser feature

## Why is this so awesome? 🤩

* 📁 **List client machine printers** You can list the printers on the client machine, directly from web browser.
* 🔄 **Print directly to specific printer without dialogs** You can print the document directly to a specific printer, no need for any dialog.
* 🙌 **Print Word, PDF, images and html data** Many options to print for many type of files: PDF, DOCX, HTML, Jpeg, PNG
* 🚀 **Expandable to support many extensions** ...like using raw print to print any type of file.

## Get your web app ready 🚚

- ☑️ [**Simply include the api js script**](https://nextcloud.com/signup/) include the api-script js file.
- 🖥 [**Install** the setup package by yourself](https://nextcloud.com/install/#instructions-server) on the client machine.
- 📦 Call the needed functions for printing [awesome direct calls to print]

## Get in touch 💬

* [📋 Forum](https://fancyhints.com/forums/forum/forum/)
* [👥 Website](https://fancyhints.com/)


## Join the team 👪

There are many ways to contribute, of which development is only one! 


### Development setup 👩‍💻

1. 🚀 [Set up your local development environment](Visual studio C# 4.5, Wix toolset)
2. 🐛 [Pick a good first issue]
3. 👩‍🔧 Create a branch and make your changes. Remember to sign off your commits using `git commit -sm "Your commit message"`
4. ⬆ Create a [pull request](https://opensource.guide/how-to-contribute/#opening-a-pull-request) and `@mention` the people from the issue to review
5. 👍 Fix things that come up during review
6. 🎉 Wait for it to get merged!

### Building front-end code 🏗

``` bash
# install dependencies
Visual studio C# 4.5
Wix toolset.

# build for development
Viusual studio build

```

**When making changes, also commit the referenced files!**

Add all the references needed if you are replacing or adding new features.
```


### Tools we use 🛠

- [👀 Wix toolset](https://wixtoolset.org) for building the setup project.
- [🌊 Visual studio](https://visualstudio.microsoft.com/downloads) for visual studio IDE.

```
### Sample Code

``` bash
- Initializing the file print setup

  var fileprint = new FilePrint(0 /*timeout for closing*/, false /*useBinary for getting the file.*/, function ready() {

  }, function erroResult(error) {

  });
  fileprint.setup();

- Getting the printers

  fileprint.getPrinters(function listprinters(data) {
      fillPrinters(data.list);//method to create a select list with the printers.
  });

- Print to specific printer

  //getting the printer name from the dropdown.
  var e = document.getElementById("printersList");
  var strPrinterName = e.options[e.selectedIndex].value;

  //print pdf file.
  fileprint.printPDF(strPrinterName, 'http://localhost:7665/test.pdf', true/*raw-printing*/);
  //print html file or some inner html data
  fileprint.printHTML(strPrinterName, 'http://localhost:7665/test.html', false/*raw-printing*/, false, '-l 0.11 -r 0.05'/*margins of the paper*/);
  //print image.
  fileprint.printIMAGE(strPrinterName, 'http://localhost:7665/test.jpg', "jpg", true/*raw-printing*/, 200/*width*/, 100/*height*/);
  //print word file
  fileprint.printWORD(strPrinterName, 'http://localhost:7665/test.docx', "docx", false/*raw-printing*/);

- Close the connection once done printing.

  fileprint.close();

```

## Contribution guidelines 📜

Universal Printing doesn't require a CLA (Contributor License Agreement).
The copyright belongs to all the individual contributors. Therefore we recommend
that every contributor adds following line to the header of a file, if they
changed it substantially:

```
@copyright Copyright (c) <year>, <your name> (<your email address>)
```
