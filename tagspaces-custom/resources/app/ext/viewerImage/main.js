/* Copyright (c) 2013-2016 The TagSpaces Authors.
 * Use of this source code is governed by the MIT license which can be found in the LICENSE.txt file. */

/* globals Nanobar, marked, EXIF, Mousetrap  */
"use strict";

$(document).ready(function() {
  function getParameterByName(name) {
    name = name.replace(/[\[]/ , "\\\[").replace(/[\]]/ , "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)") ,
            results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g , " "));
  }

  var filePath = getParameterByName("cp");

  var imageRotationClass = "";
  var isCordova = parent.isCordova;
  var isWeb = document.URL.indexOf('http') === 0;

  if (isCordova || isWeb) {
    
  } else {
    filePath = "file://" + window.parent.global.require('fs').realpathSync(filePath);
  }


  var $imgViewer = $("#htmlContent");
  var exifObj;

  $("#imageContent").attr("src" , filePath).bind("load" , function() {
    $(this).addClass("transparentImageBackground");
    $imgViewer.addClass("imgViewer");
    if (filePath.toLowerCase().indexOf("jpg") === (filePath.length - 3) ||
            filePath.toLowerCase().indexOf("jpeg") === (filePath.length - 4)) {
      EXIF.getData(this , function() {
        var orientation = EXIF.getTag(this , "Orientation");
        correctOrientation(orientation);
        //console.log(EXIF.pretty(this));
        exifObj = {};
        var tags = ['Make' , 'Model' , 'DateTime' , 'Artist' , 'Copyright' , 'ExposureTime ' , 'FNumber' , 'ISOSpeedRatings' , 'ShutterSpeedValue' , 'ApertureValue' , 'FocalLength'];
        for (var tag in tags) {
          var prop = tags[tag];
          if (this.exifdata.hasOwnProperty(prop)) {
            exifObj[prop] = this.exifdata[prop];
          }
        }
        jQuery.extend(exifObj , this.iptcdata);
        if (!jQuery.isEmptyObject(exifObj)) {
          $("#exifButton").parent().show();
          printEXIF();
        }
      });
    }
  });

  function printEXIF() {
    var $exifRow = $("#exifRow").clone(); // Preparing the template
    var $exifTableBody = $("#exifTableBody");
    $exifTableBody.empty();
    for (var key in exifObj) {
      if (exifObj.hasOwnProperty(key) && exifObj[key].length !== 0) {
        $exifRow.find("th").text(key);
        $exifRow.find("td").text(exifObj[key]);
        $exifTableBody.append($exifRow.clone());
        //$exifTableBody.append("<tr><th>" + key + "</th><td>" + exifObj[key] + "</td></tr>");
      }
    }
  }

  $imgViewer.panzoom({
    $zoomIn: $("#zoomInButton") ,
    $zoomOut: $("#zoomOutButton") ,
    $reset: $("#zoomResetButton") ,
    minScale: 0.1 ,
    maxScale: 10 ,
    increment: 0.2 ,
    easing: "ease-in-out" ,
    contain: 'invert'
  }).parent().on('mousewheel.focal' , function(e) {
    e.preventDefault();
    var delta = e.delta || e.originalEvent.wheelDelta;
    var zoomOut = delta ? delta < 0 : e.originalEvent.deltaY > 0;
    $imgViewer.panzoom('zoom' , zoomOut , {
      increment: 0.1 ,
      focal: e ,
      animate: false
    });
  });

  function correctOrientation(orientation) {
    var $image = $("#imageContent");
    $image.removeClass(imageRotationClass);
    console.log("ORIENTATION: " + orientation);
    switch (orientation) {
      case 8:
        imageRotationClass = "rotate270";
        break;
      case 3:
        imageRotationClass = "rotate180";
        break;
      case 6:
        imageRotationClass = "rotate90";
        break;
      case 1:
        imageRotationClass = "";
        break;
      default:
        imageRotationClass = "";
    }
    $image.addClass(imageRotationClass);
  }

  $("#rotateLeftButton").on("click" , function() {
    //console.log("Rotate Left");
    var $image = $("#imageContent");
    $image.removeClass(imageRotationClass);
    switch (imageRotationClass) {
      case "":
        imageRotationClass = "rotate270";
        break;
      case "rotate270":
        imageRotationClass = "rotate180";
        break;
      case "rotate180":
        imageRotationClass = "rotate90";
        break;
      case "rotate90":
        imageRotationClass = "";
        break;
      default:
        imageRotationClass = "";
    }
    $image.addClass(imageRotationClass);
  });

  $("#rotateRightButton").on("click" , function() {
    //console.log("Rotate Right");
    var $image = $("#imageContent");
    $image.removeClass(imageRotationClass);
    switch (imageRotationClass) {
      case "":
        imageRotationClass = "rotate90";
        break;
      case "rotate90":
        imageRotationClass = "rotate180";
        break;
      case "rotate180":
        imageRotationClass = "rotate270";
        break;
      case "rotate270":
        imageRotationClass = "";
        break;
      default:
        imageRotationClass = "";
    }
    $image.addClass(imageRotationClass);
  });

  if (isCordova) {
    $("#zoomInButton").hide();
    $("#zoomOutButton").hide();
    $("#printButton").hide();
  }

  // Init internationalization
  $.i18n.init({
    ns: {
      namespaces: ['ns.viewerImage']
    } ,
    debug: true ,
    fallbackLng: 'en_US'
  } , function() {
    $('[data-i18n]').i18n();
  });

  // Nano progressbar
  $(function() {
    var options = {
      bg: '#42BEDB' , // (optional) background css property, '#000' by default
      // leave target blank for global nanobar
      target: document.getElementById('nanoBar') , //(optional) Where to put the progress bar, nanobar will be fixed to top of document if target is null
      // id for new nanobar
      id: 'nanoBar' // (optional) id for nanobar div
    };
    var nanobar = new Nanobar(options);
    var pct = 0;
    $(document).ajaxSend(function() {
      pct += 0.1;
      // move bar
      nanobar.go(pct);
      if (pct > 100.0) {
        pct = 0.0;
      }
    }).ajaxComplete(function() {
      // Finish progress bar
      nanobar.go(100);
    });
  });

});
