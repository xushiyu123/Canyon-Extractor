# 1.1 Introduction

## 1.1 Project

	Canyon-Extractor automatically extracts river canyons using digial elevation data (DEM) and vector river data.

Input data: vector river data (in shapefile format) and a web service packed using tomact (URL).

Output data: river canyons (in shapefile format).

Next part aims to intoduce the packages.

## 1.2 Packages

### 1.2.1 CanyonExtractor

	CanyonExtractor is a WinForm project in C# and was interpreted with Visual Studio (2012 and versions above).

Arcgis Engine 10.2 and windows 64bit is needed when running this project

### 1.2.2 RiverFile
	river.shp is an sample data of vector river data.

### 1.2.3 Spring 

	Spring is a web service package which supplies elevations to CanyonExtractor.

Tomcat 8.5 is needed and you must put the whole Spring package into Tomcat webapp folder. The url of web service for elevations will be [tomcat root url]/Spring/api/elevation/linestring (default url is: http://127.0.0.1:8080/Spring/api/elevation/linestring).
the tomcat download url:https://tomcat.apache.org/download-80.cgi#8.5.45; the follwing is the process of packing this folder.

(1) Download tomcat;

(2) Copy the whole Spring folder and put it into [tomcat root folder]/webapps/;

(3) If you want to use your own DEM, put your dem file into [tomcat root folder]/webapps/Spring/WEB-INF/dems/ to replace the experimental data; To avoid using some third party librarys, dem file must be converted into txt form ,and must be named as dem30 (to be identified by the web service). You can use some GIS software (e.g: ArcGIS, MapGIS) to translate dem files in other forms (e.g: tiff, img) into ASCII form (txt).

(4) Start up tomcat;

(4) visit the url of this service via web browser and ensure the service runs successfully.

![image](https://github.com/xushiyu123/Canyon-Extractor/blob/master/Figs/Fig3.png)

To avoid using some third party librarys, dem file must be converted into txt form ,and must be named as dem30 (to be identified by the web service). You can use some GIS software (e.g: ArcGIS, MapGIS) to translate dem files in other forms (e.g: tiff, img) into ASCII form (txt).

### 1.2.3 Figs
	some screenshots of the program.

# 2. Operation instructions

## 2.1 GUI
![image](https://github.com/xushiyu123/Canyon-Extractor/blob/master/Figs/Fig1.png)

## 2.2 Operation
2.2.1 click the first "+" button to browse files and select your Vector river data;

2.2.2 click the second "+" button to browse folders and select an empty folder to save the output data;

2.2.3 Fill the name of output file in the textbox beside the label "Result name";

2.2.4 Fill an url in the textbox beside the label "Service" to determine thr web service for elevations;

2.2.5 Click the "Test" button to check the connection of the service, and a meessageBox will tells you whether the connection succeeds or fails;

![image](https://github.com/xushiyu123/Canyon-Extractor/blob/master/Figs/Fig2.png)

2.2.6 Fill a number in the textbox beside the label "WT" which sets the basic width of the river buffers, and WT of the experimental data is 1000 (a bit larger number will be OK either);

2.2.7 Cilck the first comboBox to choose a method of peak extracting;

2.2.8 Cilck the second comboBox to choose a method of peak mathcing;

2.2.9 At last, Cilck "Run" button to start the program, and a meessageBox will tells you whether the extraction succeeds or fails;

![image](https://github.com/xushiyu123/Canyon-Extractor/blob/master/Figs/Fig4.png)

2.2.10 Get the output data from the output folder.
