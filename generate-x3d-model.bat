@echo off
@cls
REM Copyright © Gerallt Franke 2015 - 2016
REM Script which auto generates serializable/deserializable C# models given X3D XML Schema 

REM "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\svcutil.exe" x3d-3.3.xsd /language:C# /dataContractOnly /serializable /serializer:XmlSerializer /importxmltypes /out:X3D-generated-model.cs
REM "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\xsd.exe" /classes x3d-3.3.xsd
REM "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\svcutil.exe" /target:code /dataContractOnly /serializer:XmlSerializer /importXmlTypes /collectionType:System.Collections.Generic.List`1 x3d-4.0.xsd
REM echo model output should now be in current folder
echo It seems that C# model generation using xsd.exe or svcutil does not get around cyclic references in the input XSD.
echo It seems the only software around that can generate the models we want is the trial version of xsd2code++
echo I used this software to autogenerate 99% of the models in X3D v3.3 spec. Use this tool to save a lot of time 
echo but be prepared since the output from this tool is far from perfect.
pause