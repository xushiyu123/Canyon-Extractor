# Canyon-Extractor

extract river canyons using dem and vector river data

#CanyonExtractor

CanyonExtractor is a WinForm project in C# and was interpreted with Visual Studio 2019.

Arcgis Engine 10.2 and windows 64bit is needed when running this project

#RiverFIle

river.shp is an sample data of vector river data.

#Spring 

Spring is a web service package which supplies elevation data to CanyonExtractor.

Tomcat 8.5 is needed and You can put Spring in Tomcat webapp folder.

then start up tomcat, and CanyonExtractor is able to get elevation from Spring.

dem file must be in txt form and named as dem30.

you can use some GIS software (e.g: ArcGIS, MapGIS) to translate dem files in other forms (e.g: tiff, img) into ASCII form (txt).


