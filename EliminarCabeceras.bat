@echo off
rem Elimina las cabeceras de las tablas
set CAT=.\bin\cat.exe
set MV=.\bin\mv.exe -f
set TMP=data\tmp.csv
set ETLGEN=.\bin\Debug\ETLGen.exe

%CAT% data\edificio.csv | %ETLGEN% > %TMP%
%MV% %TMP% data\edificio.csv

%CAT% data/aula.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/aula.csv

%CAT% data/departamento.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/departamento.csv

%CAT% data/area.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/area.csv

%CAT% data/examen.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/examen.csv

%CAT% data/pais.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/pais.csv

%CAT% data/provincia.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/provincia.csv

%CAT% data/ciudad.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/ciudad.csv

%CAT% data/colegioBUP.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/colegioBUP.csv

%CAT% data/colegioCOU.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/colegioCOU.csv

%CAT% data/colegioEGB.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/colegioEGB.csv

%CAT% data/colegioFP.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/colegioFP.csv

%CAT% data/persona.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/persona.csv

%CAT% data/expediente.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/expediente.csv

%CAT% data/matricula.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/matricula.csv

%CAT% data/autor.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/autor.csv

%CAT% data/biblioteca.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/biblioteca.csv

%CAT% data/libro.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/libro.csv

%CAT% data/doctorado.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/doctorado.csv

%CAT% data/carrera.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/carrera.csv

%CAT% data/profesor.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/profesor.csv

%CAT% data/asignatura.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/asignatura.csv

%CAT% data/franjahoraria.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/franjahoraria.csv

%CAT% data/abstencion.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/abstencion.csv

%CAT% data/asistencia.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/asistencia.csv

%CAT% data/evaluacion.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/evaluacion.csv

%CAT% data/librosestudiados.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/librosestudiados.csv

%CAT% data/librosprestados.csv | %ETLGEN% > %TMP%
%MV% %TMP% data/librosprestados.csv